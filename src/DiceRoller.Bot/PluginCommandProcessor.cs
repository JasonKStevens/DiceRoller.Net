using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiceRoller.Bot.Services;
using DiceRoller.Core.Parser;
using DiceRoller.Discord;
using DiceRoller.Plugins.Infrastructure.Core;
using Microsoft.Extensions.Logging;
using DiscordRollerBot;

namespace DiceRoller.Bot
{
    /// <summary>
    /// Command processor for plugin management commands
    /// </summary>
    public class PluginCommandProcessor : ICommandProcessor
    {
        private readonly ILogger<PluginCommandProcessor> _logger;
        private readonly IPluginIntegrationService _pluginService;

        public PluginCommandProcessor(
            ILogger<PluginCommandProcessor> logger,
            IPluginIntegrationService pluginService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pluginService = pluginService ?? throw new ArgumentNullException(nameof(pluginService));
        }

        /// <inheritdoc/>
        public string Prefix => "/plugin";

        /// <inheritdoc/>
        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            try
            {
                var parts = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    return (true, CreateHelpResult());
                }

                var subCommand = parts[1].ToLowerInvariant();

                return subCommand switch
                {
                    "list" => HandleListCommand(),
                    "reload" => HandleReloadCommand(parts),
                    "info" => HandleInfoCommand(parts),
                    "help" => (true, CreateHelpResult()),
                    _ => (true, TypedResult.NewSimpleResult(NodeType.Text, $"Unknown plugin command: {subCommand}. Use `/plugin help` for available commands."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing plugin command: {Command}", commandText);
                return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Error processing plugin command: {ex.Message}"));
            }
        }

        private (bool, TypedResult) HandleListCommand()
        {
            try
            {
                if (!_pluginService.IsInitialized)
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "Plugin service is not initialized."));
                }

