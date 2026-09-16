namespace Yappr.Models;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// Gets or sets the base URL of the Ollama server (e.g. <c>http://ollama:11434</c>).
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets the name of the local model to use for summarization (e.g. <c>llama3.1:8b</c>).
    /// </summary>
    public string Model { get; set; } = "llama3.1:8b";
}
