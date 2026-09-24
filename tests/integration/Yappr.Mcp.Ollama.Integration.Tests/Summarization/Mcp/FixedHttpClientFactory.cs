namespace Yappr.Mcp.Ollama.Integration.Tests.Summarization.Mcp;

using System.Net.Http;

/// <summary>
/// An <see cref="IHttpClientFactory"/> that always returns the same pre-built <see cref="HttpClient"/>, for
/// pointing <see cref="Yappr.Summarization.Mcp.McpClientToolInvoker"/> at an in-memory
/// <c>WebApplicationFactory</c> test server.
/// </summary>
/// <param name="client">The client to always return.</param>
internal sealed class FixedHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return client;
    }
}
