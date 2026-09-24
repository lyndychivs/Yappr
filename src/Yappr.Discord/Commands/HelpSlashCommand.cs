namespace Yappr.Discord.Commands;

using System;

using Microsoft.Extensions.Logging;

using NetCord.Services.ApplicationCommands;

/// <summary>
/// The `/help` slash command.
/// </summary>
public sealed partial class HelpSlashCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<HelpSlashCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpSlashCommand"/> class.
    /// </summary>
    /// <param name="logger">The logger to record received interactions with.</param>
    public HelpSlashCommand(ILogger<HelpSlashCommand> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    /// <summary>
    /// Returns the `/tldr` usage help text.
    /// </summary>
    /// <returns>The help message to reply with.</returns>
    [SlashCommand("help", "Help with Yappr")]
    public string GetHelp()
    {
        LogReceivedInteraction(_logger, Context.Interaction.Data.Name, Context.User.Username, Context.User.Id);

        return "# <a:yap:1549927339935269026> Yappr `/tldr` parameters\n" +
            "**`/tldr days count:<n>`** : summarise the last `n` days of this yap\n" +
            "**`/tldr hours count:<n>`** : summarise the last `n` hours of this yap\n" +
            "**`/tldr messages count:<n>`** : summarise the last `n` messages of this yap";
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Received {Interaction} request from {Username} {UserId}")]
    private static partial void LogReceivedInteraction(ILogger logger, string interaction, string username, ulong userId);
}
