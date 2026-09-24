namespace Yappr.Mcp.Ollama.Integration.Tests;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization.Mcp;

/// <summary>
/// Exercises <see cref="McpClientToolInvoker"/> over its HTTP transport, against a real MCP HTTP server
/// (<c>Yappr.Mcp.Ollama</c>'s <c>Program</c>) with a stubbed Ollama backend. The stdio transport is covered
/// separately, over a real subprocess, by <c>Yappr.Integration.Tests</c>; this fills the HTTP-transport half of
/// that same code path, which was previously only ever exercised indirectly via a raw <c>McpClient</c>.
/// </summary>
[TestFixture]
public sealed class McpClientToolInvokerHttpTests
{
    [Test]
    public async Task InvokeAsync_HttpTransportAgainstRealMcpServer_ReturnsToolResponse()
    {
        using var stubOllama = new StubOllamaServer();

        Environment.SetEnvironmentVariable("Ollama__Endpoint", stubOllama.Address);
        try
        {
            await using var factory = new WebApplicationFactory<Program>();

            // Not wrapped in `using`: McpClientToolInvoker's HTTP transport takes ownership (ownsHttpClient: true)
            // and disposes it itself.
            HttpClient httpClient = factory.CreateClient();

            var options = Options.Create(new McpOptions
            {
                Transport = McpTransportType.Http,
                HttpEndpoint = httpClient.BaseAddress!.ToString(),
                ToolName = "chat",
            });

            var invoker = new McpClientToolInvoker(new FixedHttpClientFactory(httpClient), options);

            string result = await invoker.InvokeAsync("summarise this", CancellationToken.None);

            Assert.That(result, Is.EqualTo("stubbed summary"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Ollama__Endpoint", value: null);
        }
    }
}
