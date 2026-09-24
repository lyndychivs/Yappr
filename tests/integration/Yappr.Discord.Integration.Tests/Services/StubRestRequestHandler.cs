namespace Yappr.Discord.Integration.Tests.Services;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetCord.Rest;

internal sealed class StubRestRequestHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : IRestRequestHandler
{
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(responder(request));
    }

    public void AddDefaultHeader(string name, IEnumerable<string> values)
    {
    }

    public void Dispose()
    {
    }
}
