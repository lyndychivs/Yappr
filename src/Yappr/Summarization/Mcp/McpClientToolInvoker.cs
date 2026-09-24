namespace Yappr.Summarization.Mcp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using Yappr.Models;

/// <summary>
/// Invokes the configured MCP server's summarization tool, opening a new client connection per call.
/// </summary>
public sealed class McpClientToolInvoker(IHttpClientFactory httpClientFactory, IOptions<McpOptions> options) : IMcpToolInvoker
{
    /// <summary>
    /// The name of the named <see cref="HttpClient"/> used for the http transport.
    /// </summary>
    public const string HttpClientName = nameof(McpClientToolInvoker);

    private readonly McpOptions _options = options.Value;

    /// <inheritdoc/>
    public async Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken)
    {
        IClientTransport transport = CreateTransport();

        await using McpClient client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        IReadOnlyDictionary<string, object?> arguments = BuildArguments(prompt);

        CallToolResult result = await client.CallToolAsync(
            _options.ToolName,
            arguments,
            cancellationToken: cancellationToken);

        TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

        return textBlock is null
            ? throw new InvalidOperationException($"The MCP tool '{_options.ToolName}' did not return any text content.")
            : textBlock.Text;
    }

    /// <summary>
    /// Builds the MCP tool call arguments for <paramref name="prompt"/>.
    /// </summary>
    /// <param name="prompt">The prompt to send to the tool.</param>
    /// <returns>The tool call arguments.</returns>
    internal static IReadOnlyDictionary<string, object?> BuildArguments(string prompt)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["prompt"] = prompt };
    }

    /// <summary>
    /// Creates the client transport for the configured <see cref="McpOptions.Transport"/>.
    /// </summary>
    /// <returns>The client transport to connect with.</returns>
    internal IClientTransport CreateTransport()
    {
        return _options.Transport switch
        {
            McpTransportType.Stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Yappr",
                Command = _options.Command ?? throw new InvalidOperationException("Mcp:Command must be configured for the Stdio transport."),
                Arguments = _options.Arguments,
            }),

            McpTransportType.Http => new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(_options.HttpEndpoint ?? throw new InvalidOperationException("Mcp:HttpEndpoint must be configured for the Http transport.")),
                },
                httpClientFactory.CreateClient(HttpClientName),
                loggerFactory: null,
                ownsHttpClient: true),

            _ => throw new InvalidOperationException($"Unsupported Mcp:Transport value '{_options.Transport}'."),
        };
    }
}
