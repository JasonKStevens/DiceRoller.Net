namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Root configuration class that aggregates all configuration sections.
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Bot-specific configuration.
        /// </summary>
        public BotConfiguration Bot { get; set; } = new();

        /// <summary>
        /// Logging configuration.
        /// </summary>
        public LoggingConfiguration Logging { get; set; } = new();

        /// <summary>
        /// Plugin system configuration.
        /// </summary>
        public PluginConfiguration Plugins { get; set; } = new();

        /// <summary>
        /// Security and rate limiting configuration.
        /// </summary>
        public SecurityConfiguration Security { get; set; } = new();

        /// <summary>
        /// Application environment (Development, Production, Docker, etc.).
        /// </summary>
        public string Environment { get; set; } = "Production";

        /// <summary>
        /// Application version for logging and diagnostics.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Whether to enable detailed health checks.
        /// </summary>
        public bool EnableHealthChecks { get; set; } = true;

        /// <summary>
        /// Interval for health check runs in seconds.
        /// </summary>
        public int HealthCheckIntervalSeconds { get; set; } = 300;
    }
}