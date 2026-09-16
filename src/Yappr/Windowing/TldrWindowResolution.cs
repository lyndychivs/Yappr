namespace Yappr.Windowing;

using System;

/// <summary>
/// The resolved, validated meaning of a `/tldr` window request: either a time cutoff (days/hours) or a
/// message count limit (messages), or a validation error if the requested value exceeded the configured cap.
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

    public bool IsValid { get; }

    public string? ValidationError { get; }

    public DateTimeOffset? SinceUtc { get; }

    public int? MessageLimit { get; }

    public static TldrWindowResolution ForTimeCutoff(DateTimeOffset sinceUtc) => new(isValid: true, validationError: null, sinceUtc, messageLimit: null);

    public static TldrWindowResolution ForMessageLimit(int messageLimit) => new(isValid: true, validationError: null, sinceUtc: null, messageLimit);

    public static TldrWindowResolution Invalid(string validationError) => new(isValid: false, validationError, sinceUtc: null, messageLimit: null);
}