                var plugins = _pluginService.GetPluginInfo().ToList();
                if (!plugins.Any())
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "No plugins found."));
                }

                var sb = new StringBuilder();
                sb.AppendLine("**Available Plugins:**");
                sb.AppendLine();

                var loadedPlugins = plugins.Where(p => p.Status == PluginStatus.Running).ToList();
                var unloadedPlugins = plugins.Where(p => p.Status != PluginStatus.Running).ToList();

                if (loadedPlugins.Any())
                {
                    sb.AppendLine("**Loaded Plugins:**");
                    foreach (var plugin in loadedPlugins.OrderBy(p => p.Name))
                    {
                        sb.AppendLine($"🟢 **{plugin.Name}** v{plugin.Version} - {plugin.Description}");
                        if (plugin.ProvidedFunctions.Any())
                        {
                            sb.AppendLine($"   Functions: {string.Join(", ", plugin.ProvidedFunctions)}");
                        }
                        if (plugin.ProvidedCommands.Any())
                        {
                            sb.AppendLine($"   Commands: {string.Join(", ", plugin.ProvidedCommands)}");
                        }
                    }
                    sb.AppendLine();
                }

                if (unloadedPlugins.Any())
                {
                    sb.AppendLine("**Unloaded Plugins:**");
                    foreach (var plugin in unloadedPlugins.OrderBy(p => p.Name))
                    {
                        var statusIcon = plugin.Status == PluginStatus.Error ? "🔴" : "⚫";
                        sb.AppendLine($"{statusIcon} **{plugin.Name}** v{plugin.Version} - {plugin.Description} ({plugin.Status})");
                        if (!string.IsNullOrEmpty(plugin.ErrorMessage))
                        {
                            sb.AppendLine($"   Error: {plugin.ErrorMessage}");
                        }
                    }
                }

                return (true, TypedResult.NewSimpleResult(NodeType.Text, sb.ToString().TrimEnd()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing plugins");
                return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Error listing plugins: {ex.Message}"));
            }
        }

        private (bool, TypedResult) HandleReloadCommand(string[] parts)
        {
            try
            {
                if (!_pluginService.IsInitialized)
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "Plugin service is not initialized."));
                }

                if (parts.Length == 2)
                {
                    // Reload all plugins
                    _pluginService.ReloadPluginsAsync().GetAwaiter().GetResult();
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "All plugins reloaded successfully."));
                }
                else if (parts.Length == 3)
                {
                    // Reload specific plugin
                    var pluginId = parts[2];
                    var success = _pluginService.UnloadPluginAsync(pluginId).GetAwaiter().GetResult();
                    if (success)
                    {
                        success = _pluginService.LoadPluginAsync(pluginId).GetAwaiter().GetResult();
                        if (success)
                        {
                            return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Plugin '{pluginId}' reloaded successfully."));
                        }
                        else
                        {
                            return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Failed to load plugin '{pluginId}' after unloading."));
                        }
                    }
                    else
                    {
                        return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Failed to unload plugin '{pluginId}'."));
                    }
                }
                else
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "Usage: `/plugin reload` or `/plugin reload <plugin-id>`"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading plugins");
                return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Error reloading plugins: {ex.Message}"));
            }
        }

        private (bool, TypedResult) HandleInfoCommand(string[] parts)
        {
            try
            {
                if (parts.Length < 3)
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, "Usage: `/plugin info <plugin-id>`"));
                }

                var pluginId = parts[2];
                var plugin = _pluginService.GetPluginInfo().FirstOrDefault(p => 
                    p.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals(pluginId, StringComparison.OrdinalIgnoreCase));

                if (plugin == null)
                {
                    return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Plugin '{pluginId}' not found."));
                }

                var sb = new StringBuilder();
                sb.AppendLine($"**Plugin Information: {plugin.Name}**");
                sb.AppendLine($"**ID:** {plugin.Id}");
                sb.AppendLine($"**Version:** {plugin.Version}");
                sb.AppendLine($"**Author:** {plugin.Author}");
                sb.AppendLine($"**Status:** {plugin.Status}");
                sb.AppendLine($"**Description:** {plugin.Description}");

                if (plugin.LoadTime.HasValue)
                {
                    sb.AppendLine($"**Loaded:** {plugin.LoadTime:yyyy-MM-dd HH:mm:ss} UTC");
                }

                if (plugin.ProvidedFunctions.Any())
                {
                    sb.AppendLine($"**Functions:** {string.Join(", ", plugin.ProvidedFunctions)}");
                }

                if (plugin.ProvidedCommands.Any())
                {
                    sb.AppendLine($"**Commands:** {string.Join(", ", plugin.ProvidedCommands)}");
                }

                sb.AppendLine($"**Game System Plugin:** {(plugin.IsGameSystemPlugin ? "Yes" : "No")}");
                sb.AppendLine($"**Dice Notation Plugin:** {(plugin.IsDiceNotationPlugin ? "Yes" : "No")}");

                if (!string.IsNullOrEmpty(plugin.ErrorMessage))
                {
                    sb.AppendLine($"**Error:** {plugin.ErrorMessage}");
                }

                // Get health status if plugin is loaded
                if (plugin.Status == PluginStatus.Running)
                {
                    var health = _pluginService.GetPluginHealth(plugin.Id);
                    if (health != null)
                    {
                        sb.AppendLine($"**Health:** {(health.IsHealthy ? "Healthy" : "Unhealthy")} - {health.Status}");
                        if (health.ResponseTime.HasValue)
                        {
                            sb.AppendLine($"**Response Time:** {health.ResponseTime.Value.TotalMilliseconds:F1}ms");
                        }
                    }
                }

                return (true, TypedResult.NewSimpleResult(NodeType.Text, sb.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin info");
                return (true, TypedResult.NewSimpleResult(NodeType.Text, $"Error getting plugin info: {ex.Message}"));
            }
        }

        private TypedResult CreateHelpResult()
        {
            var sb = new StringBuilder();
            sb.AppendLine("**Plugin Management Commands:**");
            sb.AppendLine();
            sb.AppendLine("`/plugin list` - List all available plugins and their status");
            sb.AppendLine("`/plugin info <plugin-id>` - Show detailed information about a specific plugin");
            sb.AppendLine("`/plugin reload` - Reload all plugins");
            sb.AppendLine("`/plugin reload <plugin-id>` - Reload a specific plugin");
            sb.AppendLine("`/plugin help` - Show this help message");
            sb.AppendLine();
            sb.AppendLine("**Examples:**");
            sb.AppendLine("`/plugin list`");
            sb.AppendLine("`/plugin info CallOfCthulhu`");
            sb.AppendLine("`/plugin reload CallOfCthulhu`");

            return TypedResult.NewSimpleResult(NodeType.Text, sb.ToString());
        }
    }
}