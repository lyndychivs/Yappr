namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Yappr.Models;

public interface ISummarizer
{
    Task<SummaryResult> SummarizeAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken);
}
