namespace Yappr.Discord;

using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

using Yappr.Models;
using Yappr.ServiceDefaults;
using Yappr.Summarization;
using Yappr.Summarization.Mcp;

/// <summary>
/// The Discord bot host: wires up DI, Discord gateway, and MCP client, then runs the bot.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The application entry point.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task that completes when the host shuts down.</returns>
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddOptions<McpOptions>().BindConfiguration(McpOptions.SectionName);
        builder.Services.AddOptions<TldrLimitsOptions>().BindConfiguration(TldrLimitsOptions.SectionName);

        builder.Services
            .AddDiscordGateway(options => options.Intents =
                GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent)
            .AddApplicationCommands();

        builder.Services.AddHttpClient(McpClientToolInvoker.HttpClientName)
            .AddStandardResilienceHandler()
            .ConfigureTimeouts(serviceProvider => serviceProvider.GetRequiredService<IOptions<McpOptions>>().Value.Resilience);

        builder.Services.AddScoped<IMessageFetcher, NetCordMessageFetcher>();
        builder.Services.AddScoped<IMcpToolInvoker, McpClientToolInvoker>();
        builder.Services.AddScoped<ISummariser, McpSummariser>();
        builder.Services.AddScoped<TldrOrchestrator>();

        IHost host = builder.Build();

        host.AddModules(typeof(Program).Assembly);

        await host.RunAsync();
    }
}
