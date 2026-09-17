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
/// Connects to the configured MCP server (a "chatgpt" MCP server for now — swappable via configuration, or by
/// replacing this class with a direct-API <see cref="IMcpToolInvoker"/> implementation later) and invokes its
/// summarization tool. A new client connection is opened per call, which is simple and appropriate for the
/// low request volume of a Discord slash command.
/// </summary>
public sealed class McpClientToolInvoker(IHttpClientFactory httpClientFactory, IOptions<McpOptions> options) : IMcpToolInvoker
{
    public const string HttpClientName = nameof(McpClientToolInvoker);

    private readonly McpOptions options = options.Value;

    public async Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken)
    {
        IClientTransport transport = CreateTransport();

        await using McpClient client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        IReadOnlyDictionary<string, object?> arguments = BuildArguments(prompt);

        CallToolResult result = await client.CallToolAsync(
            options.ToolName,
            arguments,
            cancellationToken: cancellationToken);

        TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

        return textBlock is null
            ? throw new InvalidOperationException($"The MCP tool '{options.ToolName}' did not return any text content.")
            : textBlock.Text;
    }

    internal static IReadOnlyDictionary<string, object?> BuildArguments(string prompt)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal) { ["prompt"] = prompt };
    }

    internal IClientTransport CreateTransport()
    {
        return options.Transport switch
        {
            McpTransportType.Stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Yappr",
                Command = options.Command ?? throw new InvalidOperationException("Mcp:Command must be configured for the Stdio transport."),
                Arguments = options.Arguments,
            }),

            McpTransportType.Http => new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(options.HttpEndpoint ?? throw new InvalidOperationException("Mcp:HttpEndpoint must be configured for the Http transport.")),
                },
                httpClientFactory.CreateClient(HttpClientName),
                loggerFactory: null,
                ownsHttpClient: true),

            _ => throw new InvalidOperationException($"Unsupported Mcp:Transport value '{options.Transport}'."),
        };
    }
}
