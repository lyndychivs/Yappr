namespace Yappr.AppHost;

using System;
using System.Globalization;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Projects;

using Yappr.Models.Options;

/// <summary>
/// The Aspire app host: wires up the Ollama, MCP, and Discord services and their configuration.
/// </summary>
internal static class Program
{
    private const string DiscordServiceName = "discord";
    private const string McpServiceName = "mcp";
    private const string OllamaServiceName = "ollama";

    private static void ForwardTimeouts(
        IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> mcpService,
        IResourceBuilder<ProjectResource> discordService)
    {
        if (!TimeSpan.TryParse(builder.Configuration["Ollama:Resilience:AttemptTimeout"], CultureInfo.InvariantCulture, out TimeSpan serverTimeout))
        {
            return;
        }

        TimeSpan clientTimeout = serverTimeout + McpResilienceOptions.ClientMargin;
        mcpService
            .WithEnvironment("Ollama__Resilience__AttemptTimeout", serverTimeout.ToString("c", CultureInfo.InvariantCulture))
            .WithEnvironment("Ollama__Resilience__TotalRequestTimeout", serverTimeout.ToString("c", CultureInfo.InvariantCulture));
        discordService
            .WithEnvironment("Mcp__ServerTimeout", serverTimeout.ToString("c", CultureInfo.InvariantCulture))
            .WithEnvironment("Mcp__Resilience__AttemptTimeout", clientTimeout.ToString("c", CultureInfo.InvariantCulture))
            .WithEnvironment("Mcp__Resilience__TotalRequestTimeout", clientTimeout.ToString("c", CultureInfo.InvariantCulture));
    }

    private static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        if (!builder.ExecutionContext.IsPublishMode)
        {
            builder.AddDockerComposeEnvironment("yappr")
                .WithDashboard(dashboard => dashboard.WithHostPort(8080)
                    .WithForwardedHeaders(true));
        }

        IResourceBuilder<ContainerResource> ollama = builder.AddContainer(OllamaServiceName, "ollama/ollama")
            .WithVolume("ollama-models", "/root/.ollama")
            .WithEndpoint(targetPort: 11434, scheme: "http", name: "http");

        IResourceBuilder<ProjectResource> mcpService = builder.AddProject<Yappr_Mcp_Ollama>(McpServiceName)
            .WithEnvironment("Ollama__Endpoint", ollama.GetEndpoint("http"))
            .WithEnvironment("Ollama__Model", builder.Configuration["Ollama:Model"] ?? OllamaOptions.DefaultModel)
            .WaitFor(ollama);

        IResourceBuilder<ProjectResource> discordService = builder.AddProject<Yappr_Discord>(DiscordServiceName)
            .WithEnvironment("Discord__Token", builder.Configuration["Discord:Token"] ?? string.Empty)
            .WithEnvironment("Discord__PublicKey", builder.Configuration["Discord:PublicKey"] ?? string.Empty)
            .WithEnvironment("Mcp__Transport", "Http")
            .WithEnvironment("Mcp__HttpEndpoint", mcpService.GetEndpoint("http"))
            .WithEnvironment("Mcp__ToolName", "chat")
            .WaitFor(mcpService);

        ForwardTimeouts(builder, mcpService, discordService);

        if (!builder.ExecutionContext.IsPublishMode)
        {
            ollama.PublishAsDockerComposeService((resource, service) => service.Name = OllamaServiceName);
            mcpService.PublishAsDockerComposeService((resource, service) => service.Name = McpServiceName);
            discordService.PublishAsDockerComposeService((resource, service) => service.Name = DiscordServiceName);
        }

        builder.Build().Run();
    }
}
