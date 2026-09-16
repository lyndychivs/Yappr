namespace Yappr.Discord;

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

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

        // ServiceDefaults applies a 30s total-request-timeout resilience handler to every HttpClient by default,
        // which is far too short for local LLM generation (cold model load + inference can take minutes). Widen it
        // for this client only.
        builder.Services.Configure<HttpStandardResilienceOptions>(McpClientToolInvoker.HttpClientName, options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddScoped<IMessageFetcher, NetCordMessageFetcher>();
        builder.Services.AddScoped<IMcpToolInvoker, McpClientToolInvoker>();
        builder.Services.AddScoped<ISummarizer, McpSummarizer>();
        builder.Services.AddScoped<TldrOrchestrator>();

        IHost host = builder.Build();

        host.AddModules(typeof(Program).Assembly);

        await host.RunAsync();
    }
}
