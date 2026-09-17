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
}
