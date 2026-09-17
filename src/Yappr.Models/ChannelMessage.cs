namespace Yappr.Models;

using System;

/// <summary>
/// A single Discord message included in a `/tldr` transcript.
/// </summary>
/// <param name="AuthorDisplayName">The display name of the message's author.</param>
/// <param name="Content">The message text.</param>
/// <param name="TimestampUtc">When the message was sent, in UTC.</param>
public sealed record ChannelMessage(
    string AuthorDisplayName,
    string Content,
    DateTimeOffset TimestampUtc);
