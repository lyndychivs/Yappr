namespace Yappr.Summarization;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models.Dto;
using Yappr.Summarization.Mcp;

/// <summary>
/// Summarises a channel transcript via an <see cref="IMcpToolInvoker"/>.
/// </summary>
public sealed class McpSummariser : ISummariser
{
    private readonly IMcpToolInvoker _toolInvoker;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpSummariser"/> class.
    /// </summary>
    /// <param name="toolInvoker">The MCP tool invoker used to run the summarization.</param>
    public McpSummariser(IMcpToolInvoker toolInvoker)
    {
        ArgumentNullException.ThrowIfNull(toolInvoker);

        _toolInvoker = toolInvoker;
    }

    /// <inheritdoc/>
    public async Task<SummaryResult> SummariseAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messages);

        string prompt = TldrPromptBuilder.Build(messages);
        string summary = await _toolInvoker.InvokeAsync(prompt, cancellationToken);

        return new SummaryResult(summary.Trim(), messages.Count);
    }
}
