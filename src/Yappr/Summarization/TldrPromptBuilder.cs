namespace Yappr.Summarization;

using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Yappr.Models;

/// <summary>
/// Renders a channel transcript into the prompt sent to the summarisation tool. Truncates from the oldest
/// end when the transcript would otherwise blow past a reasonable MCP prompt size.
/// </summary>
public static class TldrPromptBuilder
{
    public const int MaxPromptCharacters = 12_000;

    public static string Build(IReadOnlyList<ChannelMessage> messages)
    {
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
