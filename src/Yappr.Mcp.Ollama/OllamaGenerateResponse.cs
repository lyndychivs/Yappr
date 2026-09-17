namespace Yappr.Mcp.Ollama;

using System.Text.Json.Serialization;

/// <summary>
/// The response body from Ollama's <c>/api/generate</c> endpoint.
/// </summary>
/// <param name="Response">The generated text.</param>
internal sealed record OllamaGenerateResponse(
    [property: JsonPropertyName("response")] string? Response);
