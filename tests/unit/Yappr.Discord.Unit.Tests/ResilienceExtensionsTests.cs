namespace Yappr.Discord.Unit.Tests;

using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;
using Yappr.ServiceDefaults;

[TestFixture]
public sealed class ResilienceExtensionsTests
{
    [Test]
    public void ConfigureTimeouts_DiscordDefaults_AppliesClientTimeoutsAndCreatesClient()
    {
        ServiceProvider provider = BuildProvider(new McpOptions().Resilience);

        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient("test"));

        HttpStandardResilienceOptions applied = GetApplied(provider);
        TimeSpan expected = McpResilienceOptions.ServerTimeout + McpResilienceOptions.ClientMargin;
        Assert.That(applied.AttemptTimeout.Timeout, Is.EqualTo(expected));
        Assert.That(applied.TotalRequestTimeout.Timeout, Is.EqualTo(expected));
    }

    [Test]
    public void McpOptionsDefaults_ClientAttemptTimeout_ExceedsServerAttemptTimeout()
    {
        Assert.That(
            new McpOptions().Resilience.AttemptTimeout,
            Is.GreaterThan(new OllamaOptions().Resilience.AttemptTimeout));
    }

    [Test]
    public void ConfigureTimeouts_OnlyAttemptTimeoutRaised_SamplingDurationFollowsAndClientIsValid()
    {
        var resilience = new McpResilienceOptions
        {
            AttemptTimeout = TimeSpan.FromMinutes(20),
            TotalRequestTimeout = TimeSpan.FromMinutes(20),
        };
        ServiceProvider provider = BuildProvider(resilience);

        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient("test"));
        Assert.That(GetApplied(provider).CircuitBreaker.SamplingDuration, Is.EqualTo(TimeSpan.FromMinutes(40)));
    }

    [Test]
    public void ConfigureTimeouts_TotalBelowAttempt_TotalIsRaisedToAttempt()
    {
        var resilience = new McpResilienceOptions
        {
            AttemptTimeout = TimeSpan.FromMinutes(20),
            TotalRequestTimeout = TimeSpan.FromMinutes(5),
        };
        ServiceProvider provider = BuildProvider(resilience);

        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient("test"));
        Assert.That(GetApplied(provider).TotalRequestTimeout.Timeout, Is.EqualTo(TimeSpan.FromMinutes(20)));
    }

    private static ServiceProvider BuildProvider(McpResilienceOptions resilience)
    {
        var services = new ServiceCollection();
        services.AddHttpClient("test")
            .AddStandardResilienceHandler()
            .ConfigureTimeouts(_ => resilience);

        return services.BuildServiceProvider();
    }

    private static HttpStandardResilienceOptions GetApplied(ServiceProvider provider)
    {
        return provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get("test-standard");
    }
}
