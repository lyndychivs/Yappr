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
/// Exposes a "chat" MCP tool backed by a local Ollama server's <c>/api/generate</c> endpoint.
/// </summary>
/// <remarks>
/// Uses <see cref="IHttpClientFactory"/> directly because the MCP SDK constructs tools via reflection, not
/// DI, so a typed <c>HttpClient</c> parameter would get an unconfigured instance.
/// </remarks>
[McpServerToolType]
public sealed class OllamaChatTool(IHttpClientFactory httpClientFactory, IOptions<OllamaOptions> options)
{
    /// <summary>
    /// The name of the named <see cref="HttpClient"/> used to reach the Ollama server.
    /// </summary>
    internal const string HttpClientName = nameof(OllamaChatTool);

    private readonly OllamaOptions _options = options.Value;

    /// <summary>
    /// Summarises <paramref name="prompt"/> via the configured Ollama model.
    /// </summary>
    /// <param name="prompt">The prompt to summarize.</param>
    /// <param name="cancellationToken">A token to cancel the call.</param>
    /// <returns>The generated summary text.</returns>
    [McpServerTool(Name = "chat")]
    [Description("Summarises the given prompt.")]
    public async Task<string> Chat(string prompt, CancellationToken cancellationToken)
    {
        var request = new OllamaGenerateRequest(_options.Model, prompt, Stream: false);

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
