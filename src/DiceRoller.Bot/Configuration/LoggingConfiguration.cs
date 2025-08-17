using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Configuration for logging settings.
    /// </summary>
    public class LoggingConfiguration
    {
        public const string SectionName = "Logging";

        /// <summary>
        /// Minimum log level for the application.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Whether to enable console logging.
        /// </summary>
        public bool EnableConsole { get; set; } = true;

        /// <summary>
        /// Whether to enable file logging.
        /// </summary>
        public bool EnableFile { get; set; } = false;

        /// <summary>
        /// Path for log files (if file logging is enabled).
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Maximum size of log files in MB before rotation.
        /// </summary>
        [Range(1, 1000)]
        public int MaxFileSizeMB { get; set; } = 50;

        /// <summary>
        /// Number of archived log files to retain.
        /// </summary>
        [Range(1, 100)]
        public int RetainedFileCountLimit { get; set; } = 5;

        /// <summary>
        /// Specific log levels for different categories.
        /// </summary>
        public Dictionary<string, LogLevel> Categories { get; set; } = new()
        {
            ["DSharpPlus"] = LogLevel.Information,
            ["Microsoft"] = LogLevel.Warning,
            ["System"] = LogLevel.Warning
        };
    }
}