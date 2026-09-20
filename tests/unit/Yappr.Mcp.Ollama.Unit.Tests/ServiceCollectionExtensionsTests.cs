namespace Yappr.Mcp.Ollama.Unit.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Polly.Timeout;

using Yappr.Models;

[TestFixture]
public sealed class ServiceCollectionExtensionsTests
{
    private const string ClientName = nameof(OllamaChatTool);

    [Test]
    public void AddOllamaHttpClient_NoConfiguration_SetsBaseAddress()
    {
        using ServiceProvider provider = BuildProvider(new Dictionary<string, string?>(StringComparer.Ordinal));

        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName);

        Assert.That(client.BaseAddress, Is.EqualTo(new Uri(new OllamaOptions().Endpoint)));
    }

    [Test]
    public void AddOllamaHttpClient_ConfiguredTimeout_CancelsHungRequest()
    {
        using ServiceProvider provider = BuildProvider(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Ollama:Resilience:AttemptTimeout"] = "00:00:01",
            ["Ollama:Resilience:TotalRequestTimeout"] = "00:00:01",
        });
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName);

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
        services.AddOptions<OllamaOptions>().BindConfiguration(OllamaOptions.SectionName);
        services.AddOllamaHttpClient();
        services.AddHttpClient(ClientName).ConfigurePrimaryHttpMessageHandler(() => new HangingHandler());

        return services.BuildServiceProvider();
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
