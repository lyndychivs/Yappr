namespace Yappr.Mcp.Ollama;

using System.Text.Json.Serialization;

internal sealed record OllamaGenerateResponse(
    [property: JsonPropertyName("response")] string? Response);
