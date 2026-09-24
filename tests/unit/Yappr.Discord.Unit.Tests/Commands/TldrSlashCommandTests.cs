namespace Yappr.Discord.Unit.Tests.Commands;

using NUnit.Framework;

using Yappr;
using Yappr.Discord.Commands;
using Yappr.Models.Dto;

/// <summary>
/// Exercises <see cref="TldrSlashCommand.BuildResponseContent"/>, the `/tldr` command's success/failure response
/// formatting. The rest of <see cref="TldrSlashCommand"/> is thin NetCord interaction plumbing (respond, log, edit
/// the response) that isn't practically fakeable without a real Discord interaction payload.
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
}
