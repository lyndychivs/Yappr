namespace Yappr.Discord.Unit.Tests;

using System;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;

[TestFixture]
public sealed class McpOptionsValidatorTests
{
    private readonly McpOptionsValidator validator = new();

    [Test]
    public void Validate_Defaults_Succeeds()
    {
        Assert.That(this.validator.Validate(null, new McpOptions()).Succeeded, Is.True);
    }

    [Test]
    public void Validate_ServerTimeoutRaisedAlone_Fails()
    {
        var options = new McpOptions { ServerTimeout = TimeSpan.FromMinutes(10) };

        ValidateOptionsResult result = this.validator.Validate(null, options);

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("Mcp:ServerTimeout"));
    }

    [Test]
    public void Validate_ClientAttemptEqualToServer_Fails()
    {
        var options = new McpOptions();
        options.Resilience.AttemptTimeout = options.ServerTimeout;

        Assert.That(this.validator.Validate(null, options).Failed, Is.True);
    }

    [Test]
    public void Validate_ClientTotalEqualToServer_Fails()
    {
        var options = new McpOptions();
        options.Resilience.TotalRequestTimeout = options.ServerTimeout;

        Assert.That(this.validator.Validate(null, options).Failed, Is.True);
    }

    [Test]
    public void Validate_ClientAndServerRaisedTogether_Succeeds()
    {
        var options = new McpOptions { ServerTimeout = TimeSpan.FromMinutes(10) };
        options.Resilience.AttemptTimeout = TimeSpan.FromMinutes(11);
        options.Resilience.TotalRequestTimeout = TimeSpan.FromMinutes(11);

        Assert.That(this.validator.Validate(null, options).Succeeded, Is.True);
    }
}
