namespace Yappr.Models;

using System;

public sealed record ChannelMessage(string AuthorDisplayName, string Content, DateTimeOffset TimestampUtc);
