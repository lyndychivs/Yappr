namespace Yappr.Windowing;

using System;

using Yappr.Models;

/// <summary>
/// Turns a `/tldr` subcommand (days/hours/messages + a numeric value) into a validated window, enforcing the
/// configured caps so a single request can't page through unbounded Discord history or blow the MCP prompt size.
/// </summary>
public static class TldrWindowResolver
{
    public static TldrWindowResolution Resolve(TldrWindowKind kind, int value, TldrLimitsOptions limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        if (value <= 0)
        {
            return TldrWindowResolution.Invalid("The value must be greater than zero.");
        }

        return kind switch
        {
            TldrWindowKind.Days => value > limits.MaxDays
                ? TldrWindowResolution.Invalid($"`days` can be at most {limits.MaxDays}.")
                : TldrWindowResolution.ForTimeCutoff(DateTimeOffset.UtcNow.AddDays(-value)),

            TldrWindowKind.Hours => value > limits.MaxHours
                ? TldrWindowResolution.Invalid($"`hours` can be at most {limits.MaxHours}.")
                : TldrWindowResolution.ForTimeCutoff(DateTimeOffset.UtcNow.AddHours(-value)),

            TldrWindowKind.Messages => value > limits.MaxMessages
                ? TldrWindowResolution.Invalid($"`messages` can be at most {limits.MaxMessages}.")
                : TldrWindowResolution.ForMessageLimit(value),

            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }
}
