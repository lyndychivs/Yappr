namespace Yappr.Discord.Unit.Tests.Commands;

using System.Reflection;

using NetCord.Services.ApplicationCommands;

using NUnit.Framework;

using Yappr.Discord.Commands;

/// <summary>
/// Pins the `/tldr` slash command's public metadata, since NetCord builds registration from attributes and a
/// typo wouldn't fail compilation.
/// </summary>
[TestFixture]
public sealed class TldrSlashCommandMetadataTests
{
    [Test]
    public void TldrSlashCommand_IsRegisteredAsSlashCommandNamedTldr()
    {
        SlashCommandAttribute? attribute = typeof(TldrSlashCommand).GetCustomAttribute<SlashCommandAttribute>();

        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.Name, Is.EqualTo("tldr"));
    }

    [TestCase(nameof(TldrSlashCommand.DaysAsync), "days")]
    [TestCase(nameof(TldrSlashCommand.HoursAsync), "hours")]
    [TestCase(nameof(TldrSlashCommand.MessagesAsync), "messages")]
    public void TldrSlashCommand_HasExpectedSubcommand(string methodName, string expectedSubcommandName)
    {
        MethodInfo method = typeof(TldrSlashCommand).GetMethod(methodName)!;

        SubSlashCommandAttribute? attribute = method.GetCustomAttribute<SubSlashCommandAttribute>();

        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.Name, Is.EqualTo(expectedSubcommandName));
    }
}
