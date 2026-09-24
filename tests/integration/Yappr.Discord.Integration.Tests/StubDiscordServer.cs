namespace Yappr.Discord.Integration.Tests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

/// <summary>
/// Fakes Discord's REST API behind NetCord's <see cref="IRestRequestHandler"/> seam, so
/// <see cref="NetCordMessageFetcher"/> can be exercised against real (de)serialization and channel-fetch logic
/// without a real Discord connection.
/// </summary>
internal sealed class StubDiscordServer
{
    /// <summary>
    /// The channel id this stub server serves.
    /// </summary>
    public const ulong ChannelId = 123;

    private readonly IReadOnlyList<StubDiscordMessage> _messagesNewestFirst;

    private readonly int _channelType;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubDiscordServer"/> class.
    /// </summary>
    /// <param name="messagesNewestFirst">The channel's messages, newest first, as Discord's API returns them.</param>
    /// <param name="channelType">The Discord channel type to report (0 = guild text).</param>
    public StubDiscordServer(IReadOnlyList<StubDiscordMessage> messagesNewestFirst, int channelType = 0)
    {
        _messagesNewestFirst = messagesNewestFirst;
        _channelType = channelType;
    }

    /// <summary>
    /// Builds a <see cref="GatewayClient"/> whose REST calls are served by this stub.
    /// </summary>
    /// <returns>A gateway client backed by this stub server.</returns>
    public GatewayClient BuildGatewayClient()
    {
        var handler = new StubRestRequestHandler(Respond);
        var restConfig = new RestClientConfiguration { RequestHandler = handler };
        var gatewayConfig = new GatewayClientConfiguration { RestClientConfiguration = restConfig };

        return new GatewayClient(new BotToken(BuildPlaceholderToken()), gatewayConfig);
    }

    /// <summary>
    /// Builds a token that only needs to satisfy <see cref="BotToken"/>'s format check: the request handler above
    /// intercepts every call before it would ever be sent as a real Authorization header. Assembled at runtime,
    /// rather than as a source literal, so it can't be mistaken for a real credential by secret scanners.
    /// </summary>
    /// <returns>A well-formed but meaningless bot token.</returns>
    private static string BuildPlaceholderToken()
    {
        string idPart = Convert.ToBase64String(Encoding.ASCII.GetBytes(new string('0', 18)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return $"{idPart}.{new string('A', 6)}.{new string('A', 28)}";
    }

    /// <summary>
    /// Encodes <paramref name="timestampUtc"/> into a Discord snowflake id, so NetCord's id-derived
    /// <c>CreatedAt</c> matches the timestamp a test intends.
    /// </summary>
    /// <param name="timestampUtc">The timestamp to encode.</param>
    /// <returns>A snowflake id whose embedded timestamp is <paramref name="timestampUtc"/>.</returns>
    private static ulong ToSnowflakeId(DateTimeOffset timestampUtc)
    {
        const long DiscordEpochMs = 1420070400000; // 2015-01-01T00:00:00.000Z

        long millisecondsSinceDiscordEpoch = timestampUtc.ToUnixTimeMilliseconds() - DiscordEpochMs;

        return (ulong)millisecondsSinceDiscordEpoch << 22;
    }

    private HttpResponseMessage Respond(HttpRequestMessage request)
    {
        string path = request.RequestUri!.AbsolutePath;

        string json = path.EndsWith("/messages", StringComparison.Ordinal)
            ? BuildMessagesJson()
            : BuildChannelJson();

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private string BuildChannelJson()
    {
        return string.Create(CultureInfo.InvariantCulture, $$"""{"id":"{{ChannelId}}","type":{{_channelType}},"guild_id":"456","name":"general","position":0,"permission_overwrites":[],"nsfw":false,"parent_id":null}""");
    }

    private string BuildMessagesJson()
    {
        IEnumerable<string> messageObjects = _messagesNewestFirst.Select(BuildMessageJson);

        return $"[{string.Join(',', messageObjects)}]";
    }

    private string BuildMessageJson(StubDiscordMessage message)
    {
        string timestamp = message.TimestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz", CultureInfo.InvariantCulture);
        ulong snowflakeId = ToSnowflakeId(message.TimestampUtc);

        return $$"""
            {
                "id": "{{snowflakeId}}",
                "channel_id": "{{ChannelId}}",
                "author": {"id": "9", "username": "{{message.Username}}", "discriminator": "0", "avatar": null, "bot": {{(message.IsBot ? "true" : "false")}}},
                "content": {{System.Text.Json.JsonSerializer.Serialize(message.Content)}},
                "timestamp": "{{timestamp}}",
                "edited_timestamp": null,
                "tts": false,
                "mention_everyone": false,
                "mentions": [],
                "mention_roles": [],
                "attachments": [],
                "embeds": [],
                "pinned": false,
                "type": 0
            }
            """;
    }
}
