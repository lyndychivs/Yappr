namespace Yappr.Integration.Tests.Summarization.Mcp;

using System.Net.Http;

internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new();
    }
}
