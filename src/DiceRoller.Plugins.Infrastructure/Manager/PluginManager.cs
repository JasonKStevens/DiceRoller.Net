using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.Discovery;
using DiceRoller.Plugins.Infrastructure.Configuration;

namespace DiceRoller.Plugins.Infrastructure.Manager
{
    /// <summary>
    /// Central manager for plugin lifecycle, loading, and coordination
    /// </summary>
    public class PluginManager : IHostedService, IDisposable
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly PluginDiscovery _pluginDiscovery;
        private readonly PluginLoader _pluginLoader;
        private readonly PluginSandbox _pluginSandbox;
        private readonly PluginConfiguration _pluginConfiguration;
        private readonly IPluginEventAggregator _eventAggregator;

        private readonly ConcurrentDictionary<string, LoadedPlugin> _loadedPlugins = new();
        private readonly ConcurrentDictionary<string, PluginMetadata> _availablePlugins = new();
        private readonly Dictionary<string, List<string>> _dependencyGraph = new();
        private readonly Timer _healthCheckTimer;
        private readonly Timer _pluginScanTimer;

        private bool _isStarted = false;
        private bool _disposed = false;

        public PluginManager(
            ILogger<PluginManager> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            PluginDiscovery pluginDiscovery,
            PluginLoader pluginLoader,
            PluginSandbox pluginSandbox,
            PluginConfiguration pluginConfiguration,
            IPluginEventAggregator eventAggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _pluginDiscovery = pluginDiscovery ?? throw new ArgumentNullException(nameof(pluginDiscovery));
            _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
            _pluginSandbox = pluginSandbox ?? throw new ArgumentNullException(nameof(pluginSandbox));
            _pluginConfiguration = pluginConfiguration ?? throw new ArgumentNullException(nameof(pluginConfiguration));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            // Initialize timers
            _healthCheckTimer = new Timer(CheckPluginHealth, null, Timeout.Infinite, Timeout.Infinite);
            _pluginScanTimer = new Timer(ScanForNewPlugins, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Gets all loaded plugins
        /// </summary>
        public IReadOnlyDictionary<string, LoadedPlugin> LoadedPlugins => _loadedPlugins.ToDict();

        /// <summary>
        /// Gets all available plugins (loaded and unloaded)
        /// </summary>
        public IReadOnlyDictionary<string, PluginMetadata> AvailablePlugins => _availablePlugins.ToDict();

        /// <summary>
        /// Event raised when a plugin is loaded
        /// </summary>
        public event EventHandler<PluginLoadedEventArgs>? PluginLoaded;

        /// <summary>
        /// Event raised when a plugin is unloaded
        /// </summary>
        public event EventHandler<PluginUnloadedEventArgs>? PluginUnloaded;

        /// <summary>
        /// Event raised when a plugin state changes
        /// </summary>
        public event EventHandler<PluginStateChangedEventArgs>? PluginStateChanged;

        /// <summary>
        /// Start the plugin manager
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Plugin Manager");

            try
            {
                // Discover available plugins
                await DiscoverPluginsAsync(cancellationToken);

                // Load enabled plugins
                await LoadEnabledPluginsAsync(cancellationToken);

                // Start health checking
                var healthCheckInterval = _configuration.GetValue("Plugins:HealthCheckIntervalSeconds", 30);
                _healthCheckTimer.Change(TimeSpan.FromSeconds(healthCheckInterval), TimeSpan.FromSeconds(healthCheckInterval));

                // Start plugin scanning
                var scanInterval = _configuration.GetValue("Plugins:ScanIntervalSeconds", 300);
                _pluginScanTimer.Change(TimeSpan.FromSeconds(scanInterval), TimeSpan.FromSeconds(scanInterval));

                _isStarted = true;
                _logger.LogInformation("Plugin Manager started successfully. Loaded {LoadedCount} plugins", _loadedPlugins.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Plugin Manager");
                throw;
            }
        }

        /// <summary>
        /// Stop the plugin manager
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Plugin Manager");

            try
            {
                _isStarted = false;

                // Stop timers
                _healthCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _pluginScanTimer.Change(Timeout.Infinite, Timeout.Infinite);

                // Stop all plugins
                await StopAllPluginsAsync(cancellationToken);

                _logger.LogInformation("Plugin Manager stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Plugin Manager");
                throw;
            }
        }

        /// <summary>
        /// Load a specific plugin
        /// </summary>
        public async Task<bool> LoadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (_loadedPlugins.ContainsKey(pluginId))
            {
                _logger.LogWarning("Plugin {PluginId} is already loaded", pluginId);
                return true;
            }

            if (!_availablePlugins.TryGetValue(pluginId, out var metadata))
            {
                _logger.LogError("Plugin {PluginId} not found in available plugins", pluginId);
                return false;
            }

            try
            {
                _logger.LogInformation("Loading plugin {PluginId} ({PluginName})", pluginId, metadata.Name);

                // Check dependencies
                if (!await CheckDependenciesAsync(metadata, cancellationToken))
                {
                    _logger.LogError("Dependency check failed for plugin {PluginId}", pluginId);
                    return false;
                }

                // Load the plugin
                var plugin = await _pluginLoader.LoadPluginAsync(metadata, cancellationToken);
                if (plugin == null)
                {
                    _logger.LogError("Failed to load plugin {PluginId}", pluginId);
                    return false;
                }

                // Create plugin context
                var context = CreatePluginContext(metadata);

                // Initialize the plugin
                await plugin.InitializeAsync(context, cancellationToken);

                // Start the plugin
                await plugin.StartAsync(cancellationToken);

                // Create loaded plugin record
                var loadedPlugin = new LoadedPlugin(
                    metadata,
                    plugin,
                    context,
                    DateTime.UtcNow,
                    PluginStatus.Running);

                // Add to loaded plugins
                _loadedPlugins[pluginId] = loadedPlugin;

                // Subscribe to plugin events
                SubscribeToPluginEvents(plugin, pluginId);

                // Raise event
                PluginLoaded?.Invoke(this, new PluginLoadedEventArgs(pluginId, metadata));

                _logger.LogInformation("Plugin {PluginId} loaded successfully", pluginId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin {PluginId}", pluginId);
                return false;
            }
        }

        /// <summary>
        /// Unload a specific plugin
        /// </summary>
        public async Task<bool> UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (!_loadedPlugins.TryRemove(pluginId, out var loadedPlugin))
            {
                _logger.LogWarning("Plugin {PluginId} is not loaded", pluginId);
                return true;
            }

            try
            {
                _logger.LogInformation("Unloading plugin {PluginId}", pluginId);

                // Unsubscribe from plugin events
                UnsubscribeFromPluginEvents(loadedPlugin.Plugin, pluginId);

                // Stop the plugin
                await loadedPlugin.Plugin.StopAsync(cancellationToken);

                // Dispose the plugin
                loadedPlugin.Plugin.Dispose();

                // Unload from sandbox
                await _pluginSandbox.UnloadPluginAsync(pluginId, cancellationToken);

                // Raise event
                PluginUnloaded?.Invoke(this, new PluginUnloadedEventArgs(pluginId, loadedPlugin.Metadata));

                _logger.LogInformation("Plugin {PluginId} unloaded successfully", pluginId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unload plugin {PluginId}", pluginId);
                return false;
            }
        }

        /// <summary>
        /// Reload a specific plugin
        /// </summary>
        public async Task<bool> ReloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reloading plugin {PluginId}", pluginId);

            var wasLoaded = _loadedPlugins.ContainsKey(pluginId);
            
            if (wasLoaded)
            {
                await UnloadPluginAsync(pluginId, cancellationToken);
            }

            // Re-discover the plugin to get updated metadata
            await DiscoverPluginsAsync(cancellationToken);

            if (wasLoaded)
            {
                return await LoadPluginAsync(pluginId, cancellationToken);
            }

            return true;
        }

