namespace Yappr.Mcp.Ollama.Integration.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using NUnit.Framework;

[TestFixture]
public sealed class OllamaChatToolTests
{
    [Test]
    public async Task CallTool_Chat_AgainstRealMcpHttpServer_ReturnsStubbedOllamaResponse()
    {
        using var stubOllama = new StubOllamaServer();

        await WithMcpClientAsync(
            new Dictionary<string, string?>(StringComparer.Ordinal) { ["Ollama__Endpoint"] = stubOllama.Address },
            async client =>
            {
                CallToolResult result = await CallChatAsync(client);

                TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

                Assert.That(textBlock, Is.Not.Null);
                Assert.That(textBlock!.Text, Is.EqualTo("stubbed summary"));
            });
    }

    [Test]
    public async Task CallTool_Chat_OllamaServerErrors_ReturnsSanitizedMcpErrorResult()
    {
        // 400 (not one of the standard resilience handler's retried transient statuses) keeps this test fast;
        // the point is exercising the MCP error-mapping path, not the resilience pipeline.
        using var stubOllama = new StubOllamaServer(statusCode: 400, responseBody: """{"error":"model not found"}""");

        await WithMcpClientAsync(
            new Dictionary<string, string?>(StringComparer.Ordinal) { ["Ollama__Endpoint"] = stubOllama.Address },
            async client =>
            {
                CallToolResult result = await CallChatAsync(client);

                // OllamaChatTool.Chat throws InvalidOperationException on a non-success Ollama response; the MCP
                // SDK catches that at the protocol boundary and reports it as an error tool result rather than
                // propagating it as a transport-level exception - and deliberately sanitizes the message rather
                // than leaking the underlying exception's details (the Ollama status/body) to the client.
                TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result.IsError, Is.True);
                    Assert.That(textBlock, Is.Not.Null);
                    Assert.That(textBlock!.Text, Does.Contain("chat"));
                    Assert.That(textBlock.Text, Does.Not.Contain("model not found"));
                }
            });
    }

    [Test]
    public async Task CallTool_Chat_OllamaServerTimesOut_ReturnsSanitizedMcpErrorResult()
    {
        using var stubOllama = new StubOllamaServer(delay: TimeSpan.FromSeconds(2));

        var environmentVariables = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Ollama__Endpoint"] = stubOllama.Address,
            ["Ollama__Resilience__AttemptTimeout"] = "00:00:00.2",
            ["Ollama__Resilience__TotalRequestTimeout"] = "00:00:00.2",
        };

        await WithMcpClientAsync(
            environmentVariables,
            async client =>
            {
                CallToolResult result = await CallChatAsync(client);

                // The resilience pipeline's own TimeoutRejectedException takes the same unhandled-exception path
                // as OllamaChatTool's own thrown exceptions: caught at the protocol boundary, reported as an
                // error result, and sanitized rather than leaking internal timeout details.
                TextContentBlock? textBlock = result.Content.OfType<TextContentBlock>().FirstOrDefault();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result.IsError, Is.True);
                    Assert.That(textBlock, Is.Not.Null);
                    Assert.That(textBlock!.Text, Does.Contain("chat"));
                }
            });
    }

    private static async Task<CallToolResult> CallChatAsync(McpClient client)
    {
        var arguments = new Dictionary<string, object?>(StringComparer.Ordinal) { ["prompt"] = "summarise this" };

        return await client.CallToolAsync("chat", arguments, cancellationToken: CancellationToken.None);
    }

    private static async Task WithMcpClientAsync(IReadOnlyDictionary<string, string?> environmentVariables, Func<McpClient, Task> action)
    {
        foreach ((string name, string? value) in environmentVariables)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

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

            await action(client);
        }
        finally
        {
            foreach (string name in environmentVariables.Keys)
            {
                Environment.SetEnvironmentVariable(name, value: null);
            }
        }
    }
}
