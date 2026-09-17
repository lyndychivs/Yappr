namespace Yappr.Discord.Commands;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using Yappr.Models;

[SlashCommand("tldr", "Summarise recent channel yap")]
public sealed partial class TldrSlashCommand(ILogger<TldrSlashCommand> logger, TldrOrchestrator orchestrator)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("days", "Summarise the last N days of yap")]
    public Task DaysAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of days", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Days, count);
    }

    [SubSlashCommand("hours", "Summarise the last N hours of this yap")]
    public Task HoursAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of hours", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Hours, count);
    }

    [SubSlashCommand("messages", "Summarise the last N messages of this yap")]
    public Task MessagesAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of messages", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Messages, count);
    }

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
        Message = "Failed to summarise /tldr {Kind}:{Count} request from {Username} {UserId}")]
    private static partial void LogSummarisationFailed(
        ILogger logger,
        Exception exception,
        TldrWindowKind kind,
        int count,
        string username,
        ulong userId);

    private async Task RunAsync(TldrWindowKind kind, int count)
    {
        LogReceivedInteraction(logger, kind, count, Context.User.Username, Context.User.Id);

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
        {
            Content = "<a:yap:1549927339935269026> sniffing out the yap...",
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
            LogSummarisationFailed(logger, exception, kind, count, Context.User.Username, Context.User.Id);
            content = "⚠️ Something went wrong while sniffing the yap, Please try again.";
        }

        await ModifyResponseAsync(options => options.Content = content);
    }
}
