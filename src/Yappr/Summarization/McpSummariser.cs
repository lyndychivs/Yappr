namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;
using Yappr.Summarization.Mcp;

/// <summary>
/// Summarises a channel transcript via an <see cref="IMcpToolInvoker"/>.
/// </summary>
/// <param name="toolInvoker">The MCP tool invoker used to run the summarization.</param>
public sealed class McpSummariser(IMcpToolInvoker toolInvoker) : ISummariser
{
    /// <inheritdoc/>
    public async Task<SummaryResult> SummariseAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken)
    {
        string prompt = TldrPromptBuilder.Build(messages);
        string summary = await toolInvoker.InvokeAsync(prompt, cancellationToken);

        return new SummaryResult(summary.Trim(), messages.Count);
    }
}
