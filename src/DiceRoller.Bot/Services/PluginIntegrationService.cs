using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Functions;
using DiceRoller.Core.Parser;
using DiceRoller.Core.Services.Help;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.Manager;
using Irony.Parsing;

namespace DiceRoller.Bot.Services
{
    /// <summary>
    /// Service that integrates plugins with DiceRoller core functionality
    /// </summary>
    public class PluginIntegrationService : IPluginIntegrationService, IDisposable
    {
        private readonly ILogger<PluginIntegrationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IFunctionRegistry _functionRegistry;
        private readonly IGameSystemRouter _gameSystemRouter;
        private readonly ICommandHelpService _helpService;
        private readonly PluginManager _pluginManager;
        
        private readonly Dictionary<string, PluginFunctionAdapter> _functionAdapters = new();
        private readonly Dictionary<string, PluginGameSystemAdapter> _gameSystemAdapters = new();
        private bool _isInitialized = false;
        private bool _disposed = false;

        public PluginIntegrationService(
            ILogger<PluginIntegrationService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IFunctionRegistry functionRegistry,
            IGameSystemRouter gameSystemRouter,
            ICommandHelpService helpService,
            PluginManager pluginManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _functionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
            _gameSystemRouter = gameSystemRouter ?? throw new ArgumentNullException(nameof(gameSystemRouter));
            _helpService = helpService ?? throw new ArgumentNullException(nameof(helpService));
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));

