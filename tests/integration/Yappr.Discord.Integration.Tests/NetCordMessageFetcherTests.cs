namespace Yappr.Discord.Integration.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetCord.Gateway;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Windowing;

/// <summary>
/// Exercises <see cref="NetCordMessageFetcher"/> against a real NetCord <see cref="GatewayClient"/>, with Discord's
/// REST API faked behind NetCord's request-handler seam (see <see cref="StubDiscordServer"/>), so real
/// (de)serialization and pagination code paths run.
/// </summary>
[TestFixture]
public sealed class NetCordMessageFetcherTests
{
    private static readonly DateTimeOffset _baseTime = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task FetchAsync_FiltersBotAndEmptyMessages_ReturnsRealMessagesOldestFirst()
    {
        // Discord returns messages newest first; NetCordMessageFetcher walks and reverses them.
        StubDiscordMessage[] messagesNewestFirst =
        [
            new(Username: "carol", IsBot: false, Content: "third", TimestampUtc: _baseTime.AddMinutes(3)),
            new(Username: "bot", IsBot: true, Content: "ignored bot message", TimestampUtc: _baseTime.AddMinutes(2)),
            new(Username: "bob", IsBot: false, Content: "   ", TimestampUtc: _baseTime.AddMinutes(1)),
            new(Username: "alice", IsBot: false, Content: "first", TimestampUtc: _baseTime),
        ];

        IReadOnlyList<ChannelMessage> result = await FetchAsync(messagesNewestFirst, TldrWindowResolution.ForMessageLimit(100));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(new ChannelMessage("alice", "first", _baseTime)));
            Assert.That(result[1], Is.EqualTo(new ChannelMessage("carol", "third", _baseTime.AddMinutes(3))));
        }
    }

    [Test]
    public async Task FetchAsync_TimeCutoff_ExcludesMessagesOlderThanSinceUtc()
    {
        StubDiscordMessage[] messagesNewestFirst =
        [
            new(Username: "carol", IsBot: false, Content: "newest", TimestampUtc: _baseTime.AddMinutes(10)),
            new(Username: "bob", IsBot: false, Content: "right at cutoff", TimestampUtc: _baseTime.AddMinutes(5)),
            new(Username: "alice", IsBot: false, Content: "too old", TimestampUtc: _baseTime),
        ];

        IReadOnlyList<ChannelMessage> result = await FetchAsync(messagesNewestFirst, TldrWindowResolution.ForTimeCutoff(_baseTime.AddMinutes(5)));

        string[] expectedContent = ["right at cutoff", "newest"];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(m => m.Content), Is.EqualTo(expectedContent));
        }
    }

    [Test]
    public async Task FetchAsync_MessageLimit_StopsAtLimitKeepingNewestEligibleMessages()
    {
        StubDiscordMessage[] messagesNewestFirst =
        [
            new(Username: "carol", IsBot: false, Content: "newest", TimestampUtc: _baseTime.AddMinutes(2)),
            new(Username: "bob", IsBot: false, Content: "middle", TimestampUtc: _baseTime.AddMinutes(1)),
            new(Username: "alice", IsBot: false, Content: "oldest", TimestampUtc: _baseTime),
        ];

        IReadOnlyList<ChannelMessage> result = await FetchAsync(messagesNewestFirst, TldrWindowResolution.ForMessageLimit(2));

        string[] expectedContent = ["middle", "newest"];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(m => m.Content), Is.EqualTo(expectedContent));
        }
    }

    [Test]
    public async Task FetchAsync_ChannelIsNotATextChannel_ReturnsEmpty()
    {
        var server = new StubDiscordServer(messagesNewestFirst: [], channelType: 4); // 4 = guild category, not a text channel
        GatewayClient gatewayClient = server.BuildGatewayClient();
        var fetcher = new NetCordMessageFetcher(gatewayClient);

        IReadOnlyList<ChannelMessage> result = await fetcher.FetchAsync(StubDiscordServer.ChannelId, TldrWindowResolution.ForMessageLimit(100), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    private static async Task<IReadOnlyList<ChannelMessage>> FetchAsync(
        IReadOnlyList<StubDiscordMessage> messagesNewestFirst,
        TldrWindowResolution window)
    {
        var server = new StubDiscordServer(messagesNewestFirst);
        GatewayClient gatewayClient = server.BuildGatewayClient();
        var fetcher = new NetCordMessageFetcher(gatewayClient);

        return await fetcher.FetchAsync(StubDiscordServer.ChannelId, window, CancellationToken.None);
    }
}
