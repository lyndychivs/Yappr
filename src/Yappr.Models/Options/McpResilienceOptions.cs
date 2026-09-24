namespace Yappr.Models.Options;

using System;

/// <summary>
/// HTTP resilience settings for the MCP client. Defaults are wide because summarization calls (e.g. a
/// cold-loading local LLM) can take minutes.
/// </summary>
public sealed class McpResilienceOptions
{
    /// <summary>
    /// The default timeout the MCP server applies to its own outbound calls (e.g. to Ollama).
    /// </summary>
    public static readonly TimeSpan ServerTimeout = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Extra time the MCP client waits beyond <see cref="ServerTimeout"/>, so the server's error reaches the
    /// client instead of the client giving up first.
    /// </summary>
    public static readonly TimeSpan ClientMargin = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the timeout for a single attempt.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = ServerTimeout;

    /// <summary>
    /// Gets or sets the timeout for the request, including all retry attempts.
    /// </summary>
    public TimeSpan TotalRequestTimeout { get; set; } = ServerTimeout;

    /// <summary>
    /// Gets or sets the sampling window used to evaluate the circuit breaker's failure rate.
    /// </summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromMinutes(10);
}
