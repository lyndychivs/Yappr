using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Yappr.McpTestServer;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EchoTools>();

await builder.Build().RunAsync();