        /// <summary>
        /// Get plugin by ID
        /// </summary>
        public IPlugin? GetPlugin(string pluginId)
        {
            return _loadedPlugins.TryGetValue(pluginId, out var loadedPlugin) ? loadedPlugin.Plugin : null;
        }

        /// <summary>
        /// Get plugins implementing a specific interface
        /// </summary>
        public IEnumerable<T> GetPlugins<T>() where T : class, IPlugin
        {
            return _loadedPlugins.Values
                .Where(p => p.Plugin is T)
                .Select(p => (T)p.Plugin);
        }

        /// <summary>
        /// Get plugin health status
        /// </summary>
        public PluginHealthStatus? GetPluginHealth(string pluginId)
        {
            return _loadedPlugins.TryGetValue(pluginId, out var loadedPlugin) 
                ? loadedPlugin.Plugin.GetHealthStatus() 
                : null;
        }

        private async Task DiscoverPluginsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Discovering plugins");

            var discoveredPlugins = await _pluginDiscovery.DiscoverPluginsAsync(cancellationToken);
            
            foreach (var plugin in discoveredPlugins)
            {
                _availablePlugins[plugin.Id] = plugin;
                _logger.LogDebug("Discovered plugin {PluginId} ({PluginName})", plugin.Id, plugin.Name);
            }

