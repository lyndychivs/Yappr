namespace Yappr.Discord.Unit.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Polly.Timeout;

using Yappr.Models;
using Yappr.Summarization.Mcp;

[TestFixture]
public sealed class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddMcpHttpClient_NoConfiguration_AppliesClientTimeoutsToMcpClient()
    {
        using ServiceProvider provider = BuildProvider(new Dictionary<string, string?>(StringComparer.Ordinal));

        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient(McpClientToolInvoker.HttpClientName));

        HttpStandardResilienceOptions applied = GetApplied(provider);
        TimeSpan expected = McpResilienceOptions.ServerTimeout + McpResilienceOptions.ClientMargin;
        Assert.That(applied.AttemptTimeout.Timeout, Is.EqualTo(expected));
        Assert.That(applied.TotalRequestTimeout.Timeout, Is.EqualTo(expected));
    }

    [Test]
    public void AddMcpHttpClient_ConfiguredAttemptTimeout_IsAppliedToMcpClient()
    {
        using ServiceProvider provider = BuildProvider(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Mcp:Resilience:AttemptTimeout"] = "00:20:00",
            ["Mcp:Resilience:TotalRequestTimeout"] = "00:20:00",
        });

        Assert.DoesNotThrow(() => provider.GetRequiredService<IHttpClientFactory>().CreateClient(McpClientToolInvoker.HttpClientName));
        Assert.That(GetApplied(provider).AttemptTimeout.Timeout, Is.EqualTo(TimeSpan.FromMinutes(20)));
    }

    [Test]
    public void AddMcpHttpClient_ConfiguredTimeout_CancelsHungRequest()
    {
        using ServiceProvider provider = BuildProvider(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Mcp:Resilience:AttemptTimeout"] = "00:00:01",
            ["Mcp:Resilience:TotalRequestTimeout"] = "00:00:01",
        });
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(McpClientToolInvoker.HttpClientName);

        var stopwatch = Stopwatch.StartNew();
        Exception? thrown = Assert.CatchAsync(async () => await client.GetAsync(new Uri("http://localhost/")));
        stopwatch.Stop();

        Assert.That(thrown, Is.InstanceOf<TimeoutRejectedException>());
        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(10)));
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> configuration)
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddOptions<McpOptions>().BindConfiguration(McpOptions.SectionName);
        services.AddMcpHttpClient();
        services.AddHttpClient(McpClientToolInvoker.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => new HangingHandler());

        return services.BuildServiceProvider();
    }

    private static HttpStandardResilienceOptions GetApplied(ServiceProvider provider)
    {
        return provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>()
            .Get($"{McpClientToolInvoker.HttpClientName}-standard");
    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage();
        }
    }
}
