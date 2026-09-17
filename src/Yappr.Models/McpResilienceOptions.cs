namespace Yappr.Models;

using System;

/// <summary>
/// HTTP resilience settings for the MCP client. Defaults are wide because summarization calls (e.g. a
/// cold-loading local LLM) can take minutes.
/// </summary>
public sealed class McpResilienceOptions
{
    /// <summary>
    /// Gets or sets the timeout for a single attempt.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the timeout for the request, including all retry attempts.
    /// </summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the sampling window used to evaluate the circuit breaker's failure rate.
    /// </summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromMinutes(10);
}
