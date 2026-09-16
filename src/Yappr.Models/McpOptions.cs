namespace Yappr.Models;

public sealed class McpOptions
{
    public const string SectionName = "Mcp";

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
    /// Gets or sets the name of the MCP tool to invoke for summarization (e.g. a "chat" or "complete" tool
    /// exposed by the configured server). Kept configurable so the underlying LLM provider can change without
    /// touching bot code.
    /// </summary>
    public string ToolName { get; set; } = "chat";
}
