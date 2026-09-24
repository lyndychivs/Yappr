namespace Yappr.Windowing;

using System;

/// <summary>
/// The validated `/tldr` window: a time cutoff, a message count limit, or a validation error if the request
/// exceeded the configured cap.
/// </summary>
public sealed record TldrWindowResolution
{
    private TldrWindowResolution(bool isValid, string? validationError, DateTimeOffset? sinceUtc, int? messageLimit)
    {
        IsValid = isValid;
        ValidationError = validationError;
        SinceUtc = sinceUtc;
        MessageLimit = messageLimit;
    }

    /// <summary>
    /// Gets a value indicating whether the requested window was valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the user-facing validation error when <see cref="IsValid"/> is <see langword="false"/>.
    /// </summary>
    public string? ValidationError { get; }

    /// <summary>
    /// Gets the time cutoff, when the window is a days/hours request.
    /// </summary>
    public DateTimeOffset? SinceUtc { get; }

    /// <summary>
    /// Gets the message count limit, when the window is a messages request.
    /// </summary>
    public int? MessageLimit { get; }

    /// <summary>
    /// Creates a valid window resolved to a time cutoff.
    /// </summary>
    /// <param name="sinceUtc">The earliest UTC timestamp to include.</param>
    /// <returns>A valid <see cref="TldrWindowResolution"/>.</returns>
    public static TldrWindowResolution ForTimeCutoff(DateTimeOffset sinceUtc)
    {
        return new(isValid: true, validationError: null, sinceUtc, messageLimit: null);
    }

    /// <summary>
    /// Creates a valid window resolved to a message count limit.
    /// </summary>
    /// <param name="messageLimit">The maximum number of messages to include.</param>
    /// <returns>A valid <see cref="TldrWindowResolution"/>.</returns>
    public static TldrWindowResolution ForMessageLimit(int messageLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messageLimit);

        return new(isValid: true, validationError: null, sinceUtc: null, messageLimit);
    }

    /// <summary>
    /// Creates an invalid window carrying a user-facing validation error.
    /// </summary>
    /// <param name="validationError">The reason the requested window was rejected.</param>
    /// <returns>An invalid <see cref="TldrWindowResolution"/>.</returns>
    public static TldrWindowResolution Invalid(string validationError)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(validationError);

        return new(isValid: false, validationError, sinceUtc: null, messageLimit: null);
    }
}