            // Subscribe to plugin manager events
            _pluginManager.PluginLoaded += OnPluginLoaded;
            _pluginManager.PluginUnloaded += OnPluginUnloaded;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, LoadedPlugin> LoadedPlugins => _pluginManager.LoadedPlugins;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, PluginMetadata> AvailablePlugins => _pluginManager.AvailablePlugins;

        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc/>
        public event EventHandler<PluginIntegrationEventArgs>? PluginsChanged;

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized)
            {
                _logger.LogWarning("Plugin integration service is already initialized");
                return;
            }

            _logger.LogInformation("Initializing Plugin Integration Service");

            try
            {
                // Ensure plugins directory exists
                var pluginsDirectory = GetPluginsDirectory();
                if (!Directory.Exists(pluginsDirectory))
                {
                    Directory.CreateDirectory(pluginsDirectory);
                    _logger.LogInformation("Created plugins directory: {PluginsDirectory}", pluginsDirectory);
                }

                // The PluginManager is already started as a hosted service, so we just need to integrate existing plugins
                await LoadPluginsAsync(cancellationToken);

                _isInitialized = true;
                _logger.LogInformation("Plugin Integration Service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Plugin Integration Service");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task LoadPluginsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Loading and integrating plugins with core services");

            try
            {
                var loadedPlugins = _pluginManager.LoadedPlugins;
                _logger.LogInformation("Found {PluginCount} loaded plugins", loadedPlugins.Count);

                foreach (var (pluginId, loadedPlugin) in loadedPlugins)
                {
                    await IntegratePluginAsync(pluginId, loadedPlugin, cancellationToken);
                }

                _logger.LogInformation("Plugin integration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during plugin integration");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ReloadPluginsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reloading all plugins");

            try
            {
                // Unregister all current plugin integrations
                await UnregisterAllPluginIntegrationsAsync();

                // Get all current plugin IDs
                var pluginIds = _pluginManager.LoadedPlugins.Keys.ToList();

                // Reload each plugin
                foreach (var pluginId in pluginIds)
                {
                    await _pluginManager.ReloadPluginAsync(pluginId, cancellationToken);
                }

                // Re-integrate all plugins
                await LoadPluginsAsync(cancellationToken);

                PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Reloaded, "all"));
                _logger.LogInformation("All plugins reloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading plugins");
                PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Error, "all", ex.Message));
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> LoadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            try
            {
                _logger.LogInformation("Loading plugin {PluginId}", pluginId);

                var success = await _pluginManager.LoadPluginAsync(pluginId, cancellationToken);
                if (success && _pluginManager.LoadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
                {
                    await IntegratePluginAsync(pluginId, loadedPlugin, cancellationToken);
                    PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Loaded, pluginId));
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugin {PluginId}", pluginId);
                PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Error, pluginId, ex.Message));
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            try
            {
                _logger.LogInformation("Unloading plugin {PluginId}", pluginId);

                // Unregister plugin integrations first
                await UnregisterPluginIntegrationsAsync(pluginId);

                var success = await _pluginManager.UnloadPluginAsync(pluginId, cancellationToken);
                if (success)
                {
                    PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Unloaded, pluginId));
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading plugin {PluginId}", pluginId);
                PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Error, pluginId, ex.Message));
                return false;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<PluginInfo> GetPluginInfo()
        {
            var pluginInfos = new List<PluginInfo>();

            foreach (var (pluginId, loadedPlugin) in _pluginManager.LoadedPlugins)
            {
                var providedFunctions = new List<string>();
                var providedCommands = new List<string>();

                // Check if it's a dice notation plugin
                if (loadedPlugin.Plugin is IDiceNotationPlugin diceNotationPlugin)
                {
                    providedFunctions.AddRange(diceNotationPlugin.FunctionExtensions.Select(f => f.Name));
                }

                // Check if it's a game system plugin
                if (loadedPlugin.Plugin is IGameSystemPlugin gameSystemPlugin)
                {
                    providedCommands.AddRange(gameSystemPlugin.Commands.Select(c => c.Name));
                }

                var pluginInfo = new PluginInfo(
                    loadedPlugin.Metadata.Id,
                    loadedPlugin.Metadata.Name,
                    loadedPlugin.Metadata.ParsedVersion,
                    loadedPlugin.Metadata.Description,
                    loadedPlugin.Metadata.Author,
                    loadedPlugin.Status,
                    loadedPlugin.Plugin is IGameSystemPlugin,
                    loadedPlugin.Plugin is IDiceNotationPlugin,
                    providedFunctions.AsReadOnly(),
                    providedCommands.AsReadOnly(),
                    loadedPlugin.LoadTime);

                pluginInfos.Add(pluginInfo);
            }

            // Add available but unloaded plugins
            foreach (var (pluginId, metadata) in _pluginManager.AvailablePlugins.Where(kvp => !_pluginManager.LoadedPlugins.ContainsKey(kvp.Key)))
            {
                var pluginInfo = new PluginInfo(
                    metadata.Id,
                    metadata.Name,
                    metadata.ParsedVersion,
                    metadata.Description,
                    metadata.Author,
                    PluginStatus.Unloaded,
                    false, // Can't determine plugin type without loading
                    false,
                    Array.Empty<string>(),
                    Array.Empty<string>());

                pluginInfos.Add(pluginInfo);
            }

            return pluginInfos;
        }

        /// <inheritdoc/>
        public PluginHealthStatus? GetPluginHealth(string pluginId)
        {
            return _pluginManager.GetPluginHealth(pluginId);
        }

        private async Task IntegratePluginAsync(string pluginId, LoadedPlugin loadedPlugin, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Integrating plugin {PluginId} with core services", pluginId);

            try
            {
                // Integrate dice notation functions
                if (loadedPlugin.Plugin is IDiceNotationPlugin diceNotationPlugin)
                {
                    await IntegrateDiceNotationPluginAsync(pluginId, diceNotationPlugin, cancellationToken);
                }

                // Integrate game system functionality
                if (loadedPlugin.Plugin is IGameSystemPlugin gameSystemPlugin)
                {
                    await IntegrateGameSystemPluginAsync(pluginId, gameSystemPlugin, cancellationToken);
                }

                _logger.LogDebug("Plugin {PluginId} integrated successfully", pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error integrating plugin {PluginId}", pluginId);
                throw;
            }
        }

        private async Task IntegrateDiceNotationPluginAsync(string pluginId, IDiceNotationPlugin plugin, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Integrating dice notation functions from plugin {PluginId}", pluginId);

            foreach (var functionExtension in plugin.FunctionExtensions)
            {
                try
                {
                    var adapter = new PluginFunctionAdapter(pluginId, functionExtension, plugin);
                    _functionRegistry.RegisterFunction(functionExtension.Name, adapter);
                    _functionAdapters[$"{pluginId}:{functionExtension.Name}"] = adapter;

                    _logger.LogDebug("Registered function {FunctionName} from plugin {PluginId}", functionExtension.Name, pluginId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering function {FunctionName} from plugin {PluginId}", functionExtension.Name, pluginId);
                }
            }

            await Task.CompletedTask;
        }

        private async Task IntegrateGameSystemPluginAsync(string pluginId, IGameSystemPlugin plugin, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Integrating game system from plugin {PluginId}", pluginId);

            try
            {
                var adapter = new PluginGameSystemAdapter(pluginId, plugin);
                _gameSystemRouter.RegisterSystem(adapter);
                _gameSystemAdapters[pluginId] = adapter;

                _logger.LogDebug("Registered game system {GameSystemName} from plugin {PluginId}", plugin.GameSystemName, pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering game system from plugin {PluginId}", pluginId);
            }

            await Task.CompletedTask;
        }

        private async Task UnregisterAllPluginIntegrationsAsync()
        {
            _logger.LogDebug("Unregistering all plugin integrations");

            // Unregister all game systems
            foreach (var adapter in _gameSystemAdapters.Values)
            {
                _gameSystemRouter.UnregisterSystem(adapter);
            }
            _gameSystemAdapters.Clear();

            // Unregister all functions from the registry
            foreach (var (adapterKey, adapter) in _functionAdapters)
            {
                var success = _functionRegistry.UnregisterFunction(adapter.Name);
                if (success)
                {
                    _logger.LogDebug("Unregistered function {FunctionName}", adapter.Name);
                }
                else
                {
                    _logger.LogWarning("Failed to unregister function {FunctionName} - it may not have been registered", adapter.Name);
                }
            }
            _functionAdapters.Clear();

            await Task.CompletedTask;
        }

        private async Task UnregisterPluginIntegrationsAsync(string pluginId)
        {
            _logger.LogDebug("Unregistering integrations for plugin {PluginId}", pluginId);

            // Unregister game system
            if (_gameSystemAdapters.TryGetValue(pluginId, out var gameSystemAdapter))
            {
                _gameSystemRouter.UnregisterSystem(gameSystemAdapter);
                _gameSystemAdapters.Remove(pluginId);
                _logger.LogDebug("Unregistered game system from plugin {PluginId}", pluginId);
            }

            // Unregister function adapters from the registry
            var functionKeysToRemove = _functionAdapters.Keys.Where(k => k.StartsWith($"{pluginId}:")).ToList();
            foreach (var key in functionKeysToRemove)
            {
                if (_functionAdapters.TryGetValue(key, out var adapter))
                {
                    var success = _functionRegistry.UnregisterFunction(adapter.Name);
                    if (success)
                    {
                        _logger.LogDebug("Unregistered function {FunctionName} from plugin {PluginId}", adapter.Name, pluginId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to unregister function {FunctionName} from plugin {PluginId} - it may not have been registered", adapter.Name, pluginId);
                    }
                }
                _functionAdapters.Remove(key);
            }

            // Unregister help content
            UnregisterPluginHelp(pluginId);

            await Task.CompletedTask;
        }

        private string GetPluginsDirectory()
        {
            return _configuration.GetValue("Plugins:Directory", Path.Combine(Directory.GetCurrentDirectory(), "plugins"))!;
        }

        private void OnPluginLoaded(object? sender, PluginLoadedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (_pluginManager.LoadedPlugins.TryGetValue(e.PluginId, out var loadedPlugin))
                    {
                        await IntegratePluginAsync(e.PluginId, loadedPlugin, CancellationToken.None);
                        PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Loaded, e.PluginId));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error integrating newly loaded plugin {PluginId}", e.PluginId);
                    PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Error, e.PluginId, ex.Message));
                }
            });
        }

        private void OnPluginUnloaded(object? sender, PluginUnloadedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await UnregisterPluginIntegrationsAsync(e.PluginId);
                    PluginsChanged?.Invoke(this, new PluginIntegrationEventArgs(PluginIntegrationAction.Unloaded, e.PluginId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unregistering plugin integrations for {PluginId}", e.PluginId);
                }
            });
        }

        /// <inheritdoc/>
        public void RegisterPluginHelp(string commandName, string helpContent, string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    throw new ArgumentException("Command name cannot be null or empty", nameof(commandName));
                }

                if (string.IsNullOrWhiteSpace(helpContent))
                {
                    throw new ArgumentException("Help content cannot be null or empty", nameof(helpContent));
                }

                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));
                }

                _helpService.RegisterPluginHelp(commandName, helpContent, pluginId);
                _logger.LogDebug("Registered help content for command '{CommandName}' from plugin '{PluginId}'", commandName, pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering plugin help for command '{CommandName}' from plugin '{PluginId}'", commandName, pluginId);
                throw;
            }
        }

        /// <inheritdoc/>
        public void UnregisterPluginHelp(string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return;
                }

                _helpService.UnregisterPluginHelp(pluginId);
                _logger.LogDebug("Unregistered all help content for plugin '{PluginId}'", pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering plugin help for plugin '{PluginId}'", pluginId);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _pluginManager.PluginLoaded -= OnPluginLoaded;
            _pluginManager.PluginUnloaded -= OnPluginUnloaded;

            _disposed = true;
        }
    }

    /// <summary>
    /// Adapter to bridge plugin dice functions to IDiceFunction
    /// </summary>
    internal class PluginFunctionAdapter : IDiceFunction
    {
        private readonly string _pluginId;
        private readonly DiceNotationFunction _function;
        private readonly IDiceNotationPlugin _plugin;

        public PluginFunctionAdapter(string pluginId, DiceNotationFunction function, IDiceNotationPlugin plugin)
        {
            _pluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public string Name => _function.Name;

        public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
        {
            try
            {
                // Convert parameters from parse tree
                var parameters = ExtractParameters(node, context);

                // Create dice notation context with minimal information
                var diceContext = new DiceNotationContext(
                    "unknown", // UserId not available in function context
                    node.ToString() ?? string.Empty);

                // Execute the plugin function (this is synchronous wrapper around async)
                var task = _function.Implementation(parameters, diceContext);
                // Use GetAwaiter().GetResult() to avoid potential deadlocks compared to .Wait() and .Result
                var result = task.GetAwaiter().GetResult();

                // Convert result to DiceResultNode
                return ConvertToDiceResultNode(result);
            }
            catch (Exception ex)
            {
                return new DiceResultNode(0, $"Error executing plugin function {Name}: {ex.Message}");
            }
        }

        private object[] ExtractParameters(ParseTreeNode node, IFunctionContext context)
        {
            // Simplified parameter extraction - in real implementation, this would be more sophisticated
            var parameters = new List<object>();
            
            // Extract child nodes as parameters
            if (node.ChildNodes.Count > 1 && node.ChildNodes[1].ChildNodes.Count > 0)
            {
                foreach (var paramNode in node.ChildNodes[1].ChildNodes)
                {
                    // Evaluate parameter node
                    if (paramNode.Term?.Name == "number")
                    {
                        if (int.TryParse(paramNode.Token?.Text, out var intValue))
                            parameters.Add(intValue);
                        else if (double.TryParse(paramNode.Token?.Text, out var doubleValue))
                            parameters.Add(doubleValue);
                    }
                    else if (paramNode.Term?.Name == "string")
                    {
                        parameters.Add(paramNode.Token?.Text?.Trim('"') ?? string.Empty);
                    }
                    // Add more parameter type handling as needed
                }
            }

            return parameters.ToArray();
        }

        private DiceResultNode ConvertToDiceResultNode(object result)
        {
            // Convert plugin result to DiceResultNode format
            if (result is int intResult)
            {
                return new DiceResultNode(intResult, $"{Name}() = {intResult}");
            }
            else if (result is double doubleResult)
            {
                return new DiceResultNode((int)Math.Round(doubleResult), $"{Name}() = {doubleResult:F2}");
            }
            else if (result is string stringResult)
            {
                return new DiceResultNode(0, $"{Name}() = {stringResult}");
            }
            else
            {
                return new DiceResultNode(0, $"{Name}() = {result?.ToString() ?? "null"}");
            }
        }
    }

    /// <summary>
    /// Adapter to bridge plugin game systems to IGameSystem
    /// </summary>
    internal class PluginGameSystemAdapter : IGameSystem
    {
        private readonly string _pluginId;
        private readonly IGameSystemPlugin _plugin;

        public PluginGameSystemAdapter(string pluginId, IGameSystemPlugin plugin)
        {
            _pluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public string Name => _plugin.GameSystemName;

        public IEnumerable<string> SupportedCommands => _plugin.Commands.Select(c => c.Name);

        public bool SupportsCommand(string commandType)
        {
            return _plugin.Commands.Any(c => c.Name.Equals(commandType, StringComparison.OrdinalIgnoreCase));
        }

        public TypedResult ProcessCommand(string commandType, string[] parameters, string userId)
        {
            try
            {
                // Convert parameters to dictionary
                var paramDict = new Dictionary<string, object>();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        paramDict[$"param{i}"] = parameters[i];
                    }
                }

                // Create game system context
                var context = new GameSystemContext(userId);

                // Execute command (synchronous wrapper around async)
                var task = _plugin.ExecuteCommandAsync(commandType, paramDict, context);
                // Use GetAwaiter().GetResult() to avoid potential deadlocks compared to .Wait() and .Result
                var result = task.GetAwaiter().GetResult();

                // Convert result to TypedResult
                return ConvertToTypedResult(result);
            }
            catch (Exception ex)
            {
                return TypedResult.NewSimpleResult(NodeType.Text, $"Error: {ex.Message}");
            }
        }


        private TypedResult ConvertToTypedResult(GameSystemCommandResult result)
        {
            if (result.Success)
            {
                return TypedResult.NewSimpleResult(NodeType.Text, result.Description);
            }
            else
            {
                return TypedResult.NewSimpleResult(NodeType.Text, $"Error: {result.ErrorMessage ?? "Unknown error"}");
            }
        }
    }
}