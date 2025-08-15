using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.Manager
{
    /// <summary>
    /// Provides security sandbox functionality for plugins
    /// </summary>
    public class PluginSandbox : IDisposable
    {
        private readonly ILogger<PluginSandbox> _logger;
        private readonly ConcurrentDictionary<string, PluginSandboxContext> _sandboxedPlugins = new();
        private readonly Timer _resourceMonitorTimer;
        private bool _disposed = false;

        public PluginSandbox(ILogger<PluginSandbox> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Start resource monitoring
            _resourceMonitorTimer = new Timer(MonitorResources, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Create a sandbox for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="metadata">Plugin metadata with security requirements</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sandbox context</returns>
        public async Task<PluginSandboxContext> CreateSandboxAsync(string pluginId, PluginMetadata metadata, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            try
            {
                _logger.LogDebug("Creating sandbox for plugin {PluginId}", pluginId);

                // Create sandbox directory structure
                var sandboxDirectory = CreateSandboxDirectory(pluginId);

                // Set up resource limits
                var resourceLimits = CreateResourceLimits(metadata);

                // Set up security permissions
                var permissions = CreateSecurityPermissions(metadata);

                // Create monitoring context
                var monitoringContext = new PluginMonitoringContext(pluginId, DateTime.UtcNow);

                var sandboxContext = new PluginSandboxContext(
                    pluginId,
                    sandboxDirectory,
                    resourceLimits,
                    permissions,
                    monitoringContext);

                // Add to tracked sandboxes
                _sandboxedPlugins[pluginId] = sandboxContext;

                _logger.LogInformation("Sandbox created for plugin {PluginId} at {SandboxDirectory}", 
                    pluginId, sandboxDirectory);

                await Task.CompletedTask;
                return sandboxContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sandbox for plugin {PluginId}", pluginId);
                throw;
            }
        }

        /// <summary>
        /// Destroy sandbox for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task DestroySandboxAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pluginId))
                return;

            try
            {
                _logger.LogDebug("Destroying sandbox for plugin {PluginId}", pluginId);

                if (_sandboxedPlugins.TryRemove(pluginId, out var context))
                {
                    // Clean up sandbox directory
                    await CleanupSandboxDirectoryAsync(context.SandboxDirectory, cancellationToken);

                    _logger.LogInformation("Sandbox destroyed for plugin {PluginId}", pluginId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to destroy sandbox for plugin {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// Check if a plugin operation is allowed within sandbox limits
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="operation">Operation to check</param>
        /// <returns>True if operation is allowed</returns>
        public bool IsOperationAllowed(string pluginId, PluginOperation operation)
        {
            if (!_sandboxedPlugins.TryGetValue(pluginId, out var context))
            {
                _logger.LogWarning("No sandbox context found for plugin {PluginId}", pluginId);
                return false;
            }

            return CheckOperationAgainstLimits(context, operation);
        }

        /// <summary>
        /// Get current resource usage for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <returns>Current resource usage</returns>
        public PluginResourceUsage? GetResourceUsage(string pluginId)
        {
            if (!_sandboxedPlugins.TryGetValue(pluginId, out var context))
                return null;

            return CalculateResourceUsage(context);
        }

        /// <summary>
        /// Unload a plugin from the sandbox
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            await DestroySandboxAsync(pluginId, cancellationToken);
        }

        private string CreateSandboxDirectory(string pluginId)
        {
            var baseDir = Path.Combine(Path.GetTempPath(), "diceroller-sandbox");
            var pluginDir = Path.Combine(baseDir, pluginId);

            // Create directory structure
            Directory.CreateDirectory(pluginDir);
            Directory.CreateDirectory(Path.Combine(pluginDir, "temp"));
            Directory.CreateDirectory(Path.Combine(pluginDir, "data"));
            Directory.CreateDirectory(Path.Combine(pluginDir, "logs"));

            // Set restricted permissions (Windows)
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    SetRestrictedPermissions(pluginDir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not set restricted permissions on sandbox directory {Directory}", pluginDir);
                }
            }

            return pluginDir;
        }

        private void SetRestrictedPermissions(string directory)
        {
            // This is a simplified implementation
            // In a production environment, you would use more sophisticated access control
            var dirInfo = new DirectoryInfo(directory);
            var security = dirInfo.GetAccessControl();
            
            // Remove inheritance and add specific permissions for the current user only
            security.SetAccessRuleProtection(true, false);
            
            dirInfo.SetAccessControl(security);
        }

        private PluginResourceLimits CreateResourceLimits(PluginMetadata metadata)
        {
            var requirements = metadata.ResourceRequirements;

            return new PluginResourceLimits(
                MaxMemoryBytes: requirements.MaxMemoryMB.HasValue ? requirements.MaxMemoryMB.Value * 1024 * 1024 : 100 * 1024 * 1024, // Default 100MB
                MaxCpuPercent: requirements.MaxCpuPercent ?? 25.0, // Default 25%
                MaxDiskBytes: requirements.MaxDiskMB.HasValue ? requirements.MaxDiskMB.Value * 1024 * 1024 : 50 * 1024 * 1024, // Default 50MB
                MaxNetworkBytes: requirements.MaxNetworkKBps.HasValue ? requirements.MaxNetworkKBps.Value * 1024 : 1024 * 1024, // Default 1MB/s
                MaxExecutionTime: requirements.ExecutionTimeoutSeconds.HasValue ? TimeSpan.FromSeconds(requirements.ExecutionTimeoutSeconds.Value) : TimeSpan.FromMinutes(5) // Default 5 minutes
            );
        }

        private PluginSecurityPermissions CreateSecurityPermissions(PluginMetadata metadata)
        {
            return new PluginSecurityPermissions(
                AllowedPermissions: metadata.Permissions,
                AllowFileSystemAccess: metadata.Permissions.Contains("filesystem"),
                AllowNetworkAccess: metadata.Permissions.Contains("network"),
                AllowRegistryAccess: false, // Generally not allowed for plugins
                AllowEnvironmentAccess: metadata.Permissions.Contains("environment"),
                AllowReflectionAccess: metadata.Permissions.Contains("reflection")
            );
        }

        private bool CheckOperationAgainstLimits(PluginSandboxContext context, PluginOperation operation)
        {
            var currentUsage = CalculateResourceUsage(context);
            
            switch (operation.Type)
            {
                case PluginOperationType.MemoryAllocation:
                    var requestedMemory = (long)(operation.Parameters.GetValueOrDefault("size", 0L));
                    return currentUsage.MemoryUsageBytes + requestedMemory <= context.ResourceLimits.MaxMemoryBytes;

                case PluginOperationType.FileSystemAccess:
                    return context.SecurityPermissions.AllowFileSystemAccess;

                case PluginOperationType.NetworkAccess:
                    return context.SecurityPermissions.AllowNetworkAccess;

                case PluginOperationType.LongRunningOperation:
                    var estimatedDuration = (TimeSpan)(operation.Parameters.GetValueOrDefault("duration", TimeSpan.Zero));
                    return currentUsage.ExecutionTime + estimatedDuration <= context.ResourceLimits.MaxExecutionTime;

                default:
                    return true; // Allow unknown operations by default
            }
        }

        private PluginResourceUsage CalculateResourceUsage(PluginSandboxContext context)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                
                // This is a simplified calculation
                // In a real implementation, you would track plugin-specific resource usage
                return new PluginResourceUsage(
                    MemoryUsageBytes: currentProcess.WorkingSet64,
                    CpuUsagePercent: 0.0, // Would need more sophisticated tracking
                    DiskUsageBytes: CalculateDirectorySize(context.SandboxDirectory),
                    NetworkUsageBytes: 0L, // Would need network monitoring
                    ExecutionTime: DateTime.UtcNow - context.MonitoringContext.StartTime
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating resource usage for plugin {PluginId}", context.PluginId);
                return new PluginResourceUsage(0, 0, 0, 0, TimeSpan.Zero);
            }
        }

        private long CalculateDirectorySize(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return 0;

                var dirInfo = new DirectoryInfo(directory);
                long size = 0;

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }

                return size;
            }
            catch
            {
                return 0;
            }
        }

        private async Task CleanupSandboxDirectoryAsync(string directory, CancellationToken cancellationToken)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    await Task.Run(() =>
                    {
                        Directory.Delete(directory, recursive: true);
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not clean up sandbox directory {Directory}", directory);
            }
        }

        private void MonitorResources(object? state)
        {
            try
            {
                foreach (var (pluginId, context) in _sandboxedPlugins)
                {
                    try
                    {
                        var usage = CalculateResourceUsage(context);
                        CheckResourceViolations(pluginId, context, usage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error monitoring resources for plugin {PluginId}", pluginId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resource monitoring");
            }
        }

        private void CheckResourceViolations(string pluginId, PluginSandboxContext context, PluginResourceUsage usage)
        {
            var limits = context.ResourceLimits;

            if (usage.MemoryUsageBytes > limits.MaxMemoryBytes)
            {
                _logger.LogWarning("Plugin {PluginId} exceeded memory limit: {UsedMB}MB > {LimitMB}MB",
                    pluginId, usage.MemoryUsageBytes / 1024 / 1024, limits.MaxMemoryBytes / 1024 / 1024);
            }

            if (usage.CpuUsagePercent > limits.MaxCpuPercent)
            {
                _logger.LogWarning("Plugin {PluginId} exceeded CPU limit: {UsedPercent}% > {LimitPercent}%",
                    pluginId, usage.CpuUsagePercent, limits.MaxCpuPercent);
            }

            if (usage.DiskUsageBytes > limits.MaxDiskBytes)
            {
                _logger.LogWarning("Plugin {PluginId} exceeded disk limit: {UsedMB}MB > {LimitMB}MB",
                    pluginId, usage.DiskUsageBytes / 1024 / 1024, limits.MaxDiskBytes / 1024 / 1024);
            }

            if (usage.ExecutionTime > limits.MaxExecutionTime)
            {
                _logger.LogWarning("Plugin {PluginId} exceeded execution time limit: {UsedTime} > {LimitTime}",
                    pluginId, usage.ExecutionTime, limits.MaxExecutionTime);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _resourceMonitorTimer?.Dispose();

            // Clean up all sandboxes
            foreach (var (pluginId, _) in _sandboxedPlugins)
            {
                try
                {
                    DestroySandboxAsync(pluginId).Wait(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up sandbox for plugin {PluginId}", pluginId);
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Sandbox context for a plugin
    /// </summary>
    public record PluginSandboxContext(
        string PluginId,
        string SandboxDirectory,
        PluginResourceLimits ResourceLimits,
        PluginSecurityPermissions SecurityPermissions,
        PluginMonitoringContext MonitoringContext);

    /// <summary>
    /// Security permissions for a plugin
    /// </summary>
    public record PluginSecurityPermissions(
        IReadOnlyList<string> AllowedPermissions,
        bool AllowFileSystemAccess,
        bool AllowNetworkAccess,
        bool AllowRegistryAccess,
        bool AllowEnvironmentAccess,
        bool AllowReflectionAccess);

    /// <summary>
    /// Monitoring context for a plugin
    /// </summary>
    public record PluginMonitoringContext(
        string PluginId,
        DateTime StartTime);

    /// <summary>
    /// Plugin operation for sandbox checking
    /// </summary>
    public record PluginOperation(
        PluginOperationType Type,
        Dictionary<string, object> Parameters);

    /// <summary>
    /// Types of plugin operations
    /// </summary>
    public enum PluginOperationType
    {
        MemoryAllocation,
        FileSystemAccess,
        NetworkAccess,
        RegistryAccess,
        EnvironmentAccess,
        ReflectionAccess,
        LongRunningOperation
    }
}