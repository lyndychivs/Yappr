namespace Yappr.Mcp.Ollama;

using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

using Yappr.Models;

/// <summary>
/// Exposes a "chat" MCP tool backed by a local Ollama server's <c>/api/generate</c> endpoint, matching the
/// contract <c>McpClientToolInvoker</c> expects: a single <c>prompt</c> argument, plain text back.
/// </summary>
/// <remarks>
/// Uses <see cref="IHttpClientFactory"/> directly (a named client) rather than a typed client, because the MCP
/// SDK constructs tool instances itself (via reflection) rather than resolving them through DI — a typed
/// <c>HttpClient</c> constructor parameter would silently receive an unconfigured <c>new HttpClient()</c>.
/// </remarks>
[McpServerToolType]
public sealed class OllamaChatTool(IHttpClientFactory httpClientFactory, IOptions<OllamaOptions> options)
{
    internal const string HttpClientName = nameof(OllamaChatTool);

    private readonly OllamaOptions options = options.Value;

    [McpServerTool(Name = "chat")]
    [Description("Summarises the given prompt.")]
    public async Task<string> Chat(string prompt, CancellationToken cancellationToken)
    {
        var request = new OllamaGenerateRequest(options.Model, prompt, Stream: false);

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);

        using HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("/api/generate", request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Ollama returned {(int)response.StatusCode} {response.StatusCode}: {body}"));
        }

        OllamaGenerateResponse? result = await response.Content
            .ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken)
            .ConfigureAwait(false);

        return result?.Response
            ?? throw new InvalidOperationException("Ollama did not return a response.");
    }
}
