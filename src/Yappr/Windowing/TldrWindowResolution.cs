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
        this.IsValid = isValid;
        this.ValidationError = validationError;
        this.SinceUtc = sinceUtc;
        this.MessageLimit = messageLimit;
    }

    public bool IsValid { get; }

    public string? ValidationError { get; }

    public DateTimeOffset? SinceUtc { get; }

    public int? MessageLimit { get; }

    public static TldrWindowResolution ForTimeCutoff(DateTimeOffset sinceUtc) => new(true, null, sinceUtc, null);

    public static TldrWindowResolution ForMessageLimit(int messageLimit) => new(true, null, null, messageLimit);

    public static TldrWindowResolution Invalid(string validationError) => new(false, validationError, null, null);
}
