namespace Yappr.Mcp.Ollama;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Yappr.Models;
using Yappr.ServiceDefaults;

/// <summary>
/// Service registrations for the Ollama MCP server host.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the HTTP client used to reach Ollama, with the base address and timeouts taken from
    /// <see cref="OllamaOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddOllamaHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient(OllamaChatTool.HttpClientName, (serviceProvider, client) =>
            {
                OllamaOptions options = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.Endpoint);
            })
            .AddStandardResilienceHandler()
            .ConfigureTimeouts(serviceProvider => serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value.Resilience);

        return services;
    }
}
