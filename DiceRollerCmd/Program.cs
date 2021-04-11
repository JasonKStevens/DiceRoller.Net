using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Dice;
using DiceRoller.DragonQuest;
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
            services.AddSingleton<GrievousInjuries, GrievousInjuries>();
            services.AddSingleton<Backfires, Backfires>();
            services.AddSingleton<FearResult, FearResult>();
            services.AddSingleton<DQLookupTables, DQLookupTables>();

            services.AddHostedService<BotHost>();
        }
    }

    public class BotHost : IHostedService
    {
        private readonly ILogger<BotHost> _logger;
        private readonly IDiscordApi _discordInterface;
        private readonly Evaluator _evaluator;
        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;


        public BotHost(ILogger<BotHost> logger, IHostApplicationLifetime appLifetime, IDiscordApi discord, Evaluator evaluator, DQLookupTables tables)
        {
            _logger = logger;
            _discordInterface = discord;
            _evaluator = evaluator;
            _injuries = tables.Injuries;
            _backfires = tables.Backfires;
            _fears = tables.Fear;

            _discordInterface.AddHandler("!roll", HandleDiceRolls);
            _discordInterface.AddHandler("!injury", (ins) => 
            LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!specgrev", (ins) => LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!backfire", (ins) => LookupResult(_backfires, ins));
            _discordInterface.AddHandler("!fear", (ins) => LookupResult(_fears, ins));
        }

        private string HandleDiceRolls(string instructions)
        {
            var result = _evaluator.Evaluate(instructions);

            var builder = new StringBuilder();
            builder.Append("   __**"  + result.Value + "**__  ");
            if (result.Breakdown.Length > 50)
            {
                builder.AppendLine();
                builder.Append("Reason:  ");
                builder.AppendLine();
                builder.AppendLine("||" + result.Breakdown + "||");
            } else {
                builder.Append("Reason:  ");
                builder.AppendLine(result.Breakdown);
            }

            return builder.ToString();
        }

        private string LookupResult(LookupTable table, string instructions)
        {
            int roll = 0;

            if (string.IsNullOrWhiteSpace(instructions))
            {
                roll = ( int ) _evaluator.Evaluate("d100").Value;
            } else
            {
                if (!Int32.TryParse(instructions.Trim(), out roll))
                {
                    return "Huh?";
                }
            }

            return table.LookupResult(roll);
        }

        private string HandleInjuryRoll(string instructions)
        {
            return LookupResult(_injuries, instructions);
        }

        private string HandleBackfireRoll(string instructions)
        {
            return LookupResult(_backfires, instructions);
        }

        private string HandleFearRoll(string instructions)
        {
            return LookupResult(_backfires, instructions);
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
