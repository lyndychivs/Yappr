namespace Yappr.Discord.Unit.Tests.Commands;

using System;

using NUnit.Framework;

using Yappr.Discord.Commands;

[TestFixture]
public sealed class HelpSlashCommandTests
{
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new HelpSlashCommand(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("logger"));
    }
}
