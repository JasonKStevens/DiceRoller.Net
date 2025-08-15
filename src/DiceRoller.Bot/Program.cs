using System;
using System.IO;
using System.Threading.Tasks;
using DiceRoller;
using DiceRoller.Dice;
using DiceRoller.DragonQuest;
using DiceRoller.Heroes;
using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Functions;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Routing;
using DiceRoller.Bot.Services;
using DiceRoller.GameSystems.DragonQuest;
using DiscordRollerBot;
using DiscordRollerBot.Services;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PartyDSL;
using PartyDSL.Parser;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.Discovery;
using DiceRoller.Plugins.Infrastructure.Manager;
using DiceRoller.Plugins.Infrastructure.Configuration;

namespace DiceRollerCmd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TEST: Switch to minimal program for debugging
            await MinimalProgram.MainMinimal(args);
            return;
            
            Console.WriteLine("=== DiceRoller.Bot Starting (Simple Host) ===");
            Console.WriteLine($"Starting at: {DateTime.Now}");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    Console.WriteLine($"Looking for appsettings.json in: {basePath}");
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    Console.WriteLine("=== Configuring services ===");
                    ConfigureServices(services, context.Configuration);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            Console.WriteLine("=== Starting host ===");
            await host.RunAsync();
            Console.WriteLine("=== Application stopped ===");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("=== SIMPLIFIED SERVICE CONFIGURATION ===");
            
            var botConfig = new DiscordApiConfiguration();
            configuration.Bind("BotConfig", botConfig);
            Console.WriteLine($"Discord Token configured: {!string.IsNullOrEmpty(botConfig.Token)}");
            Console.WriteLine($"Token starts with: {botConfig.Token?.Substring(0, 10)}...");
            services.AddSingleton(botConfig);

            var config = new DiscordConfiguration()
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                MinimumLogLevel = LogLevel.Debug
            };
            services.AddSingleton(config);

            Console.WriteLine("Creating Discord client...");
            services.AddSingleton<DiscordClient>(provider =>
            {
                var discordConfig = provider.GetRequiredService<DiscordConfiguration>();
                var client = new DiscordClient(discordConfig);
                Console.WriteLine("Discord client created successfully");
                return client;
            });

            // Add minimal required services for Discord bot
            services.AddSingleton<IUserSettings, UserSettings>();
            services.AddSingleton<IUserAliases, UserAliases>();
            
            // Add required formatting services for InteractionHandler
            services.AddSingleton<IResultNodeConverter, ResultNodeConverter>();
            services.AddSingleton<ComplexityThresholdOptions>();
            services.AddSingleton<IComplexityAnalyzer, ComplexityAnalyzer>();
            services.AddSingleton<IAdaptiveBreakdownFormatter, EnhancedTextFormatter>();
            services.AddSingleton<AdaptiveFormatterManager>();
            
            services.AddSingleton<InteractionHandler>();
            
            // Add required dice rolling services for commands to work
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddSingleton<DiceRollEvaluator, DiceRollEvaluator>();
            
            // Add Function Registry (required for dice rolling)
            services.AddSingleton<IFunctionRegistry>(provider =>
            {
                var registry = new FunctionRegistry();
                foreach (var function in BuiltInFunctions.GetBuiltInFunctions())
                {
                    registry.RegisterFunction(function.Name, function);
                }
                return registry;
            });
            
            // Add Game System Router (required for game commands)
            services.AddSingleton<IDiceExpressionParser, DiceExpressionParser>();
            services.AddSingleton<IDiceExpressionEvaluator, DiceExpressionEvaluator>();
            
            // Add DragonQuest services
            services.AddSingleton<GrievousInjuries, GrievousInjuries>();
            services.AddSingleton<Backfires, Backfires>();
            services.AddSingleton<FearResult, FearResult>();
            services.AddSingleton<DQLookupTables, DQLookupTables>();
            
            // Add Heroes services
            services.AddSingleton<SpeedTable, SpeedTable>();
            services.AddSingleton<LocationTable, LocationTable>();
            services.AddSingleton<HerosLookupTables, HerosLookupTables>();
            
            services.AddSingleton<IGameSystemRouter>(provider =>
            {
                var router = new GameSystemRouter();
                var dqTables = provider.GetRequiredService<DQLookupTables>();
                var evaluator = provider.GetRequiredService<IDiceExpressionEvaluator>();
                var dragonQuestSystem = new DragonQuestGameSystem(evaluator, dqTables);
                router.RegisterSystem(dragonQuestSystem);
                return router;
            });
            
            // Add Party services
            services.AddSingleton<IPartyManager, PartyManager>();
            services.AddSingleton<PartyCommandEvaluator>();
            services.AddSingleton<SettingsCommandEvaluator>();
            
            // Add plugin infrastructure services (required by PluginCommandProcessor)
            // Create a non-blocking PluginManager wrapper
            services.AddSingleton<IPluginEventAggregator, PluginEventAggregator>();
            services.AddSingleton<IPluginResourceManager, PluginResourceManager>();
            services.AddSingleton<IPluginPerformanceMonitor, PluginPerformanceMonitor>();
            services.AddSingleton<PluginDiscovery>();
            services.AddSingleton<PluginLoader>();
            services.AddSingleton<PluginSandbox>();
            services.AddSingleton<PluginConfiguration>();
            services.AddSingleton<PluginManager>();
            
            Console.WriteLine("Adding NonBlockingPluginManagerService...");
            services.AddHostedService<NonBlockingPluginManagerService>();
            Console.WriteLine("NonBlockingPluginManagerService added successfully");
            
            services.AddSingleton<IPluginIntegrationService, PluginIntegrationService>();
            
            // Register command processors
            Console.WriteLine("Registering command processors...");
            services.RegisterAllTypes<ICommandProcessor>(new[] { typeof(Program).Assembly }, ServiceLifetime.Singleton);
            Console.WriteLine("Command processors registered");
            
            Console.WriteLine("Adding IDiscordApi...");
            services.AddSingleton<IDiscordApi, DiscordApi>();
            Console.WriteLine("IDiscordApi added");
            
            // Add hosted services in correct order
            Console.WriteLine("Adding TestHostedService...");
            services.AddHostedService<TestHostedService>();  // Test service first
            
            Console.WriteLine("Adding BotHost...");
            services.AddHostedService<BotHost>();  // Start Discord connection FIRST
            
            Console.WriteLine("Adding SlashCommandRegistrationService...");
            services.AddHostedService<SlashCommandRegistrationService>();  // Then register commands

            Console.WriteLine("=== All services registered successfully ===");
        }
    }
}