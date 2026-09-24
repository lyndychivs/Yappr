namespace Yappr;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Yappr.Models.Dto;
using Yappr.Models.Enums;
using Yappr.Models.Options;
using Yappr.Summarization;
using Yappr.Windowing;

/// <summary>
/// Entry point for `/tldr`: validates the requested window, fetches matching channel history, and hands it
/// to the configured <see cref="ISummariser"/>.
/// </summary>
public sealed class TldrOrchestrator(
    IMessageFetcher messageFetcher,
    ISummariser summariser,
    IOptions<TldrLimitsOptions> limits)
{
    /// <summary>
    /// Runs a `/tldr` request end to end.
    /// </summary>
    /// <param name="channelId">The channel to summarize.</param>
    /// <param name="kind">The kind of window requested (days, hours, or messages).</param>
    /// <param name="value">The window's numeric value.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The resulting summary, or a user-facing failure reason.</returns>
    public async Task<TldrOutcome> RunAsync(
        ulong channelId,
        TldrWindowKind kind,
        int value,
        CancellationToken cancellationToken)
    {
        TldrWindowResolution window = TldrWindowResolver.Resolve(kind, value, limits.Value);
        if (!window.IsValid)
        {
            return TldrOutcome.Failure(window.ValidationError!);
        }

        IReadOnlyList<ChannelMessage> messages = await messageFetcher.FetchAsync(channelId, window, cancellationToken);
        if (messages.Count == 0)
        {
            return TldrOutcome.Failure("No messages found in that window.");
        }

        SummaryResult result = await summariser.SummariseAsync(messages, cancellationToken);
        return TldrOutcome.Success(result);
    }
}
