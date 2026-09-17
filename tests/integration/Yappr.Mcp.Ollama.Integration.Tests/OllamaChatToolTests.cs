namespace Yappr.Mcp.Ollama.Integration.Tests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using NUnit.Framework;

using Yappr.Mcp.Ollama;

/// <summary>
/// Exercises the real <c>Yappr.Mcp.Ollama</c> ASP.NET Core host and its MCP HTTP transport wiring end-to-end,
/// with the outbound call to Ollama pointed at a stub HTTP listener instead of a real Ollama instance — so the
/// test stays hermetic (no Docker, no real model) while still proving the server's "chat" tool round-trips
/// correctly over real HTTP, both to the MCP client and to its Ollama backend.
/// </summary>
[TestFixture]
public sealed class OllamaChatToolTests
{
    [Test]
    public async Task CallTool_Chat_AgainstRealMcpHttpServer_ReturnsStubbedOllamaResponse()
    {
        using var stubOllama = new StubOllamaServer();

        Environment.SetEnvironmentVariable("Ollama__Endpoint", stubOllama.Address);
        try
        {
            await using var factory = new WebApplicationFactory<Program>();

            using HttpClient httpClient = factory.CreateClient();

            var transport = new HttpClientTransport(
                new HttpClientTransportOptions { Endpoint = httpClient.BaseAddress! },
                httpClient,
                loggerFactory: null,
                ownsHttpClient: false);

            await using McpClient client = await McpClient.CreateAsync(transport, cancellationToken: CancellationToken.None);

            var arguments = new Dictionary<string, object?>(StringComparer.Ordinal) { ["prompt"] = "summarise this" };
            CallToolResult result = await client.CallToolAsync("chat", arguments, cancellationToken: CancellationToken.None);

            TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

            Assert.That(textBlock, Is.Not.Null);
            Assert.That(textBlock!.Text, Is.EqualTo("stubbed summary"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Ollama__Endpoint", value: null);
        }
    }

    /// <summary>
    /// A real HTTP listener standing in for Ollama's <c>/api/generate</c> endpoint, so the tool under test
    /// makes a genuine outbound HTTP call (exercising its real <c>HttpClient</c>/<c>BaseAddress</c> wiring)
    /// without depending on an actual Ollama instance being available.
    /// </summary>
    private sealed class StubOllamaServer : IDisposable
    {
        private readonly HttpListener listener = new();
        private readonly CancellationTokenSource cts = new();

        public StubOllamaServer()
        {
            int port = GetFreeTcpPort();
            Address = string.Create(CultureInfo.InvariantCulture, $"http://127.0.0.1:{port}");
            listener.Prefixes.Add($"{Address}/");
            listener.Start();
            _ = AcceptLoopAsync();
        }

        public string Address { get; }

        public void Dispose()
        {
            cts.Cancel();
            listener.Close();
            cts.Dispose();
        }

        private static int GetFreeTcpPort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }

        private async Task AcceptLoopAsync()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    HttpListenerContext context = await listener.GetContextAsync().WaitAsync(cts.Token);
                    byte[] buffer = Encoding.UTF8.GetBytes("""{"response":"stubbed summary"}""");
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, cts.Token);
                    context.Response.OutputStream.Close();
                }
            }
            catch (Exception ex) when (cts.IsCancellationRequested
                && ex is OperationCanceledException or ObjectDisposedException or HttpListenerException)
            {
            }
        }
    }
}
