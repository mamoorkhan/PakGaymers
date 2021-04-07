using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GaymersBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GaymersBot
{
    internal class Program
    {
        private static IConfiguration Configuration { get; set; }

        private static CommandService _commands;
        private static DiscordSocketClient _client;

        private static async Task Main(string[] args)
        {
            _commands = new CommandService();
            _client = new DiscordSocketClient();

            Console.WriteLine("This is the whey!");

            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddTimers();
                b.AddServiceBus(sbOptions =>
                {
                    sbOptions.ConnectionString = Configuration["AzureWebJobsServiceBus"];
                });
            });

            builder.ConfigureServices(async (context, s) =>
            {
                ConfigureServices(s);

                var token = Configuration["Token"];
                _client.Log += LogAsync;
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

                s.AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<LoggingService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
            });

            builder.ConfigureLogging(logging =>
            {
                var appInsightsKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    // This uses the options callback to explicitly set the instrumentation key.
                    logging.AddApplicationInsights(appInsightsKey)
                        .SetMinimumLevel(LogLevel.Information);
                    logging.AddApplicationInsightsWebJobs(o => { o.InstrumentationKey = appInsightsKey; });
                }
            });

            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;
            var host = builder.Build();
            using (host)
            {
                //await host.Services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                await host.RunAsync(ct);
                tokenSource.Dispose();
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() //this doesn't do anything useful notice im setting some env variables explicitly.
                .Build();  //build it so you can use those config variables down below.

            services.AddMemoryCache(); //I'm using MemCache in some other classes
            services.AddSingleton(Configuration);
        }
    }
}