namespace Yappr.Models;

/// <summary>
/// The transport used to reach the MCP server.
/// </summary>
public enum McpTransportType
{
    /// <summary>
    /// The server is launched as a local process and communicated with over stdio.
    /// </summary>
    Stdio,

    /// <summary>
    /// The server is already running and reachable over HTTP/SSE.
    /// </summary>
    Http,
}
