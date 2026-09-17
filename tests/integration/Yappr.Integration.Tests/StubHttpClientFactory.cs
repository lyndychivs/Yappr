namespace Yappr.Integration.Tests;

using System.Net.Http;

internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new();
    }
}
