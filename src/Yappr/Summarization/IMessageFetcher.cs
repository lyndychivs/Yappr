namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;
using Yappr.Windowing;

public interface IMessageFetcher
{
    Task<IReadOnlyList<ChannelMessage>> FetchAsync(ulong channelId, TldrWindowResolution window, CancellationToken cancellationToken);
}