            _logger.LogInformation("Discovered {PluginCount} plugins", discoveredPlugins.Count);
        }

        private async Task LoadEnabledPluginsAsync(CancellationToken cancellationToken)
        {
            var enabledPlugins = _availablePlugins.Values
                .Where(p => p.IsEnabled && p.LoadOnStartup)
                .OrderBy(p => GetPluginLoadOrder(p))
                .ToList();

            _logger.LogInformation("Loading {EnabledCount} enabled plugins", enabledPlugins.Count);

            foreach (var plugin in enabledPlugins)
            {
                await LoadPluginAsync(plugin.Id, cancellationToken);
            }
        }

        private async Task StopAllPluginsAsync(CancellationToken cancellationToken)
        {
            var pluginsToStop = _loadedPlugins.Values.ToList();
            
            foreach (var loadedPlugin in pluginsToStop)
            {
                await UnloadPluginAsync(loadedPlugin.Metadata.Id, cancellationToken);
            }
        }

        private async Task<bool> CheckDependenciesAsync(PluginMetadata metadata, CancellationToken cancellationToken)
        {
            foreach (var dependency in metadata.Dependencies)
            {
                if (!dependency.IsOptional)
                {
                    if (!_loadedPlugins.ContainsKey(dependency.PluginId))
                    {
                        // Try to load the dependency
                        if (!await LoadPluginAsync(dependency.PluginId, cancellationToken))
                        {
                            _logger.LogError("Required dependency {DependencyId} could not be loaded for plugin {PluginId}", 
                                dependency.PluginId, metadata.Id);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private PluginContext CreatePluginContext(PluginMetadata metadata)
        {
            var config = _pluginConfiguration.GetPluginConfiguration(metadata.Id);
            var workingDir = Path.Combine(_configuration.GetValue("Plugins:WorkingDirectory", "plugins"), metadata.Id);
            var dataDir = Path.Combine(_configuration.GetValue("Plugins:DataDirectory", "data/plugins"), metadata.Id);
            var tempDir = Path.Combine(Path.GetTempPath(), "diceroller-plugins", metadata.Id);

            // Ensure directories exist
            Directory.CreateDirectory(workingDir);
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(tempDir);

            var hostInfo = new HostApplicationInfo(
                "DiceRoller.Net",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0),
                _configuration.GetValue("Environment", "Production"),
                DateTime.UtcNow);

            var securityContext = new PluginSecurityContext(
                metadata.Permissions,
                true, // Always sandboxed for security
                PluginTrustLevel.LimitedTrust,
                new List<string>());

            var resourceManager = _serviceProvider.GetRequiredService<IPluginResourceManager>();
            var performanceMonitor = _serviceProvider.GetRequiredService<IPluginPerformanceMonitor>();

            return new PluginContext(
                _serviceProvider,
                _serviceProvider.GetRequiredService<ILoggerFactory>(),
                _configuration,
                config,
                hostInfo,
                metadata,
                workingDir,
                dataDir,
                tempDir,
                _eventAggregator,
                resourceManager,
                securityContext,
                performanceMonitor);
        }

        private void SubscribeToPluginEvents(IPlugin plugin, string pluginId)
        {
            plugin.ErrorOccurred += (sender, args) =>
            {
                _logger.LogError(args.Exception, "Plugin {PluginId} error: {Message}", pluginId, args.Message);
            };

            plugin.StatusChanged += (sender, args) =>
            {
                _logger.LogDebug("Plugin {PluginId} status changed from {OldStatus} to {NewStatus}", 
                    pluginId, args.PreviousStatus, args.NewStatus);
                
                PluginStateChanged?.Invoke(this, new PluginStateChangedEventArgs(pluginId, args.PreviousStatus, args.NewStatus));
            };
        }

        private void UnsubscribeFromPluginEvents(IPlugin plugin, string pluginId)
        {
            // Note: In a real implementation, you'd need to keep references to the event handlers
            // to properly unsubscribe. This is a simplified version.
        }

        private int GetPluginLoadOrder(PluginMetadata metadata)
        {
            // Simple load order based on dependency count
            return metadata.Dependencies.Count;
        }

        private void CheckPluginHealth(object? state)
        {
            if (!_isStarted) return;

            try
            {
                foreach (var (pluginId, loadedPlugin) in _loadedPlugins)
                {
                    try
                    {
                        var health = loadedPlugin.Plugin.GetHealthStatus();
                        if (!health.IsHealthy)
                        {
                            _logger.LogWarning("Plugin {PluginId} is unhealthy: {Status}", pluginId, health.Status);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking health for plugin {PluginId}", pluginId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during plugin health check");
            }
        }

        private async void ScanForNewPlugins(object? state)
        {
            if (!_isStarted) return;

            try
            {
                await DiscoverPluginsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during plugin scan");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _healthCheckTimer?.Dispose();
            _pluginScanTimer?.Dispose();

            foreach (var loadedPlugin in _loadedPlugins.Values)
            {
                try
                {
                    loadedPlugin.Plugin.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing plugin {PluginId}", loadedPlugin.Metadata.Id);
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Represents a loaded plugin with its context
    /// </summary>
    public record LoadedPlugin(
        PluginMetadata Metadata,
        IPlugin Plugin,
        PluginContext Context,
        DateTime LoadTime,
        PluginStatus Status);

    /// <summary>
    /// Event arguments for plugin loaded event
    /// </summary>
    public class PluginLoadedEventArgs : EventArgs
    {
        public string PluginId { get; }
        public PluginMetadata Metadata { get; }

        public PluginLoadedEventArgs(string pluginId, PluginMetadata metadata)
        {
            PluginId = pluginId;
            Metadata = metadata;
        }
    }

    /// <summary>
    /// Event arguments for plugin unloaded event
    /// </summary>
    public class PluginUnloadedEventArgs : EventArgs
    {
        public string PluginId { get; }
        public PluginMetadata Metadata { get; }

        public PluginUnloadedEventArgs(string pluginId, PluginMetadata metadata)
        {
            PluginId = pluginId;
            Metadata = metadata;
        }
    }

    /// <summary>
    /// Event arguments for plugin state changed event
    /// </summary>
    public class PluginStateChangedEventArgs : EventArgs
    {
        public string PluginId { get; }
        public PluginStatus PreviousStatus { get; }
        public PluginStatus NewStatus { get; }

        public PluginStateChangedEventArgs(string pluginId, PluginStatus previousStatus, PluginStatus newStatus)
        {
            PluginId = pluginId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
        }
    }
}

internal static class Extensions
{
    public static Dictionary<TKey, TValue> ToDict<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source) where TKey : notnull
    {
        return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}