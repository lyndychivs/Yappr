namespace Yappr.Unit.Tests.Summarization;

using System;
using System.Collections.Generic;
using System.Globalization;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization;

[TestFixture]
public sealed class TldrPromptBuilderTests
{
    [Test]
    public void Build_IncludesAuthorAndContentForEachMessage()
    {
        var messages = new List<ChannelMessage>
        {
            new("Alice", "Let's ship the release on Friday.", DateTimeOffset.UtcNow.AddMinutes(-10)),
            new("Bob", "Sounds good, I'll write the changelog.", DateTimeOffset.UtcNow),
        };

        string prompt = TldrPromptBuilder.Build(messages);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(prompt, Does.Contain("Alice"));
            Assert.That(prompt, Does.Contain("Let's ship the release on Friday."));
            Assert.That(prompt, Does.Contain("Bob"));
            Assert.That(prompt, Does.Contain("Sounds good, I'll write the changelog."));
        }
    }

    [Test]
    public void Build_WhenTranscriptExceedsBudget_DropsOldestMessagesFirst()
    {
        var messages = new List<ChannelMessage>();
        for (int i = 0; i < 2_000; i++)
        {
            messages.Add(new ChannelMessage("User", string.Create(CultureInfo.InvariantCulture, $"message number {i} with some padding text to grow the transcript"), DateTimeOffset.UtcNow.AddSeconds(i)));
        }

        string prompt = TldrPromptBuilder.Build(messages);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(prompt, Does.Not.Contain("message number 0 "));
            Assert.That(prompt, Does.Contain("message number 1999"));
            Assert.That(prompt, Has.Length.LessThan(TldrPromptBuilder.MaxPromptCharacters + 2_000));
        }
    }

    [Test]
    public void Build_IncludesFixedInstructionPreamble()
    {
        var messages = new List<ChannelMessage> { new("Alice", "Hi", DateTimeOffset.UtcNow) };

        string prompt = TldrPromptBuilder.Build(messages);

        Assert.That(
            prompt,
            Does.StartWith(
                "Summarize the following Discord channel conversation into a short, skimmable TL;DR. " +
                "Group related messages by topic, call out decisions or action items, and ignore small talk. " +
                "Reply in plain text suitable for a Discord message." +
                "\n\n---\n"));
    }

    [Test]
    public void Build_WhenTranscriptExactlyAtBudget_KeepsAllLines()
    {
        // The rendered line (content + fixed timestamp/author prefix) plus its trailing newline equals the budget
        // exactly, so the total must NOT be treated as over budget.
        ChannelMessage message = MessageWithRenderedLineLength("A", 'x', TldrPromptBuilder.MaxPromptCharacters - 1, DateTimeOffset.UtcNow);

        string prompt = TldrPromptBuilder.Build([message]);

        Assert.That(prompt, Does.Contain(message.Content));
    }

    [Test]
    public void Build_WhenTotalIsOneOverBudget_DropsOnlyTheOldestLine()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ChannelMessage newest = MessageWithRenderedLineLength("B", 'b', TldrPromptBuilder.MaxPromptCharacters / 2, now);
        ChannelMessage oldest = MessageWithRenderedLineLength("A", 'a', (TldrPromptBuilder.MaxPromptCharacters / 2) + 1, now.AddMinutes(-1));
        var messages = new List<ChannelMessage> { oldest, newest };

        string prompt = TldrPromptBuilder.Build(messages);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(prompt, Does.Not.Contain(oldest.Content));
            Assert.That(prompt, Does.Contain(newest.Content));
        }
    }

    [Test]
    public void Build_WhenRunningTotalHitsBudgetExactlyPartway_KeepsThatLineAndDropsOnlyOlderOnes()
    {
        // Three lines, oldest to newest. Summed from the newest backward, the running total lands on exactly
        // MaxPromptCharacters after including the middle line: the original ">" comparison must NOT stop there,
        // so it keeps accumulating, then drops everything older than the middle line once the oldest line pushes
        // the running total over budget.
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ChannelMessage newest = MessageWithRenderedLineLength("C", 'c', 3_000, now);
        ChannelMessage middle = MessageWithRenderedLineLength("B", 'b', TldrPromptBuilder.MaxPromptCharacters - 3_001 - 1, now.AddMinutes(-1));
        ChannelMessage oldest = MessageWithRenderedLineLength("A", 'a', 100, now.AddMinutes(-2));
        var messages = new List<ChannelMessage> { oldest, middle, newest };

        string prompt = TldrPromptBuilder.Build(messages);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(prompt, Does.Not.Contain(oldest.Content));
            Assert.That(prompt, Does.Contain(middle.Content));
            Assert.That(prompt, Does.Contain(newest.Content));
        }
    }

    private static ChannelMessage MessageWithRenderedLineLength(string author, char fill, int renderedLineLength, DateTimeOffset timestamp)
    {
        string prefix = string.Create(CultureInfo.InvariantCulture, $"[{timestamp:yyyy-MM-dd HH:mm} UTC] {author}: ");
        return new ChannelMessage(author, new string(fill, renderedLineLength - prefix.Length), timestamp);
    }
}
