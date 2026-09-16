namespace Yappr.AppHost;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Projects;

internal static class Program
{
    private const string DiscordServiceName = "yappr-discord";

    private static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        if (!builder.ExecutionContext.IsPublishMode)
        {
            builder.AddDockerComposeEnvironment("yappr")
                .WithDashboard(dashboard => dashboard.WithHostPort(8080)
                    .WithForwardedHeaders(true));
        }

        IResourceBuilder<ProjectResource> discordService = builder.AddProject<Yappr_Discord>(DiscordServiceName)
            .WithEnvironment("Discord__Token", builder.Configuration["Discord:Token"] ?? string.Empty)
            .WithEnvironment("Discord__PublicKey", builder.Configuration["Discord:PublicKey"] ?? string.Empty);

        if (!builder.ExecutionContext.IsPublishMode)
        {
            discordService.PublishAsDockerComposeService((resource, service) => service.Name = DiscordServiceName);
        }

        builder.Build().Run();
    }
}
