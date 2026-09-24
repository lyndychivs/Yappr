namespace Yappr;

using System;
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
public sealed class TldrOrchestrator
{
    private readonly IMessageFetcher _messageFetcher;

    private readonly ISummariser _summariser;

    private readonly TldrLimitsOptions _limits;

    /// <summary>
    /// Initializes a new instance of the <see cref="TldrOrchestrator"/> class.
    /// </summary>
    /// <param name="messageFetcher">Fetches the channel messages within the requested window.</param>
    /// <param name="summariser">Summarises the fetched messages.</param>
    /// <param name="limits">The configured caps on the requested window.</param>
    public TldrOrchestrator(
        IMessageFetcher messageFetcher,
        ISummariser summariser,
        IOptions<TldrLimitsOptions> limits)
    {
        ArgumentNullException.ThrowIfNull(messageFetcher);
        ArgumentNullException.ThrowIfNull(summariser);
        ArgumentNullException.ThrowIfNull(limits);

        _messageFetcher = messageFetcher;
        _summariser = summariser;
        _limits = limits.Value;
    }

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
        TldrWindowResolution window = TldrWindowResolver.Resolve(kind, value, _limits);
        if (!window.IsValid)
        {
            return TldrOutcome.Failure(window.ValidationError!);
        }

        IReadOnlyList<ChannelMessage> messages = await _messageFetcher.FetchAsync(channelId, window, cancellationToken);
        if (messages.Count == 0)
        {
            return TldrOutcome.Failure("No messages found in that window.");
        }

        SummaryResult result = await _summariser.SummariseAsync(messages, cancellationToken);
        return TldrOutcome.Success(result);
    }
}
