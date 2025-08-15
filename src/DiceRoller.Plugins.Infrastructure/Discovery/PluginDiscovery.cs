using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.Discovery
{
    /// <summary>
    /// Discovers and catalogs available plugins from various sources
    /// </summary>
    public class PluginDiscovery
    {
        private readonly ILogger<PluginDiscovery> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<IPluginRepository> _repositories;

        public PluginDiscovery(
            ILogger<PluginDiscovery> logger,
            IConfiguration configuration,
            IEnumerable<IPluginRepository> repositories)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _repositories = repositories?.ToList() ?? throw new ArgumentNullException(nameof(repositories));
        }

        /// <summary>
        /// Discover all available plugins from configured sources
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered plugin metadata</returns>
        public async Task<List<PluginMetadata>> DiscoverPluginsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting plugin discovery");

            var discoveredPlugins = new List<PluginMetadata>();

            try
            {
                // Discover from local directories
                var localPlugins = await DiscoverLocalPluginsAsync(cancellationToken);
                discoveredPlugins.AddRange(localPlugins);

                // Discover from repositories
                var repositoryPlugins = await DiscoverRepositoryPluginsAsync(cancellationToken);
                discoveredPlugins.AddRange(repositoryPlugins);

                // Remove duplicates (prefer local over repository)
                var uniquePlugins = RemoveDuplicates(discoveredPlugins);

                _logger.LogInformation("Plugin discovery completed. Found {PluginCount} unique plugins", uniquePlugins.Count);

                return uniquePlugins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during plugin discovery");
                throw;
            }
        }

        /// <summary>
        /// Discover plugins in a specific directory
        /// </summary>
        /// <param name="directory">Directory to search</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered plugin metadata</returns>
        public async Task<List<PluginMetadata>> DiscoverPluginsInDirectoryAsync(string directory, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                _logger.LogWarning("Plugin directory does not exist: {Directory}", directory);
                return new List<PluginMetadata>();
            }

            _logger.LogDebug("Discovering plugins in directory: {Directory}", directory);

            var plugins = new List<PluginMetadata>();

            try
            {
                // Look for plugin manifest files
                var manifestFiles = Directory.GetFiles(directory, "Plugin.manifest.json", SearchOption.AllDirectories);

                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var plugin = await LoadPluginFromManifestAsync(manifestFile, cancellationToken);
                        if (plugin != null)
                        {
                            plugins.Add(plugin);
                            _logger.LogDebug("Discovered plugin {PluginId} from {ManifestFile}", plugin.Id, manifestFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load plugin from manifest: {ManifestFile}", manifestFile);
                    }
                }

                // Look for assemblies without manifests
                var assemblyFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories)
                    .Where(f => !IsSystemAssembly(f))
                    .ToArray();

                foreach (var assemblyFile in assemblyFiles)
                {
                    try
                    {
                        // Check if there's already a manifest for this assembly
                        var assemblyDir = Path.GetDirectoryName(assemblyFile) ?? string.Empty;
                        var manifestPath = Path.Combine(assemblyDir, "Plugin.manifest.json");

                        if (!File.Exists(manifestPath))
                        {
                            var plugin = await TryCreateMetadataFromAssemblyAsync(assemblyFile, cancellationToken);
                            if (plugin != null)
                            {
                                plugins.Add(plugin);
                                _logger.LogDebug("Discovered plugin {PluginId} from assembly {AssemblyFile}", plugin.Id, assemblyFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Could not discover plugin from assembly: {AssemblyFile}", assemblyFile);
                    }
                }

                _logger.LogDebug("Discovered {PluginCount} plugins in directory {Directory}", plugins.Count, directory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering plugins in directory: {Directory}", directory);
            }

            return plugins;
        }

        /// <summary>
        /// Validate plugin metadata
        /// </summary>
        /// <param name="metadata">Plugin metadata to validate</param>
        /// <returns>True if metadata is valid</returns>
        public bool ValidatePluginMetadata(PluginMetadata metadata)
        {
            if (metadata == null)
                return false;

            var errors = new List<string>();

            // Required fields
            if (string.IsNullOrEmpty(metadata.Id))
                errors.Add("Plugin ID is required");

            if (string.IsNullOrEmpty(metadata.Name))
                errors.Add("Plugin name is required");

            if (string.IsNullOrEmpty(metadata.Version))
                errors.Add("Plugin version is required");

            if (string.IsNullOrEmpty(metadata.AssemblyFileName))
                errors.Add("Assembly file name is required");

            // Version format validation
            try
            {
                var _ = metadata.ParsedVersion;
            }
            catch
            {
                errors.Add("Invalid version format");
            }

            try
            {
                var _ = metadata.ParsedMinimumHostVersion;
            }
            catch
            {
                errors.Add("Invalid minimum host version format");
            }

            // Assembly file existence
            if (!string.IsNullOrEmpty(metadata.AssemblyFileName))
            {
                var assemblyPath = ResolveAssemblyPath(metadata.AssemblyFileName);
                if (!File.Exists(assemblyPath))
                {
                    errors.Add($"Assembly file not found: {assemblyPath}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Plugin metadata validation failed for {PluginId}: {Errors}", 
                    metadata.Id, string.Join(", ", errors));
                return false;
            }

            return true;
        }

        private async Task<List<PluginMetadata>> DiscoverLocalPluginsAsync(CancellationToken cancellationToken)
        {
            var plugins = new List<PluginMetadata>();

            // Get plugin directories from configuration
            var pluginDirectories = _configuration.GetSection("Plugins:Directories").Get<string[]>() ?? new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"),
                Path.Combine(Directory.GetCurrentDirectory(), "plugins")
            };

            foreach (var directory in pluginDirectories)
            {
                if (Directory.Exists(directory))
                {
                    var directoryPlugins = await DiscoverPluginsInDirectoryAsync(directory, cancellationToken);
                    plugins.AddRange(directoryPlugins);
                }
            }

            return plugins;
        }

        private async Task<List<PluginMetadata>> DiscoverRepositoryPluginsAsync(CancellationToken cancellationToken)
        {
            var plugins = new List<PluginMetadata>();

            foreach (var repository in _repositories)
            {
                try
                {
                    _logger.LogDebug("Discovering plugins from repository: {RepositoryName}", repository.Name);

                    var repositoryPlugins = await repository.GetAvailablePluginsAsync(cancellationToken);
                    plugins.AddRange(repositoryPlugins);

                    _logger.LogDebug("Found {PluginCount} plugins in repository {RepositoryName}", 
                        repositoryPlugins.Count, repository.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error discovering plugins from repository: {RepositoryName}", repository.Name);
                }
            }

            return plugins;
        }

        private async Task<PluginMetadata?> LoadPluginFromManifestAsync(string manifestPath, CancellationToken cancellationToken)
        {
            try
            {
                var manifestContent = await File.ReadAllTextAsync(manifestPath, cancellationToken);
                var metadata = JsonConvert.DeserializeObject<PluginMetadata>(manifestContent);

                if (metadata == null)
                {
                    _logger.LogWarning("Failed to deserialize plugin manifest: {ManifestPath}", manifestPath);
                    return null;
                }

                // Resolve relative paths
                var manifestDir = Path.GetDirectoryName(manifestPath) ?? string.Empty;
                if (!Path.IsPathRooted(metadata.AssemblyFileName))
                {
                    metadata.AssemblyFileName = Path.Combine(manifestDir, metadata.AssemblyFileName);
                }

                // Validate metadata
                if (!ValidatePluginMetadata(metadata))
                {
                    return null;
                }

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugin manifest: {ManifestPath}", manifestPath);
                return null;
            }
        }

        private async Task<PluginMetadata?> TryCreateMetadataFromAssemblyAsync(string assemblyPath, CancellationToken cancellationToken)
        {
            try
            {
                // This is a simplified implementation
                // In a real scenario, you'd use reflection to examine the assembly
                var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                
                var metadata = new PluginMetadata
                {
                    Id = assemblyName.ToLowerInvariant(),
                    Name = assemblyName,
                    Version = "1.0.0",
                    Description = $"Auto-discovered plugin from {assemblyName}",
                    Author = "Unknown",
                    AssemblyFileName = assemblyPath,
                    IsEnabled = false, // Auto-discovered plugins are disabled by default
                    LoadOnStartup = false
                };

                await Task.CompletedTask;
                return ValidatePluginMetadata(metadata) ? metadata : null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not create metadata from assembly: {AssemblyPath}", assemblyPath);
                return null;
            }
        }

        private List<PluginMetadata> RemoveDuplicates(List<PluginMetadata> plugins)
        {
            var uniquePlugins = new Dictionary<string, PluginMetadata>();

            foreach (var plugin in plugins)
            {
                if (!uniquePlugins.ContainsKey(plugin.Id))
                {
                    uniquePlugins[plugin.Id] = plugin;
                }
                else
                {
                    // Prefer local plugins over repository plugins
                    var existing = uniquePlugins[plugin.Id];
                    if (plugin.InstallationSource.Type == "local" && existing.InstallationSource.Type != "local")
                    {
                        uniquePlugins[plugin.Id] = plugin;
                    }
                }
            }

            return uniquePlugins.Values.ToList();
        }

        private string ResolveAssemblyPath(string assemblyFileName)
        {
            if (Path.IsPathRooted(assemblyFileName))
                return assemblyFileName;

            // Try common locations
            var searchPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "plugins", assemblyFileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", assemblyFileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFileName),
                assemblyFileName
            };

            return searchPaths.FirstOrDefault(File.Exists) ?? assemblyFileName;
        }

        private bool IsSystemAssembly(string assemblyPath)
        {
            var fileName = Path.GetFileName(assemblyPath).ToLowerInvariant();
            
            var systemPrefixes = new[]
            {
                "system.",
                "microsoft.",
                "mscorlib",
                "netstandard",
                "runtime.",
                "windowsbase"
            };

            return systemPrefixes.Any(prefix => fileName.StartsWith(prefix));
        }
    }
}