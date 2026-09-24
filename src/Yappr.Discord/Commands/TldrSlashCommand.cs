namespace Yappr.Discord.Commands;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using Yappr.Models.Enums;

/// <summary>
/// The `/tldr` slash command and its `days`/`hours`/`messages` subcommands.
/// </summary>
[SlashCommand("tldr", "Summarise recent channel yap")]
public sealed partial class TldrSlashCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    /// <summary>
    /// The response content used when <see cref="RunAsync"/> catches an unexpected exception.
    /// </summary>
    private const string GenericFailureContent = "⚠️ Something went wrong while sniffing the yap, Please try again.";

    private readonly ILogger<TldrSlashCommand> _logger;

    private readonly TldrOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TldrSlashCommand"/> class.
    /// </summary>
    /// <param name="logger">The logger to record received interactions and failures with.</param>
    /// <param name="orchestrator">Runs the `/tldr` request.</param>
    public TldrSlashCommand(ILogger<TldrSlashCommand> logger, TldrOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(orchestrator);

        _logger = logger;
        _orchestrator = orchestrator;
    }

    /// <summary>
    /// Summarises the last <paramref name="count"/> days of the channel.
    /// </summary>
    /// <param name="count">The number of days to summarize.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    [SubSlashCommand("days", "Summarise the last N days of yap")]
    public Task DaysAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of days", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Days, count);
    }

    /// <summary>
    /// Summarises the last <paramref name="count"/> hours of the channel.
    /// </summary>
    /// <param name="count">The number of hours to summarize.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    [SubSlashCommand("hours", "Summarise the last N hours of this yap")]
    public Task HoursAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of hours", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Hours, count);
    }

    /// <summary>
    /// Summarises the last <paramref name="count"/> messages of the channel.
    /// </summary>
    /// <param name="count">The number of messages to summarize.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    [SubSlashCommand("messages", "Summarise the last N messages of this yap")]
    public Task MessagesAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of messages", MinValue = 1)]
        int count)
    {
        return RunAsync(TldrWindowKind.Messages, count);
    }

    /// <summary>
    /// Formats <paramref name="outcome"/> as the `/tldr` response content.
    /// </summary>
    /// <param name="outcome">The completed `/tldr` request's outcome.</param>
    /// <returns>The summary, or the user-facing failure reason, formatted for a Discord message.</returns>
    internal static string BuildResponseContent(TldrOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        return outcome.IsSuccess
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"### 📋 TL;DR *(from {outcome.Result!.MessageCount} messages)*\n{outcome.Result.Summary}")
            : $"⚠️ {outcome.Error}";
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
        LogReceivedInteraction(_logger, kind, count, Context.User.Username, Context.User.Id);

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
        {
            Content = "<a:yap:1549927339935269026> sniffing out the yap...",
        }));

        string content;
        try
        {
            TldrOutcome outcome = await _orchestrator.RunAsync(Context.Channel.Id, kind, count, CancellationToken.None);

            content = BuildResponseContent(outcome);
        }
        catch (Exception exception)
        {
            LogSummarisationFailed(_logger, exception, kind, count, Context.User.Username, Context.User.Id);
            content = GenericFailureContent;
        }

        await ModifyResponseAsync(options => options.Content = content);
    }
}
