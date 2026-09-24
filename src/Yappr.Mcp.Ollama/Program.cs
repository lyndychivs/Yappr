using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Yappr.Mcp.Ollama;
using Yappr.Mcp.Ollama.Tools;
using Yappr.Models.Options;
using Yappr.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<OllamaOptions>().BindConfiguration(OllamaOptions.SectionName);

builder.Services.AddOllamaHttpClient();

builder.Services.AddMcpServer().WithHttpTransport().WithTools<OllamaChatTool>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

await app.RunAsync();
