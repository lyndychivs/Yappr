namespace Yappr.Unit.Tests.Summarization.Mcp;

using System;

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
        var invoker = new McpClientToolInvoker(Options.Create(new McpOptions
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
        var invoker = new McpClientToolInvoker(Options.Create(new McpOptions
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
        var invoker = new McpClientToolInvoker(Options.Create(new McpOptions
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
        var invoker = new McpClientToolInvoker(Options.Create(new McpOptions
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
        var invoker = new McpClientToolInvoker(Options.Create(new McpOptions
        {
            Transport = (McpTransportType)99,
        }));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => invoker.CreateTransport())!;

        Assert.That(exception.Message, Is.EqualTo("Unsupported Mcp:Transport value '99'."));
    }
}
