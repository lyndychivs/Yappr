namespace Yappr.Discord;

using System;

using Microsoft.Extensions.Options;

using Yappr.Models;

/// <summary>
/// Fails startup when the MCP client would give up before the MCP server does, which would surface a client timeout
/// instead of the server's own error.
/// </summary>
public sealed class McpOptionsValidator : IValidateOptions<McpOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, McpOptions options)
    {
        McpResilienceOptions resilience = options.Resilience;
        if (resilience.AttemptTimeout > options.ServerTimeout && resilience.TotalRequestTimeout > options.ServerTimeout)
        {
            return ValidateOptionsResult.Success;
        }

        string message = FormattableString.Invariant(
            $"Mcp:Resilience AttemptTimeout ({resilience.AttemptTimeout}) and TotalRequestTimeout ({resilience.TotalRequestTimeout}) must both exceed Mcp:ServerTimeout ({options.ServerTimeout}), otherwise the client times out before the MCP server does.");

        return ValidateOptionsResult.Fail(message);
    }
}
