namespace Yappr.Unit.Tests.Summarization.Mcp;

using System.Net.Http;

internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    /// <summary>
    /// The most recently created <see cref="HttpClient"/>, so tests can observe its lifetime.
    /// </summary>
    public HttpClient? LastCreatedClient { get; private set; }

    public HttpClient CreateClient(string name)
    {
        LastCreatedClient = new HttpClient();
        return LastCreatedClient;
    }
}
