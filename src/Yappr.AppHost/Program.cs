namespace Yappr.AppHost;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Projects;

internal static class Program
{
    private const string DiscordServiceName = "discord";
    private const string McpServiceName = "mcp";
    private const string OllamaServiceName = "ollama";

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
            .WithEnvironment("Ollama__Model", builder.Configuration["Ollama:Model"] ?? "llama3.2:3b")
            .WaitFor(ollama);

        IResourceBuilder<ProjectResource> discordService = builder.AddProject<Yappr_Discord>(DiscordServiceName)
            .WithEnvironment("Discord__Token", builder.Configuration["Discord:Token"] ?? string.Empty)
            .WithEnvironment("Discord__PublicKey", builder.Configuration["Discord:PublicKey"] ?? string.Empty)
            .WithEnvironment("Mcp__Transport", "Http")
            .WithEnvironment("Mcp__HttpEndpoint", mcpService.GetEndpoint("http"))
            .WithEnvironment("Mcp__ToolName", "chat")
            .WaitFor(mcpService);

        if (!builder.ExecutionContext.IsPublishMode)
        {
            ollama.PublishAsDockerComposeService((resource, service) => service.Name = OllamaServiceName);
            mcpService.PublishAsDockerComposeService((resource, service) => service.Name = McpServiceName);
            discordService.PublishAsDockerComposeService((resource, service) => service.Name = DiscordServiceName);
        }

        builder.Build().Run();
    }
}
