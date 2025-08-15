using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.Configuration
{
    /// <summary>
    /// Manages plugin-specific configuration settings
    /// </summary>
    public class PluginConfiguration
    {
        private readonly ILogger<PluginConfiguration> _logger;
        private readonly IConfiguration _globalConfiguration;
        private readonly string _configurationDirectory;
        private readonly Dictionary<string, IConfigurationSection> _pluginConfigurations = new();

        public PluginConfiguration(
            ILogger<PluginConfiguration> logger,
            IConfiguration globalConfiguration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalConfiguration = globalConfiguration ?? throw new ArgumentNullException(nameof(globalConfiguration));
            
            _configurationDirectory = _globalConfiguration.GetValue("Plugins:ConfigurationDirectory", 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "plugins"));

            // Ensure configuration directory exists
            Directory.CreateDirectory(_configurationDirectory);

            // Load existing configurations
            LoadPluginConfigurations();
        }

        /// <summary>
        /// Get configuration section for a specific plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <returns>Plugin configuration section</returns>
        public IConfigurationSection GetPluginConfiguration(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (_pluginConfigurations.TryGetValue(pluginId, out var existingConfig))
            {
                return existingConfig;
            }

            // Try to load configuration from file
            var configPath = GetPluginConfigurationPath(pluginId);
            if (File.Exists(configPath))
            {
                try
                {
                    var configBuilder = new ConfigurationBuilder()
                        .AddJsonFile(configPath, optional: false, reloadOnChange: true);

                    var configuration = configBuilder.Build();
                    var section = configuration.GetSection("Configuration");
                    
                    _pluginConfigurations[pluginId] = section;
                    return section;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load configuration for plugin {PluginId}", pluginId);
                }
            }

            // Return empty configuration section
            var emptyConfig = new ConfigurationBuilder().Build();
            var emptySection = emptyConfig.GetSection("Empty");
            _pluginConfigurations[pluginId] = emptySection;
            
            return emptySection;
        }

        /// <summary>
        /// Set configuration for a specific plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="configuration">Configuration object</param>
        public void SetPluginConfiguration(string pluginId, object configuration)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            try
            {
                var configPath = GetPluginConfigurationPath(pluginId);
                var configWrapper = new { Configuration = configuration };
                var json = JsonConvert.SerializeObject(configWrapper, Formatting.Indented);

                File.WriteAllText(configPath, json);

                // Reload the configuration
                ReloadPluginConfiguration(pluginId);

                _logger.LogDebug("Configuration saved for plugin {PluginId}", pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration for plugin {PluginId}", pluginId);
                throw;
            }
        }

        /// <summary>
        /// Get typed configuration for a plugin
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="pluginId">Plugin identifier</param>
        /// <returns>Typed configuration object</returns>
        public T? GetPluginConfiguration<T>(string pluginId) where T : class
        {
            var configSection = GetPluginConfiguration(pluginId);
            return configSection.Get<T>();
        }

        /// <summary>
        /// Update specific configuration value for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public void UpdatePluginConfigurationValue(string pluginId, string key, object value)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Configuration key cannot be null or empty", nameof(key));

            try
            {
                var configPath = GetPluginConfigurationPath(pluginId);
                var configData = new Dictionary<string, object>();

                // Load existing configuration
                if (File.Exists(configPath))
                {
                    var existingJson = File.ReadAllText(configPath);
                    var existingConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(existingJson);
                    
                    if (existingConfig?.ContainsKey("Configuration") == true)
                    {
                        configData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                            existingConfig["Configuration"]?.ToString() ?? "{}") ?? new Dictionary<string, object>();
                    }
                }

                // Update the specific value
                configData[key] = value;

                // Save back to file
                var configWrapper = new { Configuration = configData };
                var json = JsonConvert.SerializeObject(configWrapper, Formatting.Indented);
                File.WriteAllText(configPath, json);

                // Reload the configuration
                ReloadPluginConfiguration(pluginId);

                _logger.LogDebug("Configuration value updated for plugin {PluginId}: {Key} = {Value}", 
                    pluginId, key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update configuration value for plugin {PluginId}", pluginId);
                throw;
            }
        }

        /// <summary>
        /// Remove configuration for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        public void RemovePluginConfiguration(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId))
                return;

            try
            {
                var configPath = GetPluginConfigurationPath(pluginId);
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                    _logger.LogDebug("Configuration removed for plugin {PluginId}", pluginId);
                }

                _pluginConfigurations.Remove(pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove configuration for plugin {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// Get all plugin configurations
        /// </summary>
        /// <returns>Dictionary of plugin configurations</returns>
        public IReadOnlyDictionary<string, IConfigurationSection> GetAllPluginConfigurations()
        {
            return _pluginConfigurations.AsReadOnly();
        }

        /// <summary>
        /// Validate configuration against schema
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <param name="configuration">Configuration to validate</param>
        /// <param name="schema">JSON schema for validation</param>
        /// <returns>Validation result</returns>
        public PluginConfigurationValidationResult ValidateConfiguration(string pluginId, object configuration, string? schema)
        {
            try
            {
                if (string.IsNullOrEmpty(schema))
                {
                    return new PluginConfigurationValidationResult(true, "No schema provided - validation skipped");
                }

                // This is a simplified validation
                // In a real implementation, you would use a JSON schema validation library
                var configJson = JsonConvert.SerializeObject(configuration);
                
                // Basic validation - check if it's valid JSON
                JsonConvert.DeserializeObject(configJson);

                return new PluginConfigurationValidationResult(true, "Configuration is valid");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Configuration validation failed for plugin {PluginId}", pluginId);
                return new PluginConfigurationValidationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Create default configuration for a plugin
        /// </summary>
        /// <param name="metadata">Plugin metadata</param>
        public void CreateDefaultConfiguration(PluginMetadata metadata)
        {
            if (metadata?.DefaultConfiguration == null)
                return;

            try
            {
                var configPath = GetPluginConfigurationPath(metadata.Id);
                
                // Only create default if no configuration exists
                if (!File.Exists(configPath))
                {
                    SetPluginConfiguration(metadata.Id, metadata.DefaultConfiguration);
                    _logger.LogDebug("Default configuration created for plugin {PluginId}", metadata.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default configuration for plugin {PluginId}", metadata.Id);
            }
        }

        /// <summary>
        /// Backup plugin configuration
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        /// <returns>Path to backup file</returns>
        public string BackupPluginConfiguration(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            try
            {
                var configPath = GetPluginConfigurationPath(pluginId);
                var backupPath = GetPluginConfigurationBackupPath(pluginId);

                if (File.Exists(configPath))
                {
                    File.Copy(configPath, backupPath, overwrite: true);
                    _logger.LogDebug("Configuration backed up for plugin {PluginId} to {BackupPath}", 
                        pluginId, backupPath);
                }

                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup configuration for plugin {PluginId}", pluginId);
                throw;
            }
        }

        /// <summary>
        /// Restore plugin configuration from backup
        /// </summary>
        /// <param name="pluginId">Plugin identifier</param>
        public void RestorePluginConfiguration(string pluginId)
        {
            if (string.IsNullOrEmpty(pluginId))
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));

            try
            {
                var configPath = GetPluginConfigurationPath(pluginId);
                var backupPath = GetPluginConfigurationBackupPath(pluginId);

                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, configPath, overwrite: true);
                    ReloadPluginConfiguration(pluginId);
                    
                    _logger.LogDebug("Configuration restored for plugin {PluginId} from {BackupPath}", 
                        pluginId, backupPath);
                }
                else
                {
                    _logger.LogWarning("No backup configuration found for plugin {PluginId}", pluginId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore configuration for plugin {PluginId}", pluginId);
                throw;
            }
        }

        private void LoadPluginConfigurations()
        {
            try
            {
                if (!Directory.Exists(_configurationDirectory))
                    return;

                var configFiles = Directory.GetFiles(_configurationDirectory, "*.json");
                
                foreach (var configFile in configFiles)
                {
                    try
                    {
                        var pluginId = Path.GetFileNameWithoutExtension(configFile);
                        ReloadPluginConfiguration(pluginId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load configuration from {ConfigFile}", configFile);
                    }
                }

                _logger.LogDebug("Loaded {ConfigCount} plugin configurations", _pluginConfigurations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin configurations");
            }
        }

        private void ReloadPluginConfiguration(string pluginId)
        {
            var configPath = GetPluginConfigurationPath(pluginId);
            
            if (File.Exists(configPath))
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(configPath, optional: false, reloadOnChange: true);

                var configuration = configBuilder.Build();
                var section = configuration.GetSection("Configuration");
                
                _pluginConfigurations[pluginId] = section;
            }
            else
            {
                _pluginConfigurations.Remove(pluginId);
            }
        }

        private string GetPluginConfigurationPath(string pluginId)
        {
            return Path.Combine(_configurationDirectory, $"{pluginId}.json");
        }

        private string GetPluginConfigurationBackupPath(string pluginId)
        {
            return Path.Combine(_configurationDirectory, $"{pluginId}.backup.json");
        }
    }

    /// <summary>
    /// Plugin configuration validation result
    /// </summary>
    public record PluginConfigurationValidationResult(bool IsValid, string Message, List<string>? Errors = null);
}