namespace Yappr.Discord.Integration.Tests.Services;

using System;

/// <summary>
/// A single Discord message as the real REST API would return it, for feeding into <see cref="StubDiscordServer"/>.
/// </summary>
/// <param name="Username">The author's username.</param>
/// <param name="IsBot">Whether the author is a bot account.</param>
/// <param name="Content">The message text.</param>
/// <param name="TimestampUtc">
/// When the message was sent. NetCord derives a message's <c>CreatedAt</c> from its snowflake id, not the JSON
/// <c>timestamp</c> field, so <see cref="StubDiscordServer"/> encodes this into the id it serves.
/// </param>
internal sealed record StubDiscordMessage(string Username, bool IsBot, string Content, DateTimeOffset TimestampUtc);
