using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.SDK.Helpers
{
    /// <summary>
    /// Base class for plugins providing common functionality
    /// </summary>
    public abstract class PluginBase : IPlugin, IDisposable
    {
        private readonly List<PluginDependency> _dependencies = new();
        private PluginContext? _context;
        private ILogger? _logger;
        private PluginStatus _status = PluginStatus.Unloaded;
        private bool _disposed = false;

        /// <summary>
        /// Unique identifier for the plugin
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Display name of the plugin
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Plugin author
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// Minimum required DiceRoller version
        /// </summary>
        public virtual Version MinimumHostVersion => new(1, 0, 0);

        /// <summary>
        /// Plugin dependencies
        /// </summary>
        public IReadOnlyList<PluginDependency> Dependencies => _dependencies.AsReadOnly();

        /// <summary>
        /// Plugin configuration schema
        /// </summary>
        public virtual string? ConfigurationSchema => null;

        /// <summary>
        /// Plugin context (available after initialization)
        /// </summary>
        protected PluginContext Context => _context ?? throw new InvalidOperationException("Plugin not initialized");

        /// <summary>
        /// Plugin logger (available after initialization)
        /// </summary>
        protected ILogger Logger => _logger ?? throw new InvalidOperationException("Plugin not initialized");

        /// <summary>
        /// Current plugin status
        /// </summary>
        protected PluginStatus Status
        {
            get => _status;
            set
            {
                var oldStatus = _status;
                _status = value;
                if (oldStatus != value)
                {
                    StatusChanged?.Invoke(this, new PluginStatusChangedEventArgs(oldStatus, value));
                }
            }
        }

        /// <summary>
        /// Event raised when the plugin encounters an error
        /// </summary>
        public event EventHandler<PluginErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Event raised when plugin status changes
        /// </summary>
        public event EventHandler<PluginStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Initialize the plugin with the provided context
        /// </summary>
        public virtual async Task InitializeAsync(PluginContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Status = PluginStatus.Initializing;

            try
            {
                _context = context;
                _logger = context.CreateLogger(GetType().Name);

                Logger.LogInformation("Initializing plugin {PluginName} v{Version}", Name, Version);

                // Call derived class initialization
                await OnInitializeAsync(cancellationToken);

                Status = PluginStatus.Initialized;
                Logger.LogInformation("Plugin {PluginName} initialized successfully", Name);
            }
            catch (Exception ex)
            {
                Status = PluginStatus.Error;
                var errorMessage = $"Failed to initialize plugin {Name}";
                Logger.LogError(ex, errorMessage);
                OnError(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Start the plugin
        /// </summary>
        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Status = PluginStatus.Starting;

            try
            {
                Logger.LogInformation("Starting plugin {PluginName}", Name);

                // Call derived class start logic
                await OnStartAsync(cancellationToken);

                Status = PluginStatus.Running;
                Logger.LogInformation("Plugin {PluginName} started successfully", Name);
            }
            catch (Exception ex)
            {
                Status = PluginStatus.Error;
                var errorMessage = $"Failed to start plugin {Name}";
                Logger.LogError(ex, errorMessage);
                OnError(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Stop the plugin
        /// </summary>
        public virtual async Task StopAsync(CancellationToken cancellationToken = default)
        {
            Status = PluginStatus.Stopping;

            try
            {
                Logger.LogInformation("Stopping plugin {PluginName}", Name);

                // Call derived class stop logic
                await OnStopAsync(cancellationToken);

                Status = PluginStatus.Stopped;
                Logger.LogInformation("Plugin {PluginName} stopped successfully", Name);
            }
            catch (Exception ex)
            {
                Status = PluginStatus.Error;
                var errorMessage = $"Failed to stop plugin {Name}";
                Logger.LogError(ex, errorMessage);
                OnError(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Get the current health status of the plugin
        /// </summary>
        public virtual PluginHealthStatus GetHealthStatus()
        {
            try
            {
                var isHealthy = Status == PluginStatus.Running;
                var statusText = Status.ToString();

                // Call derived class health check
                var customHealth = OnGetHealthStatus();
                if (customHealth != null)
                {
                    return customHealth;
                }

                return new PluginHealthStatus(
                    isHealthy,
                    statusText,
                    null,
                    new Dictionary<string, object>
                    {
                        ["status"] = Status,
                        ["version"] = Version.ToString()
                    });
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error getting health status for plugin {PluginName}", Name);
                return new PluginHealthStatus(
                    false,
                    "Health check failed",
                    null,
                    new Dictionary<string, object>
                    {
                        ["error"] = ex.Message
                    });
            }
        }

        /// <summary>
        /// Handle configuration changes
        /// </summary>
        public virtual async Task OnConfigurationChangedAsync(object configuration)
        {
            try
            {
                Logger.LogDebug("Configuration changed for plugin {PluginName}", Name);
                await OnConfigurationUpdatedAsync(configuration);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error handling configuration change for plugin {Name}";
                Logger.LogError(ex, errorMessage);
                OnError(ex, errorMessage);
            }
        }

        /// <summary>
        /// Add a dependency to this plugin
        /// </summary>
        protected void AddDependency(string pluginId, Version minimumVersion, bool isOptional = false)
        {
            _dependencies.Add(new PluginDependency(pluginId, minimumVersion, isOptional));
        }

        /// <summary>
        /// Get a service from the plugin context
        /// </summary>
        protected T GetService<T>() where T : notnull
        {
            return Context.GetService<T>();
        }

        /// <summary>
        /// Try to get a service from the plugin context
        /// </summary>
        protected T? GetServiceOrDefault<T>() where T : class
        {
            return Context.GetServiceOrDefault<T>();
        }

        /// <summary>
        /// Get plugin configuration
        /// </summary>
        protected T? GetConfiguration<T>() where T : class
        {
            return Context.PluginConfiguration.Get<T>();
        }

        /// <summary>
        /// Get configuration value
        /// </summary>
        protected T? GetConfigurationValue<T>(string key, T? defaultValue = default)
        {
            return Context.PluginConfiguration.GetValue(key, defaultValue);
        }

        /// <summary>
        /// Raise an error event
        /// </summary>
        protected void OnError(Exception exception, string message)
        {
            ErrorOccurred?.Invoke(this, new PluginErrorEventArgs(exception, message));
        }

        #region Abstract/Virtual Methods for Derived Classes

        /// <summary>
        /// Called during plugin initialization. Override to perform custom initialization logic.
        /// </summary>
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Called when plugin is started. Override to perform custom start logic.
        /// </summary>
        protected virtual Task OnStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Called when plugin is stopped. Override to perform custom stop logic.
        /// </summary>
        protected virtual Task OnStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Called when configuration is updated. Override to handle configuration changes.
        /// </summary>
        protected virtual Task OnConfigurationUpdatedAsync(object configuration) => Task.CompletedTask;

        /// <summary>
        /// Called during health status check. Override to provide custom health information.
        /// </summary>
        protected virtual PluginHealthStatus? OnGetHealthStatus() => null;

        /// <summary>
        /// Called during disposal. Override to perform custom cleanup.
        /// </summary>
        protected virtual void OnDispose() { }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    // Stop the plugin if it's running
                    if (Status == PluginStatus.Running)
                    {
                        StopAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(10));
                    }

                    // Call derived class disposal
                    OnDispose();

                    Status = PluginStatus.Unloaded;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error disposing plugin {PluginName}", Name);
                }
            }

            _disposed = true;
        }

        #endregion
    }
}