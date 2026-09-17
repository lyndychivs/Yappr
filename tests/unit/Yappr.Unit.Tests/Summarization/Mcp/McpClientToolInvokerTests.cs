namespace Yappr.Unit.Tests.Summarization.Mcp;

using System;
using System.Collections.Generic;
using System.Net.Http;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Client;

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

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new();
        }
    }
}
