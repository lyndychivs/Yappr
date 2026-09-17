namespace Yappr;

using Yappr.Models;

public sealed record TldrOutcome
{
    private TldrOutcome(bool isSuccess, string? error, SummaryResult? result)
    {
        IsSuccess = isSuccess;
        Error = error;
        Result = result;
    }

    public bool IsSuccess { get; }

    public string? Error { get; }

    public SummaryResult? Result { get; }

    public static TldrOutcome Success(SummaryResult result)
    {
        return new(isSuccess: true, error: null, result);
    }

    public static TldrOutcome Failure(string error)
    {
        return new(isSuccess: false, error, result: null);
    }
}
