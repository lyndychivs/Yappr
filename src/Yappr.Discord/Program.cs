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

internal static class Program
{
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

        builder.Services.AddHttpClient(McpClientToolInvoker.HttpClientName);

        builder.Services.AddOptions<HttpStandardResilienceOptions>(McpClientToolInvoker.HttpClientName)
            .Configure<IOptions<McpOptions>>((options, mcpOptions) =>
            {
                McpResilienceOptions resilience = mcpOptions.Value.Resilience;
                options.AttemptTimeout.Timeout = resilience.AttemptTimeout;
                options.TotalRequestTimeout.Timeout = resilience.TotalRequestTimeout;
                options.CircuitBreaker.SamplingDuration = resilience.CircuitBreakerSamplingDuration;
            });

        builder.Services.AddScoped<IMessageFetcher, NetCordMessageFetcher>();
        builder.Services.AddScoped<IMcpToolInvoker, McpClientToolInvoker>();
        builder.Services.AddScoped<ISummariser, McpSummariser>();
        builder.Services.AddScoped<TldrOrchestrator>();

        IHost host = builder.Build();

        host.AddModules(typeof(Program).Assembly);

        await host.RunAsync();
    }
}
