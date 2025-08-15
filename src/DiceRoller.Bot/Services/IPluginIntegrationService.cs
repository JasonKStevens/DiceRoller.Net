using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.Manager;

namespace DiceRoller.Bot.Services
{
    /// <summary>
    /// Interface for integrating plugins with the DiceRoller core functionality
    /// </summary>
    public interface IPluginIntegrationService
    {
        /// <summary>
        /// Gets all loaded plugins
        /// </summary>
        IReadOnlyDictionary<string, LoadedPlugin> LoadedPlugins { get; }

        /// <summary>
        /// Gets all available plugins (loaded and unloaded)
        /// </summary>
        IReadOnlyDictionary<string, PluginMetadata> AvailablePlugins { get; }

        /// <summary>
        /// Event raised when plugins are loaded or reloaded
        /// </summary>
        event EventHandler<PluginIntegrationEventArgs>? PluginsChanged;

        /// <summary>
        /// Initialize the plugin integration service
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the initialization operation</returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Load plugins from the plugins directory and register them with core services
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the load operation</returns>
        Task LoadPluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reload all plugins
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the reload operation</returns>
        Task ReloadPluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Load a specific plugin by ID
        /// </summary>
        /// <param name="pluginId">Plugin ID to load</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the plugin was successfully loaded</returns>
        Task<bool> LoadPluginAsync(string pluginId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unload a specific plugin by ID
        /// </summary>
        /// <param name="pluginId">Plugin ID to unload</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the plugin was successfully unloaded</returns>
        Task<bool> UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get detailed information about loaded plugins
        /// </summary>
        /// <returns>Plugin information including status and capabilities</returns>
        IEnumerable<PluginInfo> GetPluginInfo();

        /// <summary>
        /// Get plugin health status
        /// </summary>
        /// <param name="pluginId">Plugin ID to check</param>
        /// <returns>Health status information, or null if plugin not found</returns>
        PluginHealthStatus? GetPluginHealth(string pluginId);

        /// <summary>
        /// Check if the service is properly initialized
        /// </summary>
        bool IsInitialized { get; }
    }

    /// <summary>
    /// Plugin information for display purposes
    /// </summary>
    public record PluginInfo(
        string Id,
        string Name,
        Version Version,
        string Description,
        string Author,
        PluginStatus Status,
        bool IsGameSystemPlugin,
        bool IsDiceNotationPlugin,
        IReadOnlyList<string> ProvidedFunctions,
        IReadOnlyList<string> ProvidedCommands,
        DateTime? LoadTime = null,
        string? ErrorMessage = null);

    /// <summary>
    /// Event arguments for plugin integration events
    /// </summary>
    public class PluginIntegrationEventArgs : EventArgs
    {
        public PluginIntegrationAction Action { get; }
        public string PluginId { get; }
        public string? ErrorMessage { get; }

        public PluginIntegrationEventArgs(PluginIntegrationAction action, string pluginId, string? errorMessage = null)
        {
            Action = action;
            PluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Plugin integration actions
    /// </summary>
    public enum PluginIntegrationAction
    {
        Loaded,
        Unloaded,
        Reloaded,
        Error
    }
}