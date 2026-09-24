namespace Yappr.Discord.Unit.Tests.Commands;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr;
using Yappr.Discord.Commands;
using Yappr.Models.Dto;
using Yappr.Models.Options;
using Yappr.Summarization;
using Yappr.Windowing;

/// <summary>
/// Exercises <see cref="TldrSlashCommand.BuildResponseContent"/>, the `/tldr` command's success/failure response
/// formatting, and the constructor's guard clauses. The rest of <see cref="TldrSlashCommand"/> is thin NetCord
/// interaction plumbing (respond, log, edit the response) that isn't practically fakeable without a real Discord
/// interaction payload.
/// </summary>
[TestFixture]
public sealed class TldrSlashCommandTests
{
    [Test]
    public void BuildResponseContent_SuccessfulOutcome_IncludesMessageCountAndSummary()
    {
        TldrOutcome outcome = TldrOutcome.Success(new SummaryResult("Alice and Bob discussed the release.", MessageCount: 42));

        string content = TldrSlashCommand.BuildResponseContent(outcome);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("42 messages"));
            Assert.That(content, Does.Contain("Alice and Bob discussed the release."));
            Assert.That(content, Does.Not.Contain("⚠️"));
        }
    }

    [Test]
    public void BuildResponseContent_FailedOutcome_ReturnsWarningWithError()
    {
        TldrOutcome outcome = TldrOutcome.Failure("That window is too large.");

        string content = TldrSlashCommand.BuildResponseContent(outcome);

        Assert.That(content, Is.EqualTo("⚠️ That window is too large."));
    }

    [Test]
    public void BuildResponseContent_NullOutcome_ThrowsArgumentNullException()
    {
        Assert.That(
            () => TldrSlashCommand.BuildResponseContent(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("outcome"));
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        TldrOrchestrator orchestrator = CreateOrchestrator();

        Assert.That(
            () => new TldrSlashCommand(null!, orchestrator),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("logger"));
    }

    [Test]
    public void Constructor_NullOrchestrator_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TldrSlashCommand(NullLogger<TldrSlashCommand>.Instance, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("orchestrator"));
    }

    private static TldrOrchestrator CreateOrchestrator()
    {
        return new TldrOrchestrator(new UnusedMessageFetcher(), new UnusedSummariser(), Options.Create(new TldrLimitsOptions()));
    }

    private sealed class UnusedMessageFetcher : IMessageFetcher
    {
        public Task<IReadOnlyList<ChannelMessage>> FetchAsync(ulong channelId, TldrWindowResolution window, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class UnusedSummariser : ISummariser
    {
        public Task<SummaryResult> SummariseAsync(IReadOnlyList<ChannelMessage> messages, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
