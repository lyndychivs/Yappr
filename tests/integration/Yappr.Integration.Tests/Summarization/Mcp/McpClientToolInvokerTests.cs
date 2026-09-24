namespace Yappr.Integration.Tests.Summarization.Mcp;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models.Enums;
using Yappr.Models.Options;
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

    [Test]
    public async Task InvokeAsync_ToolReturnsMultipleTextBlocks_ReturnsFirstTextBlock()
    {
        string serverDllPath = Assembly.Load("Yappr.McpTestServer").Location;

        var options = Options.Create(new McpOptions
        {
            Transport = McpTransportType.Stdio,
            Command = "dotnet",
            Arguments = [serverDllPath],
            ToolName = "multi",
        });

        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), options);

        string result = await invoker.InvokeAsync("hello from the integration test", CancellationToken.None);

        // The tool returns two text blocks, "FIRST" and "SECOND": this pins down that the *first* text block
        // wins, not the last.
        Assert.That(result, Is.EqualTo("FIRST"));
    }
}
