using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.Discovery
{
    /// <summary>
    /// Interface for plugin repositories that can provide plugins
    /// </summary>
    public interface IPluginRepository
    {
        /// <summary>
        /// Repository name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Repository URL or identifier
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Whether the repository is currently available
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Get all available plugins from this repository
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available plugin metadata</returns>
        Task<List<PluginMetadata>> GetAvailablePluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get specific plugin metadata by ID
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Plugin metadata or null if not found</returns>
        Task<PluginMetadata?> GetPluginAsync(string pluginId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search for plugins matching criteria
        /// </summary>
        /// <param name="searchCriteria">Search criteria</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching plugin metadata</returns>
        Task<List<PluginMetadata>> SearchPluginsAsync(PluginSearchCriteria searchCriteria, CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a plugin from the repository
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="version">Plugin version (null for latest)</param>
        /// <param name="destinationPath">Destination path for the plugin</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Path to downloaded plugin</returns>
        Task<string> DownloadPluginAsync(string pluginId, string? version, string destinationPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get plugin download URL
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="version">Plugin version (null for latest)</param>
        /// <returns>Download URL</returns>
        string GetPluginDownloadUrl(string pluginId, string? version = null);

        /// <summary>
        /// Check if a plugin update is available
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="currentVersion">Current version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update information or null if no update available</returns>
        Task<PluginUpdateInfo?> CheckForUpdateAsync(string pluginId, string currentVersion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get plugin reviews and ratings
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Plugin reviews</returns>
        Task<PluginReviews?> GetPluginReviewsAsync(string pluginId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check repository health
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Repository health status</returns>
        Task<RepositoryHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Plugin search criteria
    /// </summary>
    public class PluginSearchCriteria
    {
        /// <summary>
        /// Search query text
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Plugin tags to filter by
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Plugin author to filter by
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Minimum rating (0-5)
        /// </summary>
        public double? MinimumRating { get; set; }

        /// <summary>
        /// Maximum results to return
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public PluginSearchSortOrder SortOrder { get; set; } = PluginSearchSortOrder.Relevance;

        /// <summary>
        /// Include pre-release versions
        /// </summary>
        public bool IncludePrerelease { get; set; } = false;
    }

    /// <summary>
    /// Plugin search sort order
    /// </summary>
    public enum PluginSearchSortOrder
    {
        Relevance,
        Name,
        Author,
        UpdateDate,
        Rating,
        Downloads
    }

    /// <summary>
    /// Plugin update information
    /// </summary>
    public record PluginUpdateInfo(
        string PluginId,
        string CurrentVersion,
        string LatestVersion,
        string UpdateDescription,
        DateTime ReleaseDate,
        bool IsSecurityUpdate = false,
        bool IsBreakingChange = false);

    /// <summary>
    /// Plugin reviews and ratings
    /// </summary>
    public class PluginReviews
    {
        /// <summary>
        /// Plugin identifier
        /// </summary>
        public string PluginId { get; set; } = string.Empty;

        /// <summary>
        /// Average rating (0-5)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of reviews
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Individual reviews
        /// </summary>
        public List<PluginReview> Reviews { get; set; } = new();

        /// <summary>
        /// Rating distribution
        /// </summary>
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    /// <summary>
    /// Individual plugin review
    /// </summary>
    public record PluginReview(
        string ReviewId,
        string Author,
        int Rating,
        string Title,
        string Comment,
        DateTime Date,
        string? Version = null);

    /// <summary>
    /// Repository health status
    /// </summary>
    public record RepositoryHealthStatus(
        bool IsHealthy,
        string Status,
        TimeSpan ResponseTime,
        DateTime LastCheck,
        string? ErrorMessage = null);

    /// <summary>
    /// File-based plugin repository implementation
    /// </summary>
    public class FilePluginRepository : IPluginRepository
    {
        private readonly string _repositoryPath;

        public string Name { get; }
        public string Url { get; }
        public bool IsAvailable => Directory.Exists(_repositoryPath);

        public FilePluginRepository(string name, string repositoryPath)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _repositoryPath = repositoryPath ?? throw new ArgumentNullException(nameof(repositoryPath));
            Url = $"file://{repositoryPath}";
        }

        public async Task<List<PluginMetadata>> GetAvailablePluginsAsync(CancellationToken cancellationToken = default)
        {
            var plugins = new List<PluginMetadata>();

            if (!IsAvailable)
                return plugins;

            try
            {
                var manifestFiles = Directory.GetFiles(_repositoryPath, "Plugin.manifest.json", SearchOption.AllDirectories);

                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var manifestContent = await File.ReadAllTextAsync(manifestFile, cancellationToken);
                        var metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<PluginMetadata>(manifestContent);
                        
                        if (metadata != null)
                        {
                            // Set installation source
                            metadata.InstallationSource = new PluginInstallationSource
                            {
                                Type = "repository",
                                Source = Url,
                                Repository = Name
                            };

                            plugins.Add(metadata);
                        }
                    }
                    catch (Exception)
                    {
                        // Skip invalid manifest files
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }

            return plugins;
        }

        public async Task<PluginMetadata?> GetPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            var plugins = await GetAvailablePluginsAsync(cancellationToken);
            return plugins.FirstOrDefault(p => p.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<PluginMetadata>> SearchPluginsAsync(PluginSearchCriteria searchCriteria, CancellationToken cancellationToken = default)
        {
            var allPlugins = await GetAvailablePluginsAsync(cancellationToken);
            var filteredPlugins = allPlugins.AsEnumerable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchCriteria.Query))
            {
                filteredPlugins = filteredPlugins.Where(p =>
                    p.Name.Contains(searchCriteria.Query, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchCriteria.Query, StringComparison.OrdinalIgnoreCase) ||
                    p.Tags.Any(t => t.Contains(searchCriteria.Query, StringComparison.OrdinalIgnoreCase)));
            }

            if (searchCriteria.Tags?.Any() == true)
            {
                filteredPlugins = filteredPlugins.Where(p =>
                    searchCriteria.Tags.Any(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(searchCriteria.Author))
            {
                filteredPlugins = filteredPlugins.Where(p =>
                    p.Author.Contains(searchCriteria.Author, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            filteredPlugins = searchCriteria.SortOrder switch
            {
                PluginSearchSortOrder.Name => filteredPlugins.OrderBy(p => p.Name),
                PluginSearchSortOrder.Author => filteredPlugins.OrderBy(p => p.Author),
                PluginSearchSortOrder.UpdateDate => filteredPlugins.OrderByDescending(p => p.LastUpdateDate ?? DateTime.MinValue),
                _ => filteredPlugins.OrderBy(p => p.Name)
            };

            // Apply limit
            if (searchCriteria.MaxResults.HasValue)
            {
                filteredPlugins = filteredPlugins.Take(searchCriteria.MaxResults.Value);
            }

            return filteredPlugins.ToList();
        }

        public async Task<string> DownloadPluginAsync(string pluginId, string? version, string destinationPath, CancellationToken cancellationToken = default)
        {
            var plugin = await GetPluginAsync(pluginId, cancellationToken);
            if (plugin == null)
                throw new FileNotFoundException($"Plugin {pluginId} not found in repository");

            var sourcePath = Path.GetDirectoryName(plugin.AssemblyFileName) ?? string.Empty;
            var destPluginPath = Path.Combine(destinationPath, pluginId);

            // Copy plugin files
            if (Directory.Exists(sourcePath))
            {
                await CopyDirectoryAsync(sourcePath, destPluginPath, cancellationToken);
            }

            return destPluginPath;
        }

        public string GetPluginDownloadUrl(string pluginId, string? version = null)
        {
            return $"{Url}/{pluginId}";
        }

        public async Task<PluginUpdateInfo?> CheckForUpdateAsync(string pluginId, string currentVersion, CancellationToken cancellationToken = default)
        {
            var plugin = await GetPluginAsync(pluginId, cancellationToken);
            if (plugin == null)
                return null;

            var currentVer = Version.Parse(currentVersion);
            var latestVer = plugin.ParsedVersion;

            if (latestVer > currentVer)
            {
                return new PluginUpdateInfo(
                    pluginId,
                    currentVersion,
                    plugin.Version,
                    $"Updated to version {plugin.Version}",
                    plugin.LastUpdateDate ?? DateTime.UtcNow);
            }

            return null;
        }

        public Task<PluginReviews?> GetPluginReviewsAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            // File repositories don't support reviews
            return Task.FromResult<PluginReviews?>(null);
        }

        public async Task<RepositoryHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var isHealthy = IsAvailable;
                var responseTime = DateTime.UtcNow - startTime;

                return new RepositoryHealthStatus(
                    isHealthy,
                    isHealthy ? "Available" : "Directory not found",
                    responseTime,
                    DateTime.UtcNow,
                    isHealthy ? null : $"Repository path not found: {_repositoryPath}");
            }
            catch (Exception ex)
            {
                var responseTime = DateTime.UtcNow - startTime;
                return new RepositoryHealthStatus(
                    false,
                    "Error",
                    responseTime,
                    DateTime.UtcNow,
                    ex.Message);
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string destDir, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                using var sourceStream = File.OpenRead(file);
                using var destStream = File.Create(destFile);
                await sourceStream.CopyToAsync(destStream, cancellationToken);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                await CopyDirectoryAsync(subDir, destSubDir, cancellationToken);
            }
        }
    }
}