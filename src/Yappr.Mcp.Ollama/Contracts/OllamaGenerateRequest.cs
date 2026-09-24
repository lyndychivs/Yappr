namespace Yappr.Mcp.Ollama.Contracts;

using System.Text.Json.Serialization;

/// <summary>
/// The request body for Ollama's <c>/api/generate</c> endpoint.
/// </summary>
/// <param name="Model">The model to generate with.</param>
/// <param name="Prompt">The prompt to generate a response for.</param>
/// <param name="Stream">Whether to stream the response; always <see langword="false"/> here.</param>
internal sealed record OllamaGenerateRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("stream")] bool Stream);
