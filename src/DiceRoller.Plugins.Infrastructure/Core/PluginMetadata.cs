using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Plugin metadata containing information about a plugin
    /// </summary>
    public class PluginMetadata
    {
        /// <summary>
        /// Unique identifier for the plugin
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the plugin
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Plugin version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Plugin description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Plugin author
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Author's website or contact information
        /// </summary>
        [JsonProperty("website")]
        public string? Website { get; set; }

        /// <summary>
        /// Plugin license
        /// </summary>
        [JsonProperty("license")]
        public string License { get; set; } = "MIT";

        /// <summary>
        /// Plugin tags for categorization
        /// </summary>
        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Minimum required DiceRoller version
        /// </summary>
        [JsonProperty("minimumHostVersion")]
        public string MinimumHostVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Maximum supported DiceRoller version (null means no limit)
        /// </summary>
        [JsonProperty("maximumHostVersion")]
        public string? MaximumHostVersion { get; set; }

        /// <summary>
        /// Plugin dependencies
        /// </summary>
        [JsonProperty("dependencies")]
        public List<PluginDependencyMetadata> Dependencies { get; set; } = new();

        /// <summary>
        /// Assembly file name containing the plugin
        /// </summary>
        [JsonProperty("assemblyFileName")]
        public string AssemblyFileName { get; set; } = string.Empty;

        /// <summary>
        /// Main plugin class type name
        /// </summary>
        [JsonProperty("pluginTypeName")]
        public string PluginTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Plugin interfaces implemented
        /// </summary>
        [JsonProperty("interfaces")]
        public List<string> Interfaces { get; set; } = new();

        /// <summary>
        /// Configuration schema for the plugin
        /// </summary>
        [JsonProperty("configurationSchema")]
        public string? ConfigurationSchema { get; set; }

        /// <summary>
        /// Default configuration for the plugin
        /// </summary>
        [JsonProperty("defaultConfiguration")]
        public object? DefaultConfiguration { get; set; }

        /// <summary>
        /// Resource requirements for the plugin
        /// </summary>
        [JsonProperty("resourceRequirements")]
        public PluginResourceRequirements ResourceRequirements { get; set; } = new();

        /// <summary>
        /// Security permissions required by the plugin
        /// </summary>
        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// Plugin installation date
        /// </summary>
        [JsonProperty("installDate")]
        public DateTime? InstallDate { get; set; }

        /// <summary>
        /// Plugin last update date
        /// </summary>
        [JsonProperty("lastUpdateDate")]
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// Whether the plugin is enabled
        /// </summary>
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the plugin should be loaded on startup
        /// </summary>
        [JsonProperty("loadOnStartup")]
        public bool LoadOnStartup { get; set; } = true;

        /// <summary>
        /// Plugin installation source
        /// </summary>
        [JsonProperty("installationSource")]
        public PluginInstallationSource InstallationSource { get; set; } = new();

        /// <summary>
        /// Additional metadata as key-value pairs
        /// </summary>
        [JsonProperty("additionalMetadata")]
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new();

        /// <summary>
        /// Parse version string to Version object
        /// </summary>
        [JsonIgnore]
        public Version ParsedVersion => System.Version.Parse(Version);

        /// <summary>
        /// Parse minimum host version string to Version object
        /// </summary>
        [JsonIgnore]
        public Version ParsedMinimumHostVersion => System.Version.Parse(MinimumHostVersion);

        /// <summary>
        /// Parse maximum host version string to Version object (if specified)
        /// </summary>
        [JsonIgnore]
        public Version? ParsedMaximumHostVersion => MaximumHostVersion != null ? System.Version.Parse(MaximumHostVersion) : null;
    }

    /// <summary>
    /// Plugin dependency metadata
    /// </summary>
    public class PluginDependencyMetadata
    {
        /// <summary>
        /// Plugin ID this depends on
        /// </summary>
        [JsonProperty("pluginId")]
        public string PluginId { get; set; } = string.Empty;

        /// <summary>
        /// Minimum version required
        /// </summary>
        [JsonProperty("minimumVersion")]
        public string MinimumVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Maximum version supported (null means no limit)
        /// </summary>
        [JsonProperty("maximumVersion")]
        public string? MaximumVersion { get; set; }

        /// <summary>
        /// Whether this dependency is optional
        /// </summary>
        [JsonProperty("isOptional")]
        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// Dependency description
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Plugin resource requirements
    /// </summary>
    public class PluginResourceRequirements
    {
        /// <summary>
        /// Maximum memory usage in MB
        /// </summary>
        [JsonProperty("maxMemoryMB")]
        public int? MaxMemoryMB { get; set; }

        /// <summary>
        /// Maximum CPU usage percentage
        /// </summary>
        [JsonProperty("maxCpuPercent")]
        public int? MaxCpuPercent { get; set; }

        /// <summary>
        /// Maximum disk usage in MB
        /// </summary>
        [JsonProperty("maxDiskMB")]
        public int? MaxDiskMB { get; set; }

        /// <summary>
        /// Maximum network bandwidth in KB/s
        /// </summary>
        [JsonProperty("maxNetworkKBps")]
        public int? MaxNetworkKBps { get; set; }

        /// <summary>
        /// Execution timeout in seconds
        /// </summary>
        [JsonProperty("executionTimeoutSeconds")]
        public int? ExecutionTimeoutSeconds { get; set; }
    }

    /// <summary>
    /// Plugin installation source information
    /// </summary>
    public class PluginInstallationSource
    {
        /// <summary>
        /// Source type (e.g., "repository", "local", "url")
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "local";

        /// <summary>
        /// Source URL or path
        /// </summary>
        [JsonProperty("source")]
        public string? Source { get; set; }

        /// <summary>
        /// Repository name (if from repository)
        /// </summary>
        [JsonProperty("repository")]
        public string? Repository { get; set; }

        /// <summary>
        /// Installation method
        /// </summary>
        [JsonProperty("installationMethod")]
        public string? InstallationMethod { get; set; }

        /// <summary>
        /// Checksum for verification
        /// </summary>
        [JsonProperty("checksum")]
        public string? Checksum { get; set; }
    }
}