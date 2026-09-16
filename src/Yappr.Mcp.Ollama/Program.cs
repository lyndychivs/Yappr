using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

builder.Services.AddMcpServer().WithHttpTransport().WithTools<OllamaChatTool>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

await app.RunAsync();
