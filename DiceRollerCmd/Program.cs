using System.IO;
using System.Threading.Tasks;
using DiceRoller.Dice;
using DiceRoller.DragonQuest;
using DiceRoller.Heroes;
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
using PartyDSL;
using PartyDSL.Parser;

namespace DiceRollerCmd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host
                                .CreateDefaultBuilder()
                                //.ConfigureWebHostDefaults()
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
            var botConfig = new DiscordApiConfiguration();
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
            services.AddSingleton<DiceRollEvaluator, DiceRollEvaluator>();
            services.AddSingleton<GrievousInjuries, GrievousInjuries>();
            services.AddSingleton<Backfires, Backfires>();
            services.AddSingleton<FearResult, FearResult>();
            services.AddSingleton<DQLookupTables, DQLookupTables>();
            services.AddSingleton<SpeedTable, SpeedTable>();
            services.AddSingleton<LocationTable, LocationTable>();
            services.AddSingleton<HerosLookupTables, HerosLookupTables>();

            services.AddSingleton<IPartyManager, PartyManager>();
            services.AddSingleton<PartyCommandEvaluator>();

            services.RegisterAllTypes<ICommandProcessor>(new[] {typeof(Program).Assembly}, ServiceLifetime.Singleton);

            services.AddHostedService<BotHost>();
        }
    }
}
