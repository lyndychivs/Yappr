namespace Yappr.Mcp.Ollama.Unit.Tests;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return responder(request);
    }
}
