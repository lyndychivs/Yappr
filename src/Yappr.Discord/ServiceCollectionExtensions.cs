namespace Yappr.Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Yappr.Models;
using Yappr.ServiceDefaults;
using Yappr.Summarization.Mcp;

/// <summary>
/// Service registrations for the Discord bot host.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the HTTP client used to reach the MCP server, with timeouts taken from
    /// <see cref="McpOptions.Resilience"/>.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddMcpHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient(McpClientToolInvoker.HttpClientName)
            .AddStandardResilienceHandler()
            .ConfigureTimeouts(serviceProvider => serviceProvider.GetRequiredService<IOptions<McpOptions>>().Value.Resilience);

        return services;
    }
}
