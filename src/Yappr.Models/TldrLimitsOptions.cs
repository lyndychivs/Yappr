namespace Yappr.Models;

/// <summary>
/// Caps on the window a `/tldr` request can cover.
/// </summary>
public sealed class TldrLimitsOptions
{
    /// <summary>
    /// The configuration section name this class binds to.
    /// </summary>
    public const string SectionName = "TldrLimits";

    /// <summary>
    /// Gets or sets the maximum number of days a `/tldr days` request can span.
    /// </summary>
    public int MaxDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of hours a `/tldr hours` request can span.
    /// </summary>
    public int MaxHours { get; set; } = 720;

    /// <summary>
    /// Gets or sets the maximum number of messages a `/tldr messages` request can span.
    /// </summary>
    public int MaxMessages { get; set; } = 500;
}
