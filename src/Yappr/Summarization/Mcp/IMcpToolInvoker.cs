namespace Yappr.Summarization.Mcp;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Boundary around a single MCP tool call, decoupled from the transport (stdio, HTTP/SSE) so it can be
/// swapped or unit tested independently of <see cref="ISummariser"/>.
/// </summary>
public interface IMcpToolInvoker
{
    /// <summary>
    /// Invokes the configured MCP tool with <paramref name="prompt"/>.
    /// </summary>
    /// <param name="prompt">The prompt to send to the tool.</param>
    /// <param name="cancellationToken">A token to cancel the call.</param>
    /// <returns>The tool's text response.</returns>
    Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken);
}
