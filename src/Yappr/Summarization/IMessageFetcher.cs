namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;
using Yappr.Windowing;

/// <summary>
/// Fetches the channel messages a `/tldr` request needs to summarize.
/// </summary>
public interface IMessageFetcher
{
    /// <summary>
    /// Fetches the messages in <paramref name="channelId"/> that fall within <paramref name="window"/>.
    /// </summary>
    /// <param name="channelId">The channel to fetch messages from.</param>
    /// <param name="window">The resolved time or count window to fetch within.</param>
    /// <param name="cancellationToken">A token to cancel the fetch.</param>
    /// <returns>The matching messages, oldest first.</returns>
    Task<IReadOnlyList<ChannelMessage>> FetchAsync(ulong channelId, TldrWindowResolution window, CancellationToken cancellationToken);
}
