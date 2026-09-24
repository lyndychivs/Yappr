namespace Yappr.Unit.Tests.Summarization.Mcp;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Summarization.Mcp;

[TestFixture]
public sealed class McpClientToolInvokerTests
{
    [Test]
    public void CreateTransport_StdioWithCommand_ReturnsStdioTransport()
    {
        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), Options.Create(new McpOptions
        {
            Transport = McpTransportType.Stdio,
            Command = "some-mcp-server",
        }));

        IClientTransport transport = invoker.CreateTransport();

        Assert.That(transport, Is.InstanceOf<StdioClientTransport>());
        Assert.That(((StdioClientTransport)transport).Name, Is.EqualTo("Yappr"));
    }

    [Test]
    public void CreateTransport_StdioWithoutCommand_Throws()
    {
        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), Options.Create(new McpOptions
        {
            Transport = McpTransportType.Stdio,
            Command = null,
        }));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => invoker.CreateTransport())!;

        Assert.That(exception.Message, Is.EqualTo("Mcp:Command must be configured for the Stdio transport."));
    }

    [Test]
    public void CreateTransport_HttpWithEndpoint_ReturnsHttpTransport()
    {
        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), Options.Create(new McpOptions
        {
            Transport = McpTransportType.Http,
            HttpEndpoint = "https://example.test/mcp",
        }));

        IClientTransport transport = invoker.CreateTransport();

        Assert.That(transport, Is.InstanceOf<HttpClientTransport>());
    }

    [Test]
    public async Task CreateTransport_HttpWithEndpoint_OwnsAndDisposesHttpClient()
    {
        var httpClientFactory = new StubHttpClientFactory();
        var invoker = new McpClientToolInvoker(httpClientFactory, Options.Create(new McpOptions
        {
            Transport = McpTransportType.Http,
            HttpEndpoint = "https://example.test/mcp",
        }));

        IClientTransport transport = invoker.CreateTransport();
        await ((HttpClientTransport)transport).DisposeAsync();

        Assert.That(
            () => httpClientFactory.LastCreatedClient!.Timeout = TimeSpan.FromSeconds(5),
            Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void CreateTransport_HttpWithoutEndpoint_Throws()
    {
        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), Options.Create(new McpOptions
        {
            Transport = McpTransportType.Http,
            HttpEndpoint = null,
        }));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => invoker.CreateTransport())!;

        Assert.That(exception.Message, Is.EqualTo("Mcp:HttpEndpoint must be configured for the Http transport."));
    }

    [Test]
    public void CreateTransport_UnsupportedTransportType_Throws()
    {
        var invoker = new McpClientToolInvoker(new StubHttpClientFactory(), Options.Create(new McpOptions
        {
            Transport = (McpTransportType)99,
        }));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => invoker.CreateTransport())!;

        Assert.That(exception.Message, Is.EqualTo("Unsupported Mcp:Transport value '99'."));
    }

    [Test]
    public async Task InvokeAsync_ResultHasTextBlock_PassesArgumentsThroughAndReturnsExtractedText()
    {
        IClientTransport? capturedTransport = null;
        string? capturedToolName = null;
        IReadOnlyDictionary<string, object?>? capturedArguments = null;
        CancellationToken capturedToken = default;

        Task<CallToolResult> CallTool(
            IClientTransport transport,
            string toolName,
            IReadOnlyDictionary<string, object?> arguments,
            CancellationToken cancellationToken)
        {
            capturedTransport = transport;
            capturedToolName = toolName;
            capturedArguments = arguments;
            capturedToken = cancellationToken;

            return Task.FromResult(new CallToolResult { Content = [new TextContentBlock { Text = "the summary" }] });
        }

        var invoker = new McpClientToolInvoker(
            new StubHttpClientFactory(),
            Options.Create(new McpOptions
            {
                Transport = McpTransportType.Stdio,
                Command = "some-mcp-server",
                ToolName = "chat",
            }),
            CallTool);

        using var cancellationTokenSource = new CancellationTokenSource();

        string result = await invoker.InvokeAsync("hello", cancellationTokenSource.Token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("the summary"));
            Assert.That(capturedTransport, Is.InstanceOf<StdioClientTransport>());
            Assert.That(capturedToolName, Is.EqualTo("chat"));
            Assert.That(capturedArguments!["prompt"], Is.EqualTo("hello"));
            Assert.That(capturedToken, Is.EqualTo(cancellationTokenSource.Token));
        }
    }

    [Test]
    public void InvokeAsync_CallToolReturnsNoTextContent_Throws()
    {
        var invoker = new McpClientToolInvoker(
            new StubHttpClientFactory(),
            Options.Create(new McpOptions
            {
                Transport = McpTransportType.Stdio,
                Command = "some-mcp-server",
                ToolName = "silent",
            }),
            (_, _, _, _) => Task.FromResult(new CallToolResult { Content = [] }));

        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => invoker.InvokeAsync("hello", CancellationToken.None));

        Assert.That(exception!.Message, Is.EqualTo("The MCP tool 'silent' did not return any text content."));
    }

    [Test]
    public void ExtractText_ResultHasTextBlock_ReturnsItsText()
    {
        var result = new CallToolResult
        {
            Content = [new TextContentBlock { Text = "hello" }],
        };

        string text = McpClientToolInvoker.ExtractText(result, "chat");

        Assert.That(text, Is.EqualTo("hello"));
    }

    [Test]
    public void ExtractText_ResultHasMultipleTextBlocks_ReturnsFirstOne()
    {
        var result = new CallToolResult
        {
            Content = [new TextContentBlock { Text = "FIRST" }, new TextContentBlock { Text = "SECOND" }],
        };

        string text = McpClientToolInvoker.ExtractText(result, "chat");

        Assert.That(text, Is.EqualTo("FIRST"));
    }

    [Test]
    public void ExtractText_ResultHasNoTextBlock_Throws()
    {
        var result = new CallToolResult { Content = [] };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => McpClientToolInvoker.ExtractText(result, "chat"))!;

        Assert.That(exception.Message, Is.EqualTo("The MCP tool 'chat' did not return any text content."));
    }

    [Test]
    public void BuildArguments_ReturnsSinglePromptEntryUnderOrdinalPromptKey()
    {
        IReadOnlyDictionary<string, object?> arguments = McpClientToolInvoker.BuildArguments("summarise this");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(arguments, Has.Count.EqualTo(1));
            Assert.That(arguments["prompt"], Is.EqualTo("summarise this"));
            Assert.That(arguments.ContainsKey("PROMPT"), Is.False, "the key comparer must be ordinal (case-sensitive), not case-insensitive");
        }
    }
}
