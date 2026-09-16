namespace Yappr.Unit.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization;

[TestFixture]
public sealed class TldrOrchestratorTests
{
    private readonly IOptions<TldrLimitsOptions> limits = Options.Create(new TldrLimitsOptions());

    [Test]
    public async Task RunAsync_InvalidWindow_ReturnsFailureWithoutFetchingOrSummarizing()
    {
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        var summarizer = new Mock<ISummarizer>(MockBehavior.Strict);
        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summarizer.Object, limits);

        TldrOutcome outcome = await orchestrator.RunAsync(1, TldrWindowKind.Days, 0, CancellationToken.None);

        Assert.That(outcome.IsSuccess, Is.False);
    }

    [Test]
    public async Task RunAsync_NoMessagesFound_ReturnsFailure()
    {
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        messageFetcher
            .Setup(f => f.FetchAsync(It.IsAny<ulong>(), It.IsAny<Yappr.Windowing.TldrWindowResolution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ChannelMessage>)[]);
        var summarizer = new Mock<ISummarizer>(MockBehavior.Strict);
        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summarizer.Object, limits);

        TldrOutcome outcome = await orchestrator.RunAsync(1, TldrWindowKind.Messages, 10, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outcome.IsSuccess, Is.False);
            Assert.That(outcome.Error, Does.Contain("No messages"));
        }
    }

    [Test]
    public async Task RunAsync_MessagesFound_ReturnsSummaryFromSummarizer()
    {
        IReadOnlyList<ChannelMessage> messages = [new("Alice", "Hi", DateTimeOffset.UtcNow)];
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        messageFetcher
            .Setup(f => f.FetchAsync(It.IsAny<ulong>(), It.IsAny<Yappr.Windowing.TldrWindowResolution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var expected = new SummaryResult("Summary text", 1);
        var summarizer = new Mock<ISummarizer>(MockBehavior.Strict);
        summarizer
            .Setup(s => s.SummarizeAsync(messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summarizer.Object, limits);

        TldrOutcome outcome = await orchestrator.RunAsync(1, TldrWindowKind.Messages, 10, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outcome.IsSuccess, Is.True);
            Assert.That(outcome.Result, Is.EqualTo(expected));
        }
    }
}
