using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiceRoller.Plugins.Infrastructure.SDK.Abstractions
{
    /// <summary>
    /// Interface for accessing plugin context services and configuration
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        /// Plugin identifier
        /// </summary>
        string PluginId { get; }

        /// <summary>
        /// Plugin name
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        Version PluginVersion { get; }

        /// <summary>
        /// Host application information
        /// </summary>
        IHostInfo HostInfo { get; }

        /// <summary>
        /// Plugin working directory
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Plugin data directory for persistent storage
        /// </summary>
        string DataDirectory { get; }

        /// <summary>
        /// Plugin temporary directory
        /// </summary>
        string TempDirectory { get; }

        /// <summary>
        /// Get a service from the dependency injection container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance</returns>
        T GetService<T>() where T : notnull;

        /// <summary>
        /// Try to get a service from the dependency injection container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if not found</returns>
        T? GetServiceOrDefault<T>() where T : class;

        /// <summary>
        /// Create a logger for the plugin
        /// </summary>
        /// <param name="categoryName">Logger category name</param>
        /// <returns>Logger instance</returns>
        ILogger CreateLogger(string categoryName);

        /// <summary>
        /// Create a logger for a specific type
        /// </summary>
        /// <typeparam name="T">Type to create logger for</typeparam>
        /// <returns>Logger instance</returns>
        ILogger<T> CreateLogger<T>();

        /// <summary>
        /// Get plugin configuration
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <returns>Configuration object</returns>
        T? GetConfiguration<T>() where T : class;

        /// <summary>
        /// Get configuration value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Configuration value</returns>
        T? GetConfigurationValue<T>(string key, T? defaultValue = default);

        /// <summary>
        /// Get plugin configuration section
        /// </summary>
        /// <param name="sectionName">Section name</param>
        /// <returns>Configuration section</returns>
        IConfigurationSection GetConfigurationSection(string sectionName);

        /// <summary>
        /// Publish an event to other plugins
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="eventData">Event data</param>
        void PublishEvent<T>(T eventData) where T : class;

        /// <summary>
        /// Subscribe to events from other plugins
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler</param>
        void SubscribeToEvent<T>(Action<T> handler) where T : class;

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler</param>
        void UnsubscribeFromEvent<T>(Action<T> handler) where T : class;

        /// <summary>
        /// Check if the plugin has a specific permission
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if the plugin has the permission</returns>
        bool HasPermission(string permission);

        /// <summary>
        /// Get current resource usage for the plugin
        /// </summary>
        /// <returns>Resource usage information</returns>
        IResourceUsage GetResourceUsage();

        /// <summary>
        /// Record a performance metric
        /// </summary>
        /// <param name="metricName">Metric name</param>
        /// <param name="value">Metric value</param>
        /// <param name="tags">Optional tags</param>
        void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Start measuring an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>Disposable that will stop measurement when disposed</returns>
        IDisposable StartOperation(string operationName);
    }

    /// <summary>
    /// Interface for host application information
    /// </summary>
    public interface IHostInfo
    {
        /// <summary>
        /// Host application name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Host application version
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Host environment (Development, Staging, Production)
        /// </summary>
        string Environment { get; }

        /// <summary>
        /// Host application start time
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Additional host information
        /// </summary>
        IReadOnlyDictionary<string, object>? AdditionalInfo { get; }
    }

    /// <summary>
    /// Interface for plugin resource usage information
    /// </summary>
    public interface IResourceUsage
    {
        /// <summary>
        /// Memory usage in bytes
        /// </summary>
        long MemoryUsageBytes { get; }

        /// <summary>
        /// CPU usage percentage
        /// </summary>
        double CpuUsagePercent { get; }

        /// <summary>
        /// Disk usage in bytes
        /// </summary>
        long DiskUsageBytes { get; }

        /// <summary>
        /// Network usage in bytes
        /// </summary>
        long NetworkUsageBytes { get; }

        /// <summary>
        /// Total execution time
        /// </summary>
        TimeSpan ExecutionTime { get; }
    }

    /// <summary>
    /// Interface for plugin service registration
    /// </summary>
    public interface IPluginServiceRegistry
    {
        /// <summary>
        /// Register a service with the plugin's service collection
        /// </summary>
        /// <typeparam name="TService">Service interface type</typeparam>
        /// <typeparam name="TImplementation">Service implementation type</typeparam>
        void RegisterTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Register a singleton service with the plugin's service collection
        /// </summary>
        /// <typeparam name="TService">Service interface type</typeparam>
        /// <typeparam name="TImplementation">Service implementation type</typeparam>
        void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Register a scoped service with the plugin's service collection
        /// </summary>
        /// <typeparam name="TService">Service interface type</typeparam>
        /// <typeparam name="TImplementation">Service implementation type</typeparam>
        void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Register a singleton instance
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="instance">Service instance</param>
        void RegisterInstance<TService>(TService instance) where TService : class;
    }

    /// <summary>
    /// Interface for plugin events
    /// </summary>
    public interface IPluginEvent
    {
        /// <summary>
        /// Event timestamp
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Source plugin ID
        /// </summary>
        string SourcePluginId { get; }

        /// <summary>
        /// Event type name
        /// </summary>
        string EventType { get; }
    }

    /// <summary>
    /// Base class for plugin events
    /// </summary>
    public abstract class PluginEventBase : IPluginEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string SourcePluginId { get; }
        public string EventType { get; }

        protected PluginEventBase(string sourcePluginId, string eventType)
        {
            SourcePluginId = sourcePluginId ?? throw new ArgumentNullException(nameof(sourcePluginId));
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        }
    }
}