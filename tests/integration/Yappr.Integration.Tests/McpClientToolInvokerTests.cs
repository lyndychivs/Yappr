namespace Yappr.Integration.Tests;

using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization.Mcp;

/// <summary>
/// Exercises <see cref="McpClientToolInvoker"/> against a real MCP server (<c>Yappr.McpTestServer</c>) over
/// stdio, so the actual ModelContextProtocol SDK wiring — not just a mocked interface — is verified. The test
/// server exposes a deterministic "chat" tool, standing in for a real "chatgpt" MCP server.
/// </summary>
[TestFixture]
public sealed class McpClientToolInvokerTests
{
    [Test]
    public async Task InvokeAsync_AgainstRealMcpServer_ReturnsToolResponse()
    {
        string serverDllPath = Assembly.Load("Yappr.McpTestServer").Location;

        var options = Options.Create(new McpOptions
        {
            Transport = McpTransportType.Stdio,
            Command = "dotnet",
            Arguments = [serverDllPath],
            ToolName = "chat",
        });

        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), options);

        string result = await invoker.InvokeAsync("hello from the integration test", CancellationToken.None);

        Assert.That(result, Is.EqualTo("ECHO: hello from the integration test"));
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
