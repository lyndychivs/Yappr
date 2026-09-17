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
public sealed class McpSummariserTests
{
    [Test]
    public async Task SummariseAsync_ReturnsTrimmedSummaryAndMessageCount()
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

        var summariser = new McpSummariser(toolInvoker.Object);

        SummaryResult result = await summariser.SummariseAsync(messages, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Summary, Is.EqualTo("A short summary."));
            Assert.That(result.MessageCount, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task SummariseAsync_PassesBuiltPromptToToolInvoker()
    {
        var messages = new List<ChannelMessage> { new("Alice", "Unique message content", DateTimeOffset.UtcNow) };

        string? capturedPrompt = null;
        var toolInvoker = new Mock<IMcpToolInvoker>(MockBehavior.Strict);
        toolInvoker
            .Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("summary");

        var summariser = new McpSummariser(toolInvoker.Object);

        await summariser.SummariseAsync(messages, CancellationToken.None);

        Assert.That(capturedPrompt, Does.Contain("Unique message content"));
    }
}
