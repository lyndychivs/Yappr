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
}
