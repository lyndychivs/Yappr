namespace Yappr.Discord.Commands;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using Yappr.Models;

[SlashCommand("tldr", "Summarize recent channel activity")]
public sealed partial class TldrSlashCommand(ILogger<TldrSlashCommand> logger, TldrOrchestrator orchestrator)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("days", "Summarize the last N days of this channel")]
    public Task DaysAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of days to summarize", MinValue = 1)]
        int count)
        => RunAsync(TldrWindowKind.Days, count);

    [SubSlashCommand("hours", "Summarize the last N hours of this channel")]
    public Task HoursAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of hours to summarize", MinValue = 1)]
        int count)
        => RunAsync(TldrWindowKind.Hours, count);

    [SubSlashCommand("messages", "Summarize the last N messages of this channel")]
    public Task MessagesAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of messages to summarize", MinValue = 1)]
        int count)
        => RunAsync(TldrWindowKind.Messages, count);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Received /tldr {Kind}:{Count} request from {Username} {UserId}")]
    private static partial void LogReceivedInteraction(
        ILogger logger,
        TldrWindowKind kind,
        int count,
        string username,
        ulong userId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to summarize /tldr {Kind}:{Count} request from {Username} {UserId}")]
    private static partial void LogSummarizationFailed(
        ILogger logger,
        Exception exception,
        TldrWindowKind kind,
        int count,
        string username,
        ulong userId);

    private async Task RunAsync(TldrWindowKind kind, int count)
    {
        LogReceivedInteraction(logger, kind, count, Context.User.Username, Context.User.Id);

        // Summarization can take well over Discord's 3-second interaction response window (model load +
        // generation), so acknowledge immediately and edit the response once the summary is ready.
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
        {
            Content = "🐱 the cat is chewing through the yap...",
        }));

        string content;
        try
        {
            TldrOutcome outcome = await orchestrator.RunAsync(Context.Channel.Id, kind, count, CancellationToken.None);

            content = outcome.IsSuccess
                ? string.Create(
                    CultureInfo.InvariantCulture,
                    $"### 📋 TL;DR *(from {outcome.Result!.MessageCount} messages)*\n{outcome.Result.Summary}")
                : $"⚠️ {outcome.Error}";
        }
        catch (Exception exception)
        {
            LogSummarizationFailed(logger, exception, kind, count, Context.User.Username, Context.User.Id);
            content = "⚠️ Something went wrong while summarizing. Please try again.";
        }

        await ModifyResponseAsync(options => options.Content = content);
    }
}
