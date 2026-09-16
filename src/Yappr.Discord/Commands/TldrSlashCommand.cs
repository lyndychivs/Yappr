namespace Yappr.Discord.Commands;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetCord.Services.ApplicationCommands;

using Yappr.Models;

[SlashCommand("tldr", "Summarize recent channel activity")]
public sealed partial class TldrSlashCommand(ILogger<TldrSlashCommand> logger, TldrOrchestrator orchestrator)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("days", "Summarize the last N days of this channel")]
    public Task<string> DaysAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of days to summarize", MinValue = 1)]
        int count)
        => this.RunAsync(TldrWindowKind.Days, count);

    [SubSlashCommand("hours", "Summarize the last N hours of this channel")]
    public Task<string> HoursAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of hours to summarize", MinValue = 1)]
        int count)
        => this.RunAsync(TldrWindowKind.Hours, count);

    [SubSlashCommand("messages", "Summarize the last N messages of this channel")]
    public Task<string> MessagesAsync(
        [SlashCommandParameter(Name = "count", Description = "Number of messages to summarize", MinValue = 1)]
        int count)
        => this.RunAsync(TldrWindowKind.Messages, count);

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

    private async Task<string> RunAsync(TldrWindowKind kind, int count)
    {
        LogReceivedInteraction(logger, kind, count, this.Context.User.Username, this.Context.User.Id);

        TldrOutcome outcome = await orchestrator.RunAsync(this.Context.Channel.Id, kind, count, CancellationToken.None);

        return outcome.IsSuccess
            ? $"### 📋 TL;DR *(from {outcome.Result!.MessageCount} messages)*\n{outcome.Result.Summary}"
            : $"⚠️ {outcome.Error}";
    }
}
