namespace Yappr.Windowing;

using System;
using System.Globalization;

using Yappr.Models.Enums;
using Yappr.Models.Options;

/// <summary>
/// Turns a `/tldr` subcommand into a validated <see cref="TldrWindowResolution"/>.
/// </summary>
internal static class TldrWindowResolver
{
    /// <summary>
    /// Validates a `/tldr` subcommand's numeric value against <paramref name="limits"/> and resolves it to a
    /// window.
    /// </summary>
    /// <param name="kind">The kind of window requested (days, hours, or messages).</param>
    /// <param name="value">The window's numeric value.</param>
    /// <param name="limits">The configured caps to validate against.</param>
    /// <returns>The resolved window, or a validation error if the value exceeded the configured cap.</returns>
    public static TldrWindowResolution Resolve(TldrWindowKind kind, int value, TldrLimitsOptions limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        return value <= 0
            ? TldrWindowResolution.Invalid("The value must be greater than zero.")
            : kind switch
        {
            TldrWindowKind.Days => value > limits.MaxDays
                ? TldrWindowResolution.Invalid(string.Create(CultureInfo.InvariantCulture, $"`days` can be at most {limits.MaxDays}."))
                : TldrWindowResolution.ForTimeCutoff(DateTimeOffset.UtcNow.AddDays(-value)),

            TldrWindowKind.Hours => value > limits.MaxHours
                ? TldrWindowResolution.Invalid(string.Create(CultureInfo.InvariantCulture, $"`hours` can be at most {limits.MaxHours}."))
                : TldrWindowResolution.ForTimeCutoff(DateTimeOffset.UtcNow.AddHours(-value)),

            TldrWindowKind.Messages => value > limits.MaxMessages
                ? TldrWindowResolution.Invalid(string.Create(CultureInfo.InvariantCulture, $"`messages` can be at most {limits.MaxMessages}."))
                : TldrWindowResolution.ForMessageLimit(value),

            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, message: null),
        };
    }
}
