namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;

public interface ISummariser
{
    Task<SummaryResult> SummariseAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken);
}
