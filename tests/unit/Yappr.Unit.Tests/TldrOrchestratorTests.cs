namespace Yappr.Unit.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Yappr.Models.Dto;
using Yappr.Models.Enums;
using Yappr.Models.Options;
using Yappr.Summarization;

[TestFixture]
public sealed class TldrOrchestratorTests
{
    private readonly IOptions<TldrLimitsOptions> _limits = Options.Create(new TldrLimitsOptions());

    [Test]
    public async Task RunAsync_InvalidWindow_ReturnsFailureWithoutFetchingOrSummarising()
    {
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        var summariser = new Mock<ISummariser>(MockBehavior.Strict);
        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summariser.Object, _limits);

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
        var summariser = new Mock<ISummariser>(MockBehavior.Strict);
        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summariser.Object, _limits);

        TldrOutcome outcome = await orchestrator.RunAsync(1, TldrWindowKind.Messages, 10, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outcome.IsSuccess, Is.False);
            Assert.That(outcome.Error, Does.Contain("No messages"));
        }
    }

    [Test]
    public async Task RunAsync_MessagesFound_ReturnsSummaryFromSummariser()
    {
        IReadOnlyList<ChannelMessage> messages = [new("Alice", "Hi", DateTimeOffset.UtcNow)];
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        messageFetcher
            .Setup(f => f.FetchAsync(It.IsAny<ulong>(), It.IsAny<Yappr.Windowing.TldrWindowResolution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var expected = new SummaryResult("Summary text", 1);
        var summariser = new Mock<ISummariser>(MockBehavior.Strict);
        summariser
            .Setup(s => s.SummariseAsync(messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var orchestrator = new TldrOrchestrator(messageFetcher.Object, summariser.Object, _limits);

        TldrOutcome outcome = await orchestrator.RunAsync(1, TldrWindowKind.Messages, 10, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(outcome.IsSuccess, Is.True);
            Assert.That(outcome.Result, Is.EqualTo(expected));
        }
    }

    [Test]
    public void Constructor_NullMessageFetcher_ThrowsArgumentNullException()
    {
        var summariser = new Mock<ISummariser>(MockBehavior.Strict);

        Assert.That(
            () => new TldrOrchestrator(null!, summariser.Object, _limits),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("messageFetcher"));
    }

    [Test]
    public void Constructor_NullSummariser_ThrowsArgumentNullException()
    {
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);

        Assert.That(
            () => new TldrOrchestrator(messageFetcher.Object, null!, _limits),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("summariser"));
    }

    [Test]
    public void Constructor_NullLimits_ThrowsArgumentNullException()
    {
        var messageFetcher = new Mock<IMessageFetcher>(MockBehavior.Strict);
        var summariser = new Mock<ISummariser>(MockBehavior.Strict);

        Assert.That(
            () => new TldrOrchestrator(messageFetcher.Object, summariser.Object, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("limits"));
    }
}
