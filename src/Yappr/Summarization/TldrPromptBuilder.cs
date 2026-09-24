namespace Yappr.Summarization;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Yappr.Models.Dto;

/// <summary>
/// Renders a channel transcript into the summarization prompt, truncating the oldest messages if it would
/// exceed a reasonable size.
/// </summary>
public static class TldrPromptBuilder
{
    /// <summary>
    /// The maximum length, in characters, of the transcript portion of the prompt.
    /// </summary>
    public const int MaxPromptCharacters = 12_000;

    /// <summary>
    /// Builds the summarization prompt for <paramref name="messages"/>.
    /// </summary>
    /// <param name="messages">The channel transcript, oldest first.</param>
    /// <returns>The prompt to send to the summarization tool.</returns>
    public static string Build(IReadOnlyList<ChannelMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var lines = new List<string>(messages.Count);
        foreach (ChannelMessage message in messages)
        {
            lines.Add(string.Create(CultureInfo.InvariantCulture, $"[{message.TimestampUtc:yyyy-MM-dd HH:mm} UTC] {message.AuthorDisplayName}: {message.Content}"));
        }

        string transcript = BuildWithinBudget(lines);

        return "Summarise the following Discord channel conversation into a short, skimmable TL;DR. " +
            "Group related messages by topic, call out decisions or action items, and ignore small talk. " +
            "Reply in plain text suitable for a Discord message." +
            "\n\n---\n" +
            transcript;
    }

    private static string BuildWithinBudget(List<string> lines)
    {
        var builder = new StringBuilder();
        int start = 0;

        int totalLength = 0;
        foreach (string line in lines)
        {
            totalLength += line.Length + 1;
        }

        if (totalLength > MaxPromptCharacters)
        {
            int runningLength = 0;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                runningLength += lines[i].Length + 1;
                if (runningLength > MaxPromptCharacters)
                {
                    start = i + 1;
                    break;
                }
            }
        }

        for (int i = start; i < lines.Count; i++)
        {
            builder.AppendLine(lines[i]);
        }

        return builder.ToString();
    }
}
