namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;
using Yappr.Summarization.Mcp;

public sealed class McpSummarizer(IMcpToolInvoker toolInvoker) : ISummarizer
{
    public async Task<SummaryResult> SummarizeAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken)
    {
        string prompt = TldrPromptBuilder.Build(messages);
        string summary = await toolInvoker.InvokeAsync(prompt, cancellationToken);

        return new SummaryResult(summary.Trim(), messages.Count);
    }
}
