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
public sealed class McpClientToolInvoker : IMcpToolInvoker
{
    /// <summary>
    /// The name of the named <see cref="HttpClient"/> used for the http transport.
    /// </summary>
    public const string HttpClientName = nameof(McpClientToolInvoker);

    private readonly McpOptions _mcpOptions;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly CallToolFunc _callToolFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpClientToolInvoker"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the named <see cref="HttpClient"/> for the http transport.</param>
    /// <param name="options">The MCP options to invoke tools with.</param>
    public McpClientToolInvoker(IHttpClientFactory httpClientFactory, IOptions<McpOptions> options)
        : this(httpClientFactory, options, ConnectAndCallToolAsync)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpClientToolInvoker"/> class that calls tools through
    /// <paramref name="callTool"/> instead of a real MCP connection, so <see cref="InvokeAsync"/>'s orchestration
    /// can be unit tested without one.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the named <see cref="HttpClient"/> for the http transport.</param>
    /// <param name="options">The MCP options to invoke tools with.</param>
    /// <param name="callTool">The function used to connect and call the tool.</param>
    internal McpClientToolInvoker(IHttpClientFactory httpClientFactory, IOptions<McpOptions> options, CallToolFunc callTool)
    {
        _httpClientFactory = httpClientFactory;
        _mcpOptions = options.Value;
        _callToolFunc = callTool;
    }

    /// <summary>
    /// Connects <paramref name="transport"/> and calls <paramref name="toolName"/> with <paramref name="arguments"/>.
    /// </summary>
    /// <param name="transport">The client transport to connect with.</param>
    /// <param name="toolName">The name of the tool to call.</param>
    /// <param name="arguments">The tool call arguments.</param>
    /// <param name="cancellationToken">A token to cancel the connection and tool call.</param>
    /// <returns>The tool call result.</returns>
    internal delegate Task<CallToolResult> CallToolFunc(
        IClientTransport transport,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken);

    /// <inheritdoc/>
    public async Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken)
    {
        IClientTransport transport = CreateTransport();

        IReadOnlyDictionary<string, object?> arguments = BuildArguments(prompt);

        CallToolResult result = await _callToolFunc(transport, _mcpOptions.ToolName, arguments, cancellationToken);

        return ExtractText(result, _mcpOptions.ToolName);
    }

    /// <summary>
    /// Extracts the text of the first <see cref="TextContentBlock"/> in <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The tool call result to extract text from.</param>
    /// <param name="toolName">The name of the tool that produced <paramref name="result"/>, for the error message.</param>
    /// <returns>The first text block's text.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> contains no text content.</exception>
    internal static string ExtractText(CallToolResult result, string toolName)
    {
        TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

        return textBlock is null
            ? throw new InvalidOperationException($"The MCP tool '{toolName}' did not return any text content.")
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
        return _mcpOptions.Transport switch
        {
            McpTransportType.Stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Yappr",
                Command = _mcpOptions.Command ?? throw new InvalidOperationException("Mcp:Command must be configured for the Stdio transport."),
                Arguments = _mcpOptions.Arguments,
            }),

            McpTransportType.Http => new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(_mcpOptions.HttpEndpoint ?? throw new InvalidOperationException("Mcp:HttpEndpoint must be configured for the Http transport.")),
                },
                _httpClientFactory.CreateClient(HttpClientName),
                loggerFactory: null,
                ownsHttpClient: true),

            _ => throw new InvalidOperationException($"Unsupported Mcp:Transport value '{_mcpOptions.Transport}'."),
        };
    }

    /// <summary>
    /// Opens an MCP client connection over <paramref name="transport"/> and calls <paramref name="toolName"/>.
    /// </summary>
    /// <param name="transport">The client transport to connect with.</param>
    /// <param name="toolName">The name of the tool to call.</param>
    /// <param name="arguments">The tool call arguments.</param>
    /// <param name="cancellationToken">A token to cancel the connection and tool call.</param>
    /// <returns>The tool call result.</returns>
    private static async Task<CallToolResult> ConnectAndCallToolAsync(
        IClientTransport transport,
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        await using McpClient client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        return await client.CallToolAsync(toolName, arguments, cancellationToken: cancellationToken);
    }
}
