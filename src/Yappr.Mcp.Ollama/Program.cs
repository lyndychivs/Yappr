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

builder.Services.AddOptions<HttpStandardResilienceOptions>(OllamaChatTool.HttpClientName)
    .Configure<IOptions<OllamaOptions>>((options, ollamaOptions) =>
    {
        McpResilienceOptions resilience = ollamaOptions.Value.Resilience;
        options.AttemptTimeout.Timeout = resilience.AttemptTimeout;
        options.TotalRequestTimeout.Timeout = resilience.TotalRequestTimeout;
        options.CircuitBreaker.SamplingDuration = resilience.CircuitBreakerSamplingDuration;
    });

builder.Services.AddMcpServer().WithHttpTransport().WithTools<OllamaChatTool>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

await app.RunAsync();
