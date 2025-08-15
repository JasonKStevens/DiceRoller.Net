using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Context provided to plugins during initialization and operation
    /// </summary>
    public class PluginContext
    {
        /// <summary>
        /// Service provider for dependency injection
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Logger factory for creating plugin loggers
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Configuration for the plugin
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Plugin-specific configuration section
        /// </summary>
        public IConfigurationSection PluginConfiguration { get; }

        /// <summary>
        /// Host application information
        /// </summary>
        public HostApplicationInfo HostInfo { get; }

        /// <summary>
        /// Plugin metadata
        /// </summary>
        public PluginMetadata Metadata { get; }

        /// <summary>
        /// Plugin working directory
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Plugin data directory for persistent storage
        /// </summary>
        public string DataDirectory { get; }

        /// <summary>
        /// Plugin temporary directory
        /// </summary>
        public string TempDirectory { get; }

        /// <summary>
        /// Event aggregator for plugin communication
        /// </summary>
        public IPluginEventAggregator EventAggregator { get; }

        /// <summary>
        /// Resource manager for plugin resources
        /// </summary>
        public IPluginResourceManager ResourceManager { get; }

        /// <summary>
        /// Security context for the plugin
        /// </summary>
        public PluginSecurityContext SecurityContext { get; }

        /// <summary>
        /// Performance monitor for the plugin
        /// </summary>
        public IPluginPerformanceMonitor PerformanceMonitor { get; }

        /// <summary>
        /// Additional context data
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; }

        public PluginContext(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IConfigurationSection pluginConfiguration,
            HostApplicationInfo hostInfo,
            PluginMetadata metadata,
            string workingDirectory,
            string dataDirectory,
            string tempDirectory,
            IPluginEventAggregator eventAggregator,
            IPluginResourceManager resourceManager,
            PluginSecurityContext securityContext,
            IPluginPerformanceMonitor performanceMonitor)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            PluginConfiguration = pluginConfiguration ?? throw new ArgumentNullException(nameof(pluginConfiguration));
            HostInfo = hostInfo ?? throw new ArgumentNullException(nameof(hostInfo));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            DataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
            TempDirectory = tempDirectory ?? throw new ArgumentNullException(nameof(tempDirectory));
            EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            SecurityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
            PerformanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
            AdditionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Get a service from the service provider
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance</returns>
        public T GetService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Try to get a service from the service provider
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if not found</returns>
        public T? GetServiceOrDefault<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// Create a logger for the plugin
        /// </summary>
        /// <param name="categoryName">Logger category name</param>
        /// <returns>Logger instance</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return LoggerFactory.CreateLogger($"Plugin.{Metadata.Id}.{categoryName}");
        }

        /// <summary>
        /// Create a logger for a specific type
        /// </summary>
        /// <typeparam name="T">Type to create logger for</typeparam>
        /// <returns>Logger instance</returns>
        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }

    /// <summary>
    /// Host application information
    /// </summary>
    public record HostApplicationInfo(
        string Name,
        Version Version,
        string Environment,
        DateTime StartTime,
        Dictionary<string, object>? AdditionalInfo = null);

    /// <summary>
    /// Plugin security context
    /// </summary>
    public class PluginSecurityContext
    {
        /// <summary>
        /// Permissions granted to the plugin
        /// </summary>
        public IReadOnlyList<string> GrantedPermissions { get; }

        /// <summary>
        /// Whether the plugin runs in a sandbox
        /// </summary>
        public bool IsSandboxed { get; }

        /// <summary>
        /// Trust level of the plugin
        /// </summary>
        public PluginTrustLevel TrustLevel { get; }

        /// <summary>
        /// Security restrictions applied to the plugin
        /// </summary>
        public IReadOnlyList<string> Restrictions { get; }

        public PluginSecurityContext(
            IReadOnlyList<string> grantedPermissions,
            bool isSandboxed,
            PluginTrustLevel trustLevel,
            IReadOnlyList<string> restrictions)
        {
            GrantedPermissions = grantedPermissions ?? throw new ArgumentNullException(nameof(grantedPermissions));
            IsSandboxed = isSandboxed;
            TrustLevel = trustLevel;
            Restrictions = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
        }

        /// <summary>
        /// Check if the plugin has a specific permission
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if the plugin has the permission</returns>
        public bool HasPermission(string permission)
        {
            return GrantedPermissions.Contains(permission);
        }
    }

    /// <summary>
    /// Plugin trust level
    /// </summary>
    public enum PluginTrustLevel
    {
        Untrusted,
        LimitedTrust,
        Trusted,
        FullyTrusted
    }

    /// <summary>
    /// Plugin event aggregator interface
    /// </summary>
    public interface IPluginEventAggregator
    {
        /// <summary>
        /// Publish an event
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="eventData">Event data</param>
        void Publish<T>(T eventData) where T : class;

        /// <summary>
        /// Subscribe to events of a specific type
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler</param>
        void Subscribe<T>(Action<T> handler) where T : class;

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="handler">Event handler</param>
        void Unsubscribe<T>(Action<T> handler) where T : class;
    }

    /// <summary>
    /// Plugin resource manager interface
    /// </summary>
    public interface IPluginResourceManager
    {
        /// <summary>
        /// Get current resource usage
        /// </summary>
        /// <returns>Resource usage information</returns>
        PluginResourceUsage GetResourceUsage();

        /// <summary>
        /// Set resource limits for the plugin
        /// </summary>
        /// <param name="limits">Resource limits</param>
        void SetResourceLimits(PluginResourceLimits limits);

        /// <summary>
        /// Check if operation would exceed resource limits
        /// </summary>
        /// <param name="operation">Operation description</param>
        /// <returns>True if operation is allowed</returns>
        bool CanPerformOperation(string operation);
    }

    /// <summary>
    /// Plugin resource usage information
    /// </summary>
    public record PluginResourceUsage(
        long MemoryUsageBytes,
        double CpuUsagePercent,
        long DiskUsageBytes,
        long NetworkUsageBytes,
        TimeSpan ExecutionTime);

    /// <summary>
    /// Plugin resource limits
    /// </summary>
    public record PluginResourceLimits(
        long? MaxMemoryBytes = null,
        double? MaxCpuPercent = null,
        long? MaxDiskBytes = null,
        long? MaxNetworkBytes = null,
        TimeSpan? MaxExecutionTime = null);

    /// <summary>
    /// Plugin performance monitor interface
    /// </summary>
    public interface IPluginPerformanceMonitor
    {
        /// <summary>
        /// Start measuring an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>Disposable that will stop measurement when disposed</returns>
        IDisposable StartOperation(string operationName);

        /// <summary>
        /// Record a metric value
        /// </summary>
        /// <param name="metricName">Metric name</param>
        /// <param name="value">Metric value</param>
        /// <param name="tags">Optional tags</param>
        void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Get performance statistics
        /// </summary>
        /// <returns>Performance statistics</returns>
        PluginPerformanceStats GetStats();
    }

    /// <summary>
    /// Plugin performance statistics
    /// </summary>
    public record PluginPerformanceStats(
        Dictionary<string, OperationStats> OperationStats,
        Dictionary<string, MetricStats> MetricStats,
        TimeSpan TotalRuntime,
        DateTime LastActivity);

    /// <summary>
    /// Operation performance statistics
    /// </summary>
    public record OperationStats(
        string Name,
        long Count,
        TimeSpan TotalTime,
        TimeSpan AverageTime,
        TimeSpan MinTime,
        TimeSpan MaxTime);

    /// <summary>
    /// Metric statistics
    /// </summary>
    public record MetricStats(
        string Name,
        long Count,
        double Sum,
        double Average,
        double Min,
        double Max);
}