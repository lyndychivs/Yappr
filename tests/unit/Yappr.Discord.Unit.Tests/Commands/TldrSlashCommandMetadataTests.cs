namespace Yappr.Discord.Unit.Tests.Commands;

using System.Reflection;

using NetCord.Services.ApplicationCommands;

using NUnit.Framework;

using Yappr.Discord.Commands;

/// <summary>
/// NetCord builds slash command registration from attributes at startup, so a typo or accidental removal
/// wouldn't fail a normal compile — these tests pin the public command surface exposed to Discord users.
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
