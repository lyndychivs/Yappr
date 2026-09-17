namespace Yappr.Mcp.Ollama.Unit.Tests;

using System.Net.Http;

internal sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return httpClient;
    }
}
