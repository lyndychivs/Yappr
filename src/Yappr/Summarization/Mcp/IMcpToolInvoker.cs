namespace Yappr.Summarization.Mcp;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Thin boundary around the actual MCP tool call, kept separate from <see cref="ISummariser"/> so the
/// transport (stdio process, HTTP/SSE server, or a future direct-API implementation) can change without
/// touching prompt-building or orchestration logic, and so summarisation logic can be unit tested without a
/// live MCP server.
/// </summary>
public interface IMcpToolInvoker
{
    Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken);
}
