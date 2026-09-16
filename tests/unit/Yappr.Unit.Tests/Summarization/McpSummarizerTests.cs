namespace Yappr.Unit.Tests.Summarization;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization;
using Yappr.Summarization.Mcp;

[TestFixture]
public sealed class McpSummarizerTests
{
    [Test]
    public async Task SummarizeAsync_ReturnsTrimmedSummaryAndMessageCount()
    {
        var messages = new List<ChannelMessage>
        {
            new("Alice", "Hello", DateTimeOffset.UtcNow),
            new("Bob", "World", DateTimeOffset.UtcNow),
        };

        var toolInvoker = new Mock<IMcpToolInvoker>(MockBehavior.Strict);
        toolInvoker
            .Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("  A short summary.  ");

        var summarizer = new McpSummarizer(toolInvoker.Object);

        SummaryResult result = await summarizer.SummarizeAsync(messages, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Summary, Is.EqualTo("A short summary."));
            Assert.That(result.MessageCount, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task SummarizeAsync_PassesBuiltPromptToToolInvoker()
    {
        var messages = new List<ChannelMessage> { new("Alice", "Unique message content", DateTimeOffset.UtcNow) };

        string? capturedPrompt = null;
        var toolInvoker = new Mock<IMcpToolInvoker>(MockBehavior.Strict);
        toolInvoker
            .Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("summary");

        var summarizer = new McpSummarizer(toolInvoker.Object);

        await summarizer.SummarizeAsync(messages, CancellationToken.None);

        Assert.That(capturedPrompt, Does.Contain("Unique message content"));
    }
}
