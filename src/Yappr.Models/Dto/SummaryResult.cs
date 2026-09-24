namespace Yappr.Models.Dto;

/// <summary>
/// The result of summarizing a channel transcript.
/// </summary>
/// <param name="Summary">The generated summary text.</param>
/// <param name="MessageCount">The number of messages summarized.</param>
public sealed record SummaryResult(
    string Summary,
    int MessageCount);
