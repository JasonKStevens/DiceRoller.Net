using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Extension methods for configuring the application configuration system.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configures the application configuration system with proper loading order
        /// and environment-specific settings.
        /// </summary>
        public static IConfigurationBuilder ConfigureApplicationConfiguration(
            this IConfigurationBuilder builder, 
            string[] args, 
            string? basePath = null)
        {
            // Note: Host.CreateDefaultBuilder already adds:
            // - appsettings.json
            // - appsettings.{Environment}.json
            // - Environment variables
            // - Command line args
            // 
            // So we don't need to add them again. We just need to ensure the base path is set
            // and add any additional configuration sources if needed.
            
            // Set base path if needed
            var appBasePath = basePath ?? AppDomain.CurrentDomain.BaseDirectory;
            
            // The builder already has the sources from CreateDefaultBuilder,
            // so we just return it as-is to preserve the proper loading order
            return builder;
        }

        /// <summary>
        /// Registers all configuration sections with the dependency injection container
        /// using the Options pattern with validation.
        /// </summary>
        public static IServiceCollection AddApplicationConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register all configuration sections consistently with AddOptions pattern and validation
            services.AddOptions<BotConfiguration>()
                .Bind(configuration.GetSection(BotConfiguration.SectionName))
                .ValidateDataAnnotations();
            
            services.AddOptions<LoggingConfiguration>()
                .Bind(configuration.GetSection(LoggingConfiguration.SectionName))
                .ValidateDataAnnotations();
            
            services.AddOptions<PluginConfiguration>()
                .Bind(configuration.GetSection(PluginConfiguration.SectionName))
                .ValidateDataAnnotations();
            
            services.AddOptions<SecurityConfiguration>()
                .Bind(configuration.GetSection(SecurityConfiguration.SectionName))
                .ValidateDataAnnotations();

            // Register the root configuration
            services.AddOptions<ApplicationConfiguration>()
                .Bind(configuration)
                .ValidateDataAnnotations();

            // Register configuration objects for direct injection (backward compatibility)
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<BotConfiguration>>().Value);
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<LoggingConfiguration>>().Value);
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<PluginConfiguration>>().Value);
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<SecurityConfiguration>>().Value);
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<ApplicationConfiguration>>().Value);

            return services;
        }

        /// <summary>
        /// Configures logging based on the application configuration.
        /// </summary>
        public static ILoggingBuilder ConfigureApplicationLogging(
            this ILoggingBuilder logging, 
            IConfiguration configuration)
        {
            var loggingConfig = configuration.GetSection(LoggingConfiguration.SectionName).Get<LoggingConfiguration>() 
                               ?? new LoggingConfiguration();

            logging.ClearProviders();

            // Configure console logging
            if (loggingConfig.EnableConsole)
            {
                logging.AddConsole();
            }

            // Configure file logging (if enabled and available)
            if (loggingConfig.EnableFile && !string.IsNullOrEmpty(loggingConfig.LogFilePath))
            {
                // TODO: File logging implementation requires additional NuGet package like Serilog
                // To implement: Install Serilog.Extensions.Logging and Serilog.Sinks.File packages
                // and replace this console output with actual file sink configuration
                Console.WriteLine($"File logging requested but not implemented: {loggingConfig.LogFilePath}");
            }

            // Set minimum log level
            logging.SetMinimumLevel(loggingConfig.MinimumLevel);

            // Configure category-specific log levels
            foreach (var category in loggingConfig.Categories)
            {
                logging.AddFilter(category.Key, category.Value);
            }

            return logging;
        }

        /// <summary>
        /// Creates a backward-compatible DiscordApiConfiguration from the new BotConfiguration.
        /// </summary>
        public static DiscordRollerBot.DiscordApiConfiguration CreateLegacyDiscordApiConfiguration(
            IConfiguration configuration)
        {
            var botConfig = configuration.GetSection(BotConfiguration.SectionName).Get<BotConfiguration>() 
                           ?? new BotConfiguration();

            // Try legacy configuration if new one is empty
            if (string.IsNullOrEmpty(botConfig.Token))
            {
                var legacyConfig = new DiscordRollerBot.DiscordApiConfiguration();
                configuration.Bind("BotConfig", legacyConfig);
                
                if (!string.IsNullOrEmpty(legacyConfig.Token))
                {
                    botConfig.Token = legacyConfig.Token;
                    botConfig.CommandPrefix = legacyConfig.CommandPrefix ?? botConfig.CommandPrefix;
                }
            }

            return new DiscordRollerBot.DiscordApiConfiguration
            {
                Token = botConfig.Token,
                CommandPrefix = botConfig.CommandPrefix,
                EnableSlashCommands = botConfig.EnableSlashCommands,
                EnablePrefixCommands = botConfig.EnablePrefixCommands,
                EnableButtonInteractions = botConfig.EnableButtonInteractions
            };
        }

        /// <summary>
        /// Gets the current environment name for configuration file selection.
        /// </summary>
        private static string GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                   ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                   ?? Environment.GetEnvironmentVariable("DICEROLLER_ENVIRONMENT")
                   ?? "Production";
        }

        /// <summary>
        /// Validates configuration at startup and logs any issues.
        /// </summary>
        public static void ValidateConfiguration(IServiceProvider serviceProvider)
        {
            try
            {
                // Validate each configuration section
                var botConfig = serviceProvider.GetRequiredService<IOptions<BotConfiguration>>();
                var securityConfig = serviceProvider.GetRequiredService<IOptions<SecurityConfiguration>>();
                var loggingConfig = serviceProvider.GetRequiredService<IOptions<LoggingConfiguration>>();
                var pluginConfig = serviceProvider.GetRequiredService<IOptions<PluginConfiguration>>();

                // Access .Value to trigger validation
                _ = botConfig.Value;
                _ = securityConfig.Value;
                _ = loggingConfig.Value;
                _ = pluginConfig.Value;

                Console.WriteLine("Configuration validation passed.");
            }
            catch (OptionsValidationException ex)
            {
                Console.WriteLine($"Configuration validation failed: {ex.Message}");
                foreach (var failure in ex.Failures)
                {
                    Console.WriteLine($"  - {failure}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Configuration validation error: {ex.Message}");
                throw;
            }
        }
    }
}