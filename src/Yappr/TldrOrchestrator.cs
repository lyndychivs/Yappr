namespace Yappr;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Yappr.Models;
using Yappr.Summarization;
using Yappr.Windowing;

/// <summary>
/// Top-level entry point for `/tldr`: validates the requested window, fetches the matching channel history,
/// and hands it off to the configured <see cref="ISummariser"/>.
/// </summary>
public sealed class TldrOrchestrator(
    IMessageFetcher messageFetcher,
    ISummariser summariser,
    IOptions<TldrLimitsOptions> limits)
{
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
