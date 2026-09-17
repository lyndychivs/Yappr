namespace Yappr.Models;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// Gets or sets the default model to use when none is configured (e.g. via <c>Ollama:Model</c>). Also used
    /// as the fallback in <c>Yappr.AppHost</c>, so both entry points stay in sync.
    /// </summary>
    public const string DefaultModel = "llama3.2:3b";

    /// <summary>
    /// Gets or sets the base URL of the Ollama server (e.g. <c>http://ollama:11434</c>).
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets the name of the local model to use for summarization (e.g. <c>llama3.2:3b</c>). Size this
    /// to available RAM — larger models (e.g. <c>llama3.1:8b</c>) need several GB to load and can be killed by
    /// the OOM killer on constrained hosts.
    /// </summary>
    public string Model { get; set; } = DefaultModel;

    public McpResilienceOptions Resilience { get; set; } = new();
}
