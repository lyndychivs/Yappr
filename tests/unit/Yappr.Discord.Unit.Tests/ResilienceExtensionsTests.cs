namespace Yappr.Discord.Unit.Tests;

using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using NUnit.Framework;

using Yappr.Models;
using Yappr.ServiceDefaults;

[TestFixture]
public sealed class ResilienceExtensionsTests
{
    [Test]
    public void ConfigureTimeouts_DiscordDefaults_AppliesClientTimeouts()
    {
        HttpStandardResilienceOptions applied = Apply(new McpOptions().Resilience);

        TimeSpan expected = McpResilienceOptions.ServerTimeout + McpResilienceOptions.ClientMargin;
        Assert.That(applied.AttemptTimeout.Timeout, Is.EqualTo(expected));
        Assert.That(applied.TotalRequestTimeout.Timeout, Is.EqualTo(expected));
    }

    [Test]
    public void ConfigureTimeouts_OnlyAttemptTimeoutRaised_SamplingDurationFollows()
    {
        var resilience = new McpResilienceOptions
        {
            AttemptTimeout = TimeSpan.FromMinutes(20),
            TotalRequestTimeout = TimeSpan.FromMinutes(20),
        };

        Assert.That(Apply(resilience).CircuitBreaker.SamplingDuration, Is.EqualTo(TimeSpan.FromMinutes(40)));
    }

    [Test]
    public void ConfigureTimeouts_TotalBelowAttempt_TotalIsRaisedToAttempt()
    {
        var resilience = new McpResilienceOptions
        {
            AttemptTimeout = TimeSpan.FromMinutes(20),
            TotalRequestTimeout = TimeSpan.FromMinutes(5),
        };

        Assert.That(Apply(resilience).TotalRequestTimeout.Timeout, Is.EqualTo(TimeSpan.FromMinutes(20)));
    }

    /// <summary>
    /// Applies <paramref name="resilience"/> and captures the resulting options by chaining a second
    /// <c>Configure</c> on the same builder, so the test does not depend on the handler's options name.
    /// Creating the client also runs the standard handler's option validation.
    /// </summary>
    private static HttpStandardResilienceOptions Apply(McpResilienceOptions resilience)
    {
        HttpStandardResilienceOptions? captured = null;
        var services = new ServiceCollection();
        services.AddHttpClient("test")
            .AddStandardResilienceHandler()
            .ConfigureTimeouts(_ => resilience)
            .Configure(options => captured = options);

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient("test"));

        return captured ?? throw new InvalidOperationException("The resilience options were never resolved.");
    }
}
