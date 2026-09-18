namespace Yappr.Models;

/// <summary>
/// The kind of window a `/tldr` request specifies.
/// </summary>
public enum TldrWindowKind
{
    /// <summary>
    /// A cutoff expressed in days.
    /// </summary>
    Days,

    /// <summary>
    /// A cutoff expressed in hours.
    /// </summary>
    Hours,

    /// <summary>
    /// A limit on the number of messages.
    /// </summary>
    Messages,
}
