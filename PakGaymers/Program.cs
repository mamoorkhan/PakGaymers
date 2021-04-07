using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GaymersBot.Services;
using Microsoft.Azure.WebJobs.Host;
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

            ConfigureConfiguration();

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
                await ConfigureServices(s);
            });

            builder.ConfigureLogging(logging =>
            {
                var appInsightsKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    // This uses the options callback to explicitly set the instrumentation key.
                    logging.AddApplicationInsights(appInsightsKey)
                        .SetMinimumLevel(LogLevel.Information);

                    logging.AddApplicationInsightsWebJobs(o =>
                    {
                        o.InstrumentationKey = appInsightsKey;
                    });
                }
            });

            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;
            var host = builder.Build();
            //var services = host.Services;
            //var commandHandlingService = services.GetService<CommandHandlingService>();
            //if (commandHandlingService != null)
            //{
            //    await commandHandlingService.InitializeAsync();
            //}
            using (host)
            {
                await host.RunAsync(ct);
                tokenSource.Dispose();
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static async Task ConfigureServices(IServiceCollection services)
        {
            var token = Configuration["Token"];
            _client.Log += LogAsync;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            var sp = services.AddMemoryCache()
                .AddSingleton(Configuration)
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<LoggingService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<IJobActivator, JobActivator>()
                .BuildServiceProvider();

            await sp.GetRequiredService<CommandHandlingService>().InitializeAsync();
        }

        private static void ConfigureConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() //this doesn't do anything useful notice im setting some env variables explicitly.
                .Build();  //build it so you can use those config variables down below.
        }
    }
}