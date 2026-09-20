namespace Yappr.Models;

using System;

/// <summary>
/// Configures the MCP server used for summarization.
/// </summary>
public sealed class McpOptions
{
    /// <summary>
    /// The configuration section name this class binds to.
    /// </summary>
    public const string SectionName = "Mcp";

    /// <summary>
    /// Gets or sets the transport used to reach the MCP server.
    /// </summary>
    public McpTransportType Transport { get; set; } = McpTransportType.Stdio;

    /// <summary>
    /// Gets or sets the command used to launch the MCP server process (stdio transport).
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Gets or sets the arguments passed to <see cref="Command"/> (stdio transport).
    /// </summary>
    public string[] Arguments { get; set; } = [];

    /// <summary>
    /// Gets or sets the HTTP/SSE endpoint of an already-running MCP server (http transport).
    /// </summary>
    public string? HttpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the MCP tool to invoke for summarization.
    /// </summary>
    public string ToolName { get; set; } = "chat";

    /// <summary>
    /// Gets or sets the HTTP resilience settings for the MCP client. Timeouts default to the MCP server's
    /// plus <see cref="McpResilienceOptions.ClientMargin"/>. If the timeouts are overridden in configuration,
    /// keep the client's above <c>Ollama:Resilience</c> on the server; the two are configured independently.
    /// </summary>
    public McpResilienceOptions Resilience { get; set; } = new()
    {
        AttemptTimeout = McpResilienceOptions.ServerTimeout + McpResilienceOptions.ClientMargin,
        TotalRequestTimeout = McpResilienceOptions.ServerTimeout + McpResilienceOptions.ClientMargin,
    };
}
