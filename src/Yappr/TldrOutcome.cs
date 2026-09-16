namespace Yappr;

using Yappr.Models;

public sealed record TldrOutcome
{
    private TldrOutcome(bool isSuccess, string? error, SummaryResult? result)
    {
        this.IsSuccess = isSuccess;
        this.Error = error;
        this.Result = result;
    }

    public bool IsSuccess { get; }

    public string? Error { get; }

    public SummaryResult? Result { get; }

    public static TldrOutcome Success(SummaryResult result) => new(true, null, result);

    public static TldrOutcome Failure(string error) => new(false, error, null);
}
