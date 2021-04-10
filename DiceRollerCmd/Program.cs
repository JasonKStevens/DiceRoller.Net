﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Dice;
using DiceRoller.Parser;
using DiscordRollerBot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
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
                                })
                                .ConfigureLogging(logging => {
                                    logging.AddConsole().SetMinimumLevel(LogLevel.Information);
                                })
                                .ConfigureServices(ConfigureServices)
                                .Build();

            await host.RunAsync();
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

            services.AddSingleton<IDiscordApi, DiscordApi>();

            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddSingleton<Evaluator, Evaluator>();

            services.AddHostedService<BotHost>();
        }
    }

    public class BotHost : IHostedService
    {
        private readonly ILogger<BotHost> _logger;
        private readonly IDiscordApi _discordInterface;
        private readonly Evaluator _evaluator;

        public BotHost(ILogger<BotHost> logger, IHostApplicationLifetime appLifetime, IDiscordApi discord, Evaluator evaluator)
        {
            _logger = logger;
            _discordInterface = discord;
            _evaluator = evaluator;
            _discordInterface.AddHandler("!roll", HandleDiceRolls);
        }

        private string HandleDiceRolls(string instructions)
        {
            var result = _evaluator.Evaluate(instructions);

            var builder = new StringBuilder();
            builder.Append("   __**"  + result.Value + "**__  ");
            if (result.Breakdown.Length > 50)
                builder.AppendLine();
            builder.Append("Reason:  ");
            if (result.Breakdown.Length > 50)
                builder.AppendLine();
            builder.AppendLine(result.Breakdown);

            return builder.ToString();
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
