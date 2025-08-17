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
using DiceRoller.Bot;
using DiceRoller.Bot.Services;
using DiceRoller.Bot.Configuration;
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
            Console.WriteLine("Starting DiceRoller.Bot...");
            
            // Check if Bot__Token is in process environment, if not, try to load from system
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Bot__Token")))
            {
                var systemToken = Environment.GetEnvironmentVariable("Bot__Token", EnvironmentVariableTarget.Machine);
                if (!string.IsNullOrEmpty(systemToken))
                {
                    Environment.SetEnvironmentVariable("Bot__Token", systemToken);
                }
            }

            Console.WriteLine("Building host...");
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Bind configuration sections
                    services.Configure<BotConfiguration>(context.Configuration.GetSection("Bot"));
                    services.Configure<LoggingConfiguration>(context.Configuration.GetSection("Logging"));
                    services.Configure<DiceRoller.Bot.Configuration.PluginConfiguration>(context.Configuration.GetSection("Plugins"));
                    services.Configure<SecurityConfiguration>(context.Configuration.GetSection("Security"));
                    
                    ConfigureServices(services, context.Configuration);
                })
                .Build();

            Console.WriteLine("Starting host...");
            await host.RunAsync();
            Console.WriteLine("Host stopped.");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("[DI] Starting ConfigureServices");
            // Get the token directly from configuration
            string? token = configuration["Bot:Token"];
            
            // Check if token is missing and provide helpful error message
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("\n⚠️  WARNING: Discord bot token is not configured!");
                Console.WriteLine("\nTo set the bot token, use one of these methods:");
                Console.WriteLine("\n1. Set environment variable (PowerShell):");
                Console.WriteLine("   $env:Bot__Token = \"your-bot-token-here\"");
                Console.WriteLine("   dotnet run");
                Console.WriteLine("\n2. Update appsettings.json:");
                Console.WriteLine("   Edit the 'Bot.Token' field");
                Console.WriteLine("\n3. Use command line:");
                Console.WriteLine("   dotnet run --Bot:Token=\"your-bot-token-here\"");
                
                throw new InvalidOperationException("Bot token is required but not configured. See above for configuration options.");
            }
            
            // Create simple config object for Discord
            var botConfig = new DiscordRollerBot.DiscordApiConfiguration
            {
                Token = token,
                CommandPrefix = configuration["Bot:CommandPrefix"] ?? "!roll",
                EnableSlashCommands = configuration.GetValue<bool>("Bot:EnableSlashCommands", true),
                EnablePrefixCommands = configuration.GetValue<bool>("Bot:EnablePrefixCommands", false),
                EnableButtonInteractions = configuration.GetValue<bool>("Bot:EnableButtonInteractions", true)
            };
            services.AddSingleton(botConfig);
            Console.WriteLine("[DI] Added botConfig");

            // Get logging configuration for Discord client
            var loggingConfig = configuration.GetSection(LoggingConfiguration.SectionName).Get<LoggingConfiguration>() 
                               ?? new LoggingConfiguration();
            
            var config = new DiscordConfiguration()
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                MinimumLogLevel = loggingConfig.Categories.GetValueOrDefault("DSharpPlus", loggingConfig.MinimumLevel)
            };
            services.AddSingleton(config);
            Console.WriteLine("[DI] Added DiscordConfiguration");

            services.AddSingleton<DiscordClient>(provider =>
            {
                var discordConfig = provider.GetRequiredService<DiscordConfiguration>();
                var client = new DiscordClient(discordConfig);
                return client;
            });
            Console.WriteLine("[DI] Added DiscordClient factory");

            // Add minimal required services for Discord bot
            services.AddSingleton<IUserSettings, UserSettings>();
            Console.WriteLine("[DI] Added UserSettings");
            services.AddSingleton<IUserAliases, UserAliases>();
            Console.WriteLine("[DI] Added UserAliases");
            
            // Add required formatting services for InteractionHandler
            services.AddSingleton<IResultNodeConverter, ResultNodeConverter>();
            services.AddSingleton<ComplexityThresholdOptions>();
            services.AddSingleton<IComplexityAnalyzer, ComplexityAnalyzer>();
            services.AddSingleton<IAdaptiveBreakdownFormatter, EnhancedTextFormatter>();
            services.AddSingleton<AdaptiveFormatterManager>();
            Console.WriteLine("[DI] Added formatting services");
            
            services.AddSingleton<InteractionHandler>();
            Console.WriteLine("[DI] Added InteractionHandler");
            
            // Add required dice rolling services for commands to work
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            
            // Register DiceRollEvaluator with proper dependencies
            services.AddSingleton<DiceRollEvaluator>(provider =>
            {
                var rng = provider.GetRequiredService<IRandomNumberGenerator>();
                var functionRegistry = provider.GetRequiredService<IFunctionRegistry>();
                // Pass null for IGameSystemRouter initially to avoid circular dependency
                // The actual game routing happens through command processors
                return new DiceRollEvaluator(rng, functionRegistry, null);
            });
            Console.WriteLine("[DI] Added dice rolling services");
            
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
            Console.WriteLine("[DI] Added FunctionRegistry");
            
            // Add Game System Router (required for game commands)
            services.AddSingleton<IDiceExpressionParser, DiceExpressionParser>();
            
            // Create a lazy wrapper to break the circular dependency
            // The evaluator will get the router from the service provider when needed
            services.AddSingleton<IDiceExpressionEvaluator>(provider =>
            {
                var rng = provider.GetRequiredService<IRandomNumberGenerator>();
                var parser = provider.GetRequiredService<IDiceExpressionParser>();
                var functionRegistry = provider.GetRequiredService<IFunctionRegistry>();
                
                // Create evaluator without router initially
                var evaluator = new DiceExpressionEvaluator(rng, parser, functionRegistry, null);
                
                // Note: The router will be available when needed through the visitor pattern
                // Game commands are processed through command processors which have their own routing
                return evaluator;
            });
            Console.WriteLine("[DI] Added expression parser/evaluator");
            
            // Add DragonQuest services
            services.AddSingleton<GrievousInjuries, GrievousInjuries>();
            services.AddSingleton<Backfires, Backfires>();
            services.AddSingleton<FearResult, FearResult>();
            services.AddSingleton<DQLookupTables, DQLookupTables>();
            Console.WriteLine("[DI] Added DragonQuest services");
            
            // Add Heroes services
            services.AddSingleton<SpeedTable, SpeedTable>();
            services.AddSingleton<LocationTable, LocationTable>();
            services.AddSingleton<HerosLookupTables, HerosLookupTables>();
            Console.WriteLine("[DI] Added Heroes services");
            
            services.AddSingleton<IGameSystemRouter>(provider =>
            {
                var router = new GameSystemRouter();
                var dqTables = provider.GetRequiredService<DQLookupTables>();
                var evaluator = provider.GetRequiredService<IDiceExpressionEvaluator>();
                var dragonQuestSystem = new DragonQuestGameSystem(evaluator, dqTables);
                router.RegisterSystem(dragonQuestSystem);
                return router;
            });
            Console.WriteLine("[DI] Added GameSystemRouter");
            
            // Add Party services
            services.AddSingleton<IPartyManager, PartyManager>();
            services.AddSingleton<PartyCommandEvaluator>();
            services.AddSingleton<SettingsCommandEvaluator>();
            Console.WriteLine("[DI] Added Party services");
            
            // Add plugin infrastructure services (required by PluginCommandProcessor)
            // Create a non-blocking PluginManager wrapper
            services.AddSingleton<IPluginEventAggregator, PluginEventAggregator>();
            services.AddSingleton<IPluginResourceManager, PluginResourceManager>();
            services.AddSingleton<IPluginPerformanceMonitor, PluginPerformanceMonitor>();
            services.AddSingleton<PluginDiscovery>();
            services.AddSingleton<PluginLoader>();
            services.AddSingleton<PluginSandbox>();
            services.AddSingleton<DiceRoller.Plugins.Infrastructure.Configuration.PluginConfiguration>();
            services.AddSingleton<PluginManager>();
            Console.WriteLine("[DI] Added plugin infrastructure");
            
            Console.WriteLine("[DI] Adding PluginStartupService...");
            services.AddHostedService<PluginStartupService>();
            Console.WriteLine("[DI] Added PluginStartupService");
            
            services.AddSingleton<IPluginIntegrationService, PluginIntegrationService>();
            Console.WriteLine("[DI] Added PluginIntegrationService");
            
            // Register command processors BEFORE they're needed by DiscordApi
            services.AddSingleton<ICommandProcessor, RollCommandProcessor>();
            services.AddSingleton<ICommandProcessor, DQCommandProcessor>();
            services.AddSingleton<ICommandProcessor, PartyCommandPocessor>();
            services.AddSingleton<ICommandProcessor, SettingsCommandPocessor>();
            services.AddSingleton<ICommandProcessor, PluginCommandProcessor>();
            Console.WriteLine("[DI] Added all command processors");
            
            services.AddSingleton<IDiscordApi>(provider =>
            {
                Console.WriteLine("Creating DiscordApi...");
                var client = provider.GetRequiredService<DiscordClient>();
                Console.WriteLine("Got DiscordClient");
                var apiConfig = provider.GetRequiredService<DiscordApiConfiguration>();
                Console.WriteLine("Got DiscordApiConfiguration");
                var commandProcessors = provider.GetRequiredService<IEnumerable<ICommandProcessor>>();
                Console.WriteLine($"Got {commandProcessors.Count()} command processors");
                var userSettings = provider.GetRequiredService<IUserSettings>();
                var interactionHandler = provider.GetRequiredService<InteractionHandler>();
                var logger = provider.GetService<ILogger<DiscordApi>>();
                
                return new DiscordApi(client, apiConfig, commandProcessors, userSettings, interactionHandler, logger);
            });
            Console.WriteLine("[DI] Added DiscordApi");
            
            // Add hosted services in correct order
            Console.WriteLine("[DI] Adding BotHost...");
            services.AddHostedService<BotHost>();  // Start Discord connection FIRST
            Console.WriteLine("[DI] Added BotHost");
            
            Console.WriteLine("[DI] Adding SlashCommandRegistrationService...");
            services.AddHostedService<SlashCommandRegistrationService>();  // Then register commands
            Console.WriteLine("[DI] Added SlashCommandRegistrationService");
            Console.WriteLine("[DI] ConfigureServices completed");
        }
    }
}