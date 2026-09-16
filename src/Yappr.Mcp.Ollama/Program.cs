using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Yappr.Mcp.Ollama;
using Yappr.Models;
using Yappr.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<OllamaOptions>().BindConfiguration(OllamaOptions.SectionName);

builder.Services.AddHttpClient(OllamaChatTool.HttpClientName, (serviceProvider, client) =>
{
    OllamaOptions options = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.Endpoint);
});

// ServiceDefaults applies a 30s total-request-timeout resilience handler to every HttpClient by default,
// which is far too short for local LLM generation (cold model load + inference can take minutes). Widen it
// for this client only.
builder.Services.Configure<HttpStandardResilienceOptions>(OllamaChatTool.HttpClientName, options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
});

builder.Services.AddMcpServer().WithHttpTransport().WithTools<OllamaChatTool>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

await app.RunAsync();
