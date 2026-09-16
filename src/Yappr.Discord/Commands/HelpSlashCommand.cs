namespace Yappr.Discord.Commands;

using Microsoft.Extensions.Logging;

using NetCord.Services.ApplicationCommands;

public sealed partial class HelpSlashCommand(ILogger<HelpSlashCommand> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("help", "Help with Yappr")]
    public string GetHelp()
    {
        LogReceivedInteraction(logger, this.Context.Interaction.Data.Name, this.Context.User.Username, this.Context.User.Id);

        return "# 🗣️ Yappr — `/tldr` parameters\n" +
            "**`/tldr days count:<n>`** — summarize the last `n` days of this channel\n" +
            "**`/tldr hours count:<n>`** — summarize the last `n` hours of this channel\n" +
            "**`/tldr messages count:<n>`** — summarize the last `n` messages of this channel";
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Received {Interaction} request from {Username} {UserId}")]
    private static partial void LogReceivedInteraction(ILogger logger, string interaction, string username, ulong userId);
}
