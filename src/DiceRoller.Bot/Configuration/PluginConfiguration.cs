using System.ComponentModel.DataAnnotations;

namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Configuration for the plugin system.
    /// </summary>
    public class PluginConfiguration
    {
        public const string SectionName = "Plugins";

        /// <summary>
        /// Directory where plugins are located.
        /// </summary>
        [Required]
        public string Directory { get; set; } = "plugins";

        /// <summary>
        /// Whether to automatically load plugins at startup.
        /// </summary>
        public bool AutoLoadAtStartup { get; set; } = true;

        /// <summary>
        /// Whether to enable hot reloading of plugins.
        /// </summary>
        public bool EnableHotReload { get; set; } = false;

        /// <summary>
        /// Maximum time to wait for plugin operations (in seconds).
        /// </summary>
        [Range(1, 300)]
        public int OperationTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to enable plugin sandboxing for security.
        /// </summary>
        public bool EnableSandboxing { get; set; } = true;

        /// <summary>
        /// Maximum memory usage per plugin in MB.
        /// </summary>
        [Range(1, 1000)]
        public int MaxMemoryUsageMB { get; set; } = 100;

        /// <summary>
        /// Whether to allow plugins to register Discord commands.
        /// </summary>
        public bool AllowCommandRegistration { get; set; } = true;

        /// <summary>
        /// Whether to allow plugins to register dice functions.
        /// </summary>
        public bool AllowFunctionRegistration { get; set; } = true;

        /// <summary>
        /// List of explicitly allowed plugin assemblies.
        /// </summary>
        public List<string> AllowedPlugins { get; set; } = new();

        /// <summary>
        /// List of explicitly blocked plugin assemblies.
        /// </summary>
        public List<string> BlockedPlugins { get; set; } = new();
    }
}