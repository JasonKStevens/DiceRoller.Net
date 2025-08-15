using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.Manager
{
    /// <summary>
    /// Handles loading and unloading of plugin assemblies with isolation support
    /// </summary>
    public class PluginLoader : IDisposable
    {
        private readonly ILogger<PluginLoader> _logger;
        private readonly PluginAssemblyLoadContext _loadContext;
        private bool _disposed = false;

        public PluginLoader(ILogger<PluginLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadContext = new PluginAssemblyLoadContext("DiceRollerPlugins");
        }

        /// <summary>
        /// Load a plugin from metadata
        /// </summary>
        /// <param name="metadata">Plugin metadata</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Loaded plugin instance</returns>
        public async Task<IPlugin?> LoadPluginAsync(PluginMetadata metadata, CancellationToken cancellationToken = default)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            try
            {
                _logger.LogDebug("Loading plugin assembly for {PluginId}", metadata.Id);

                // Resolve assembly path
                var assemblyPath = ResolveAssemblyPath(metadata);
                if (!File.Exists(assemblyPath))
                {
                    _logger.LogError("Plugin assembly not found: {AssemblyPath}", assemblyPath);
                    return null;
                }

                // Load assembly
                var assembly = await LoadAssemblyAsync(assemblyPath, cancellationToken);
                if (assembly == null)
                {
                    _logger.LogError("Failed to load assembly: {AssemblyPath}", assemblyPath);
                    return null;
                }

                // Create plugin instance
                var plugin = CreatePluginInstance(assembly, metadata);
                if (plugin == null)
                {
                    _logger.LogError("Failed to create plugin instance for {PluginId}", metadata.Id);
                    return null;
                }

                _logger.LogDebug("Successfully loaded plugin {PluginId}", metadata.Id);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugin {PluginId}", metadata.Id);
                return null;
            }
        }

        /// <summary>
        /// Unload a plugin assembly
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Unloading plugin assembly for {PluginId}", pluginId);

                // In .NET, we can't easily unload individual assemblies from an AssemblyLoadContext
                // This is a placeholder for more sophisticated unloading logic
                await Task.CompletedTask;

                _logger.LogDebug("Plugin assembly unloaded for {PluginId}", pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading plugin {PluginId}", pluginId);
            }
        }

        private string ResolveAssemblyPath(PluginMetadata metadata)
        {
            // Check if it's an absolute path
            if (Path.IsPathRooted(metadata.AssemblyFileName))
            {
                return metadata.AssemblyFileName;
            }

            // Try relative to plugin directory
            var pluginDir = Path.GetDirectoryName(metadata.AssemblyFileName) ?? string.Empty;
            var fileName = Path.GetFileName(metadata.AssemblyFileName);

            // Common plugin locations
            var searchPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "plugins", pluginDir, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "plugins", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", pluginDir, fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName),
                metadata.AssemblyFileName
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return metadata.AssemblyFileName; // Return original if not found
        }

        private async Task<Assembly?> LoadAssemblyAsync(string assemblyPath, CancellationToken cancellationToken)
        {
            try
            {
                // Validate assembly before loading
                if (!await IsValidAssemblyAsync(assemblyPath, cancellationToken))
                {
                    _logger.LogError("Assembly validation failed: {AssemblyPath}", assemblyPath);
                    return null;
                }

                // Load the assembly in the isolated context
                var assembly = _loadContext.LoadFromAssemblyPath(assemblyPath);

                // Load dependencies
                await LoadDependenciesAsync(assembly, assemblyPath, cancellationToken);

                return assembly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly: {AssemblyPath}", assemblyPath);
                return null;
            }
        }

        private async Task<bool> IsValidAssemblyAsync(string assemblyPath, CancellationToken cancellationToken)
        {
            try
            {
                // Basic validation - check if it's a .NET assembly
                using var stream = File.OpenRead(assemblyPath);
                var buffer = new byte[4];
                await stream.ReadAsync(buffer, 0, 4, cancellationToken);

                // Check for PE header
                return buffer[0] == 0x4D && buffer[1] == 0x5A; // "MZ"
            }
            catch
            {
                return false;
            }
        }

        private async Task LoadDependenciesAsync(Assembly assembly, string assemblyPath, CancellationToken cancellationToken)
        {
            try
            {
                var assemblyDir = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                foreach (var referencedAssembly in referencedAssemblies)
                {
                    try
                    {
                        // Try to load from the same directory as the plugin
                        var depPath = Path.Combine(assemblyDir, referencedAssembly.Name + ".dll");
                        if (File.Exists(depPath))
                        {
                            _loadContext.LoadFromAssemblyPath(depPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not load dependency {DependencyName} for assembly {AssemblyPath}", 
                            referencedAssembly.Name, assemblyPath);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading dependencies for {AssemblyPath}", assemblyPath);
            }
        }

        private IPlugin? CreatePluginInstance(Assembly assembly, PluginMetadata metadata)
        {
            try
            {
                // Find the plugin type
                Type? pluginType = null;

                if (!string.IsNullOrEmpty(metadata.PluginTypeName))
                {
                    pluginType = assembly.GetType(metadata.PluginTypeName);
                }

                if (pluginType == null)
                {
                    // Search for types implementing IPlugin
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .ToArray();

                    if (pluginTypes.Length == 0)
                    {
                        _logger.LogError("No plugin types found in assembly for {PluginId}", metadata.Id);
                        return null;
                    }

                    if (pluginTypes.Length > 1)
                    {
                        _logger.LogWarning("Multiple plugin types found in assembly for {PluginId}, using first one", metadata.Id);
                    }

                    pluginType = pluginTypes[0];
                }

                // Create instance
                var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                if (plugin == null)
                {
                    _logger.LogError("Failed to create instance of plugin type {TypeName} for {PluginId}", 
                        pluginType.Name, metadata.Id);
                    return null;
                }

                // Verify plugin properties match metadata
                if (!ValidatePluginInstance(plugin, metadata))
                {
                    _logger.LogError("Plugin instance validation failed for {PluginId}", metadata.Id);
                    return null;
                }

                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin instance for {PluginId}", metadata.Id);
                return null;
            }
        }

        private bool ValidatePluginInstance(IPlugin plugin, PluginMetadata metadata)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(plugin.Id) || string.IsNullOrEmpty(plugin.Name))
                {
                    _logger.LogError("Plugin has invalid basic properties");
                    return false;
                }

                // Check if IDs match
                if (plugin.Id != metadata.Id)
                {
                    _logger.LogWarning("Plugin ID mismatch: metadata={MetadataId}, plugin={PluginId}", 
                        metadata.Id, plugin.Id);
                }

                // Version compatibility check
                if (plugin.Version != metadata.ParsedVersion)
                {
                    _logger.LogWarning("Plugin version mismatch: metadata={MetadataVersion}, plugin={PluginVersion}", 
                        metadata.Version, plugin.Version);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating plugin instance");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _loadContext?.Unload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing plugin loader");
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Custom AssemblyLoadContext for plugin isolation
    /// </summary>
    internal class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(string name) : base(name, isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(AppDomain.CurrentDomain.BaseDirectory);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Allow host assemblies to be loaded
            if (IsHostAssembly(assemblyName))
            {
                return null; // Let default context handle it
            }

            // Try to resolve from plugin directory
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        private static bool IsHostAssembly(AssemblyName assemblyName)
        {
            // List of assemblies that should be loaded from the host context
            var hostAssemblies = new[]
            {
                "DiceRoller.Plugins.Infrastructure",
                "DiceRoller.Plugins.SDK",
                "DiceRoller.Shared",
                "DiceRoller.Domain.Core",
                "Microsoft.Extensions",
                "System",
                "netstandard",
                "mscorlib"
            };

            var name = assemblyName.Name ?? string.Empty;
            return hostAssemblies.Any(hostAssembly => name.StartsWith(hostAssembly, StringComparison.OrdinalIgnoreCase));
        }
    }
}