using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Dice;
using DiceRoller.Parser;
using DiscordRollerBot;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiceRollerCmd
{
    class Program
    {
        static IServiceCollection _services;
        
        static async Task Main(string[] args)
        {
            using IHost host = Host
                                .CreateDefaultBuilder()
                                .ConfigureHostConfiguration(configHost =>
                                {
                                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                                    configHost.AddJsonFile("appsettings.json", optional: false);
                                    configHost.AddCommandLine(args);
                                })
                                .ConfigureServices(ConfigureServices)
                                .ConfigureLogging(logging => {
                                    logging.AddConsole().SetMinimumLevel(LogLevel.Information);
                                })
                                .Build();



            await host.RunAsync();

            //var input = Console.ReadLine();


            // Console.WriteLine("eg. (3d10 + 5) / 2 + 1d2!");
            // Console.WriteLine();

            // var evaluator = new Evaluator(new RandomNumberGenerator());

            // while (true)
            // {
            //     Console.Write("> ");
            //     var input = Console.ReadLine();
            //     if (input == "exit" || input == "quit")
            //         break;

            //     try
            //     {
            //         var evaluation = evaluator.Evaluate(input);
            //         Console.WriteLine(evaluation.Value + " Reason: " + evaluation.Breakdown);
            //         Console.WriteLine();
            //     }
            //     catch (Exception ex)
            //     {
            //         Console.WriteLine(ex.Message);
            //         Console.WriteLine();
            //     }
            // }
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            _services = services;

            var botConfig = new DiscordInterfaceConfiguration();
            context.Configuration.Bind("BotConfig", botConfig);
            services.AddSingleton(botConfig);

            var config = new DiscordConfiguration()
            {
                Token =  botConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            };            
            services.AddSingleton(config);

            services.AddSingleton<DiscordClient, DiscordClient>();

            services.AddSingleton<IDiscordInterface, DiscordInterface>();

            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddSingleton<Evaluator, Evaluator>();

            services.AddHostedService<ConsoleHost>();
        }
    }

    public class ConsoleHost : IHostedService
    {
        private readonly ILogger<ConsoleHost> _logger;
        private readonly IDiscordInterface _discordInterface;

        public ConsoleHost(ILogger<ConsoleHost> logger, IHostApplicationLifetime appLifetime, IDiscordInterface discord)
        {
            _logger = logger;
            _discordInterface = discord;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Stop();
        }
    }
}
