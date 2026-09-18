namespace Yappr.Models;

/// <summary>
/// Configures the Ollama server used for summarization.
/// </summary>
public sealed class OllamaOptions
{
    /// <summary>
    /// The configuration section name this class binds to.
    /// </summary>
    public const string SectionName = "Ollama";

    /// <summary>
    /// Default model used when none is configured. Also used as the fallback in <c>Yappr.AppHost</c>.
    /// </summary>
    public const string DefaultModel = "llama3.2:3b";

    /// <summary>
    /// Gets or sets the base URL of the Ollama server.
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets the model used for summarization. Size to available RAM — larger models can be
    /// OOM-killed on constrained hosts.
    /// </summary>
    public string Model { get; set; } = DefaultModel;

    /// <summary>
    /// Gets or sets the HTTP resilience settings for the Ollama client.
    /// </summary>
    public McpResilienceOptions Resilience { get; set; } = new();
}
