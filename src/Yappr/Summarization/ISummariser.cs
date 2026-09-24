namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models.Dto;

/// <summary>
/// Summarises a channel transcript.
/// </summary>
public interface ISummariser
{
    /// <summary>
    /// Summarises <paramref name="messages"/>.
    /// </summary>
    /// <param name="messages">The channel transcript to summarize.</param>
    /// <param name="cancellationToken">A token to cancel the summarization.</param>
    /// <returns>The generated summary.</returns>
    Task<SummaryResult> SummariseAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken);
}
