namespace Yappr.Integration.Tests;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization.Mcp;

/// <summary>
/// Exercises <see cref="McpClientToolInvoker"/> against a real MCP server (<c>Yappr.McpTestServer</c>) over stdio.
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

    [Test]
    public void InvokeAsync_ToolReturnsNoTextContent_Throws()
    {
        string serverDllPath = Assembly.Load("Yappr.McpTestServer").Location;

        var options = Options.Create(new McpOptions
        {
            Transport = McpTransportType.Stdio,
            Command = "dotnet",
            Arguments = [serverDllPath],
            ToolName = "silent",
        });

        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), options);

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => invoker.InvokeAsync("hello from the integration test", CancellationToken.None));

        Assert.That(exception!.Message, Is.EqualTo("The MCP tool 'silent' did not return any text content."));
    }
}
