namespace Yappr.Mcp.Ollama.Unit.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Yappr.Models;

[TestFixture]
public sealed class OllamaChatToolTests
{
    [Test]
    public async Task Chat_ValidResponse_SendsModelAndPromptAndReturnsResponseText()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var handler = new StubHttpMessageHandler(async request =>
        {
            capturedRequest = request;
            capturedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"response":"a short summary"}"""),
            };
        });

        OllamaChatTool tool = CreateTool(handler, model: "llama3.1:8b", endpoint: "http://ollama:11434");

        string result = await tool.Chat("summarise this", CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("a short summary"));
            Assert.That(capturedRequest!.RequestUri!.AbsolutePath, Is.EqualTo("/api/generate"));
            Assert.That(capturedBody, Does.Contain("\"model\":\"llama3.1:8b\""));
            Assert.That(capturedBody, Does.Contain("\"prompt\":\"summarise this\""));
            Assert.That(capturedBody, Does.Contain("\"stream\":false"));
        }
    }

    [Test]
    public void Chat_NonSuccessStatusCode_ThrowsWithResponseBody()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"error":"model 'llama3.1:8b' not found, try pulling it first"}"""),
        }));

        OllamaChatTool tool = CreateTool(handler, model: "llama3.1:8b", endpoint: "http://ollama:11434");

        InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(() => tool.Chat("prompt", CancellationToken.None))!;

        Assert.That(exception.Message, Does.Contain("model 'llama3.1:8b' not found, try pulling it first"));
    }

    [Test]
    public void Chat_ResponseMissingTextField_Throws()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}"),
        }));

        OllamaChatTool tool = CreateTool(handler, model: "llama3.1:8b", endpoint: "http://ollama:11434");

        InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(() => tool.Chat("prompt", CancellationToken.None))!;

        Assert.That(exception.Message, Is.EqualTo("Ollama did not return a response."));
    }

    private static OllamaChatTool CreateTool(HttpMessageHandler handler, string model, string endpoint)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(endpoint) };

        var options = Options.Create(new OllamaOptions
        {
            Model = model,
            Endpoint = endpoint,
        });

        return new OllamaChatTool(new StubHttpClientFactory(httpClient), options);
    }
}
