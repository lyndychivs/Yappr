namespace Yappr.ServiceDefaults;

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

using Yappr.Models;

/// <summary>
/// Applies <see cref="McpResilienceOptions"/> to a standard HTTP resilience handler.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Configures the standard resilience handler's timeouts from <paramref name="settings"/>. Configuring through
    /// the handler's builder binds to the handler's own options name, unlike a hand-named
    /// <c>AddOptions&lt;HttpStandardResilienceOptions&gt;</c> registration that is silently ignored if the name is wrong.
    /// </summary>
    /// <remarks>
    /// The attempt and total timeouts are equal by default, so a slow LLM call is not retried within the total window.
    /// The total timeout is raised to at least the attempt timeout, and the circuit breaker sampling duration is raised to at least twice the attempt timeout, the minimum the standard
    /// handler accepts, so overriding only the attempt timeout cannot produce an invalid configuration.
    /// </remarks>
    /// <param name="builder">The resilience handler builder returned by <c>AddStandardResilienceHandler</c>.</param>
    /// <param name="settings">Selects the settings to apply.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IHttpStandardResiliencePipelineBuilder ConfigureTimeouts(
        this IHttpStandardResiliencePipelineBuilder builder,
        Func<IServiceProvider, McpResilienceOptions> settings)
    {
        return builder.Configure((options, serviceProvider) =>
        {
            McpResilienceOptions resilience = settings(serviceProvider);
            options.AttemptTimeout.Timeout = resilience.AttemptTimeout;
            options.TotalRequestTimeout.Timeout = TimeSpan.FromTicks(Math.Max(
                resilience.TotalRequestTimeout.Ticks,
                resilience.AttemptTimeout.Ticks));
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromTicks(Math.Max(
                resilience.CircuitBreakerSamplingDuration.Ticks,
                2 * resilience.AttemptTimeout.Ticks));
        });
    }
}
