namespace Yappr.Discord.Commands;

using Microsoft.Extensions.Logging;

using NetCord.Services.ApplicationCommands;

public sealed partial class HelpSlashCommand(ILogger<HelpSlashCommand> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("help", "Help with Yappr")]
    public string GetHelp()
    {
        LogReceivedInteraction(logger, Context.Interaction.Data.Name, Context.User.Username, Context.User.Id);

        return "# <a:yap:1549927339935269026> Yappr `/tldr` parameters\n" +
            "**`/tldr days count:<n>`** : summarize the last `n` days of this yap\n" +
            "**`/tldr hours count:<n>`** : summarize the last `n` hours of this yap\n" +
            "**`/tldr messages count:<n>`** : summarize the last `n` messages of this yap";
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Received {Interaction} request from {Username} {UserId}")]
    private static partial void LogReceivedInteraction(ILogger logger, string interaction, string username, ulong userId);
}
