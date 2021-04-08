using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PakGaymers.Data.Table;
using PakGaymers.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PakGaymers
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
                var services = await ConfigureServices(s);
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
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

        private static async Task<IServiceProvider> ConfigureServices(IServiceCollection services)
        {
            var token = Configuration["Token"];
            _client.Log += LogAsync;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();




            return services
                .AddMemoryCache()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton(ServiceDescriptor.Describe(typeof(ILogger<>), typeof(Logger<>), ServiceLifetime.Singleton))
                .AddSingleton(Configuration)
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<ITableStorage, AzureTableStorage>()
                .AddSingleton<LoggingService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<IJobActivator, JobActivator>()
                .BuildServiceProvider();
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