namespace Yappr;

using Yappr.Models;

/// <summary>
/// The result of running a `/tldr` request: either a summary or a user-facing failure reason.
/// </summary>
public sealed record TldrOutcome
{
    private TldrOutcome(bool isSuccess, string? error, SummaryResult? result)
    {
        IsSuccess = isSuccess;
        Error = error;
        Result = result;
    }

    /// <summary>
    /// Gets a value indicating whether the request succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the user-facing failure reason when <see cref="IsSuccess"/> is <see langword="false"/>.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the summary when <see cref="IsSuccess"/> is <see langword="true"/>.
    /// </summary>
    public SummaryResult? Result { get; }

    /// <summary>
    /// Creates a successful outcome carrying <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The generated summary.</param>
    /// <returns>A successful <see cref="TldrOutcome"/>.</returns>
    public static TldrOutcome Success(SummaryResult result)
    {
        return new(isSuccess: true, error: null, result);
    }

    /// <summary>
    /// Creates a failed outcome carrying <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The user-facing failure reason.</param>
    /// <returns>A failed <see cref="TldrOutcome"/>.</returns>
    public static TldrOutcome Failure(string error)
    {
        return new(isSuccess: false, error, result: null);
    }
}
