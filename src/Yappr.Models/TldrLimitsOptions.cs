namespace Yappr.Models;

public sealed class TldrLimitsOptions
{
    public const string SectionName = "TldrLimits";

    public int MaxDays { get; set; } = 30;

    public int MaxHours { get; set; } = 720;

    public int MaxMessages { get; set; } = 500;
}
