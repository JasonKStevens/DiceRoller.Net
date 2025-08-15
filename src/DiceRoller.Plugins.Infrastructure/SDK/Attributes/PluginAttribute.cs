using System;

namespace DiceRoller.Plugins.Infrastructure.SDK.Attributes
{
    /// <summary>
    /// Attribute to mark a class as a DiceRoller plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Plugin identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Plugin display name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Plugin author
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Minimum required host version
        /// </summary>
        public string? MinimumHostVersion { get; set; }

        /// <summary>
        /// Plugin category or tags
        /// </summary>
        public string[]? Tags { get; set; }

        /// <summary>
        /// Plugin priority (higher numbers load first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Whether plugin should be enabled by default
        /// </summary>
        public bool EnabledByDefault { get; set; } = true;

        /// <summary>
        /// Whether plugin should load on startup
        /// </summary>
        public bool LoadOnStartup { get; set; } = true;

        public PluginAttribute(string id, string name, string version)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }

    /// <summary>
    /// Attribute to mark a plugin as providing game system functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GameSystemPluginAttribute : Attribute
    {
        /// <summary>
        /// Game system name
        /// </summary>
        public string GameSystemName { get; }

        /// <summary>
        /// Game system version
        /// </summary>
        public string? GameSystemVersion { get; set; }

        /// <summary>
        /// Game system publisher/author
        /// </summary>
        public string? Publisher { get; set; }

        /// <summary>
        /// Whether this is an official game system implementation
        /// </summary>
        public bool IsOfficial { get; set; } = false;

        public GameSystemPluginAttribute(string gameSystemName)
        {
            GameSystemName = gameSystemName ?? throw new ArgumentNullException(nameof(gameSystemName));
        }
    }

    /// <summary>
    /// Attribute to mark a plugin as providing dice notation extensions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DiceNotationPluginAttribute : Attribute
    {
        /// <summary>
        /// Priority for notation processing (higher numbers processed first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Supported notation patterns
        /// </summary>
        public string[]? SupportedPatterns { get; set; }

        /// <summary>
        /// Whether this plugin modifies core dice notation behavior
        /// </summary>
        public bool ModifiesCore { get; set; } = false;
    }

    /// <summary>
    /// Attribute to mark a plugin as providing formatting functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FormatterPluginAttribute : Attribute
    {
        /// <summary>
        /// Supported output formats
        /// </summary>
        public string[] SupportedFormats { get; }

        /// <summary>
        /// Priority for formatter selection (higher numbers processed first)
        /// </summary>
        public int Priority { get; set; } = 0;

        public FormatterPluginAttribute(params string[] supportedFormats)
        {
            SupportedFormats = supportedFormats ?? throw new ArgumentNullException(nameof(supportedFormats));
        }
    }

    /// <summary>
    /// Attribute to specify plugin dependencies
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PluginDependencyAttribute : Attribute
    {
        /// <summary>
        /// Dependency plugin ID
        /// </summary>
        public string PluginId { get; }

        /// <summary>
        /// Minimum required version
        /// </summary>
        public string? MinimumVersion { get; set; }

        /// <summary>
        /// Whether dependency is optional
        /// </summary>
        public bool IsOptional { get; set; } = false;

        public PluginDependencyAttribute(string pluginId)
        {
            PluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
        }
    }

    /// <summary>
    /// Attribute to specify required permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredPermissionAttribute : Attribute
    {
        /// <summary>
        /// Permission name
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Permission description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether permission is critical for plugin functionality
        /// </summary>
        public bool IsCritical { get; set; } = false;

        public RequiredPermissionAttribute(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }

    /// <summary>
    /// Attribute to specify resource requirements
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceRequirementsAttribute : Attribute
    {
        /// <summary>
        /// Maximum memory usage in MB
        /// </summary>
        public int? MaxMemoryMB { get; set; }

        /// <summary>
        /// Maximum CPU usage percentage
        /// </summary>
        public int? MaxCpuPercent { get; set; }

        /// <summary>
        /// Maximum disk usage in MB
        /// </summary>
        public int? MaxDiskMB { get; set; }

        /// <summary>
        /// Execution timeout in seconds
        /// </summary>
        public int? ExecutionTimeoutSeconds { get; set; }
    }

    /// <summary>
    /// Attribute to provide configuration schema
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConfigurationSchemaAttribute : Attribute
    {
        /// <summary>
        /// JSON schema for configuration validation
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Schema version
        /// </summary>
        public string? Version { get; set; }

        public ConfigurationSchemaAttribute(string schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
    }
}