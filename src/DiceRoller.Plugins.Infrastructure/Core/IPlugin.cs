using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Base interface for all DiceRoller plugins
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Unique identifier for the plugin
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Plugin author
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Minimum required DiceRoller version
        /// </summary>
        Version MinimumHostVersion { get; }

        /// <summary>
        /// Plugin dependencies
        /// </summary>
        IReadOnlyList<PluginDependency> Dependencies { get; }

        /// <summary>
        /// Plugin configuration schema
        /// </summary>
        string? ConfigurationSchema { get; }

        /// <summary>
        /// Initialize the plugin with the provided context
        /// </summary>
        /// <param name="context">Plugin context containing services and configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the initialization operation</returns>
        Task InitializeAsync(PluginContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Start the plugin
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the start operation</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop the plugin
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the stop operation</returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the current health status of the plugin
        /// </summary>
        /// <returns>Plugin health information</returns>
        PluginHealthStatus GetHealthStatus();

        /// <summary>
        /// Handle configuration changes
        /// </summary>
        /// <param name="configuration">New configuration</param>
        /// <returns>Task representing the configuration update</returns>
        Task OnConfigurationChangedAsync(object configuration);

        /// <summary>
        /// Event raised when the plugin encounters an error
        /// </summary>
        event EventHandler<PluginErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Event raised when plugin status changes
        /// </summary>
        event EventHandler<PluginStatusChangedEventArgs>? StatusChanged;
    }

    /// <summary>
    /// Plugin dependency information
    /// </summary>
    public record PluginDependency(string PluginId, Version MinimumVersion, bool IsOptional = false);

    /// <summary>
    /// Plugin health status
    /// </summary>
    public record PluginHealthStatus(
        bool IsHealthy,
        string Status,
        TimeSpan? ResponseTime = null,
        Dictionary<string, object>? AdditionalData = null);

    /// <summary>
    /// Plugin error event arguments
    /// </summary>
    public class PluginErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public PluginErrorEventArgs(Exception exception, string message)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Plugin status changed event arguments
    /// </summary>
    public class PluginStatusChangedEventArgs : EventArgs
    {
        public PluginStatus PreviousStatus { get; }
        public PluginStatus NewStatus { get; }
        public DateTime Timestamp { get; }

        public PluginStatusChangedEventArgs(PluginStatus previousStatus, PluginStatus newStatus)
        {
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Plugin status enumeration
    /// </summary>
    public enum PluginStatus
    {
        Unloaded,
        Loading,
        Loaded,
        Initializing,
        Initialized,
        Starting,
        Running,
        Stopping,
        Stopped,
        Error,
        Disabled
    }
}