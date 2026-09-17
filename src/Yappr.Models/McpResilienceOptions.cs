namespace Yappr.Models;

using System;

/// <summary>
/// Configures the HTTP resilience policy applied to the MCP client's <see cref="System.Net.Http.HttpClient"/>.
/// The defaults are wide because summarization tool calls (e.g. a local LLM cold-loading a model) can take
/// minutes, far longer than the resilience defaults applied to HTTP clients in general.
/// </summary>
public sealed class McpResilienceOptions
{
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromMinutes(10);
}
