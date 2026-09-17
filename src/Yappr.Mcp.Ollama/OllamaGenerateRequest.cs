namespace Yappr.Mcp.Ollama;

using System.Text.Json.Serialization;

internal sealed record OllamaGenerateRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("stream")] bool Stream);
