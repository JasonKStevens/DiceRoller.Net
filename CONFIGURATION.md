# Configuration System

DiceRoller.Bot features a comprehensive configuration system with environment-specific settings, strong typing, validation, and flexible configuration sources. This guide explains how to configure and customize the bot for your deployment needs.

## Overview

The configuration system provides:

- **Multi-environment support** (Development, Production, Docker)
- **Strong typing** with data annotation validation
- **Flexible configuration sources** with proper precedence order
- **Environment variable support** with both new and legacy formats
- **Options pattern** with `IOptions<T>` dependency injection
- **Backward compatibility** with existing configuration

## Configuration Structure

### Configuration Sections

The application configuration is organized into logical sections:

```json
{
  "Bot": {
    "Token": "your-discord-bot-token",
    "CommandPrefix": "!roll",
    "EnableSlashCommands": true,
    "EnablePrefixCommands": false,
    "EnableButtonInteractions": true,
    "DiscordTimeoutSeconds": 30,
    "UseGlobalCommands": true,
    "TestGuildId": null
  },
  "Logging": {
    "MinimumLevel": "Information",
    "EnableConsole": true,
    "EnableFile": false,
    "LogFilePath": null,
    "MaxFileSizeMB": 50,
    "RetainedFileCountLimit": 5,
    "Categories": {
      "DSharpPlus": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Plugins": {
    "Directory": "plugins",
    "AutoLoadAtStartup": true,
    "EnableHotReload": false,
    "OperationTimeoutSeconds": 30,
    "EnableSandboxing": true,
    "MaxMemoryUsageMB": 100,
    "AllowCommandRegistration": true,
    "AllowFunctionRegistration": true,
    "AllowedPlugins": [],
    "BlockedPlugins": []
  },
  "Security": {
    "MaxMessageLength": 2000,
    "EnableRateLimiting": true,
    "CommandsPerMinute": 60,
    "MaxExpressionComplexity": 5,
    "MaxDiceCount": 1000,
    "MaxDieSides": 100000,
    "LogSecurityViolations": true,
    "AutoBanViolators": false,
    "ViolationsBeforeBan": 10
  },
  "Environment": "Production",
  "Version": "1.0.0",
  "EnableHealthChecks": true,
  "HealthCheckIntervalSeconds": 300
}
```

## Configuration Files

### Environment-Specific Files

The bot loads configuration from multiple files in priority order:

1. **`appsettings.json`** - Base configuration for all environments
2. **`appsettings.{Environment}.json`** - Environment-specific overrides
3. **Environment variables** - Runtime configuration
4. **Command line arguments** - Highest priority overrides

#### Available Environment Files

- **`appsettings.Development.json`** - Development settings with verbose logging
- **`appsettings.Production.json`** - Production settings with file logging
- **`appsettings.Docker.json`** - Docker deployment settings with reduced limits

### Environment Detection

The environment is determined from these variables (in order):

1. `ASPNETCORE_ENVIRONMENT`
2. `DOTNET_ENVIRONMENT`
3. `DICEROLLER_ENVIRONMENT`
4. Defaults to `"Production"` if none are set

## Environment Variables

### New Format (Recommended)

Use the `DICEROLLER_` prefix with double underscore section separators:

```bash
# Bot configuration
DICEROLLER_Bot__Token=your-discord-bot-token
DICEROLLER_Bot__CommandPrefix=!dice
DICEROLLER_Bot__UseGlobalCommands=false
DICEROLLER_Bot__TestGuildId=123456789012345678

# Logging configuration
DICEROLLER_Logging__MinimumLevel=Debug
DICEROLLER_Logging__EnableFile=true
DICEROLLER_Logging__LogFilePath=/app/logs/diceroller.log

# Plugin configuration
DICEROLLER_Plugins__Directory=/app/plugins
DICEROLLER_Plugins__EnableHotReload=true
DICEROLLER_Plugins__EnableSandboxing=false

# Security configuration
DICEROLLER_Security__EnableRateLimiting=false
DICEROLLER_Security__MaxDiceCount=5000
DICEROLLER_Security__CommandsPerMinute=120

# Application configuration
DICEROLLER_Environment=Development
DICEROLLER_Version=2.0.0
DICEROLLER_EnableHealthChecks=false
```

### Legacy Format (Backward Compatible)

The old `BotConfig_` format is still supported:

```bash
# Legacy bot configuration (still works)
BotConfig_Token=your-discord-bot-token
BotConfig_CommandPrefix=!roll
```

### Configuration Precedence

Configuration values are applied in this order (later values override earlier ones):

1. **appsettings.json** (base configuration)
2. **appsettings.{Environment}.json** (environment-specific)
3. **Environment variables** (DICEROLLER_ prefix)
4. **Environment variables** (BotConfig_ prefix, legacy)
5. **Environment variables** (all others)
6. **Command line arguments** (highest priority)

## Configuration Sections Detail

### Bot Configuration

Controls Discord bot connection and basic behavior:

```json
{
  "Bot": {
    "Token": "your-discord-bot-token",           // Required: Discord bot token
    "CommandPrefix": "!roll",                    // Legacy text command prefix
    "EnableSlashCommands": true,                 // Enable slash commands (recommended)
    "EnablePrefixCommands": false,               // Enable text commands (deprecated)
    "EnableButtonInteractions": true,            // Enable interactive buttons
    "DiscordTimeoutSeconds": 30,                // Discord API timeout
    "UseGlobalCommands": true,                   // Global vs guild-specific commands
    "TestGuildId": null                          // Guild ID for testing (when UseGlobalCommands=false)
  }
}
```

**Environment Variables:**
```bash
DICEROLLER_Bot__Token=your-token
DICEROLLER_Bot__EnableSlashCommands=true
DICEROLLER_Bot__UseGlobalCommands=false
DICEROLLER_Bot__TestGuildId=123456789012345678
```

### Logging Configuration

Controls application logging behavior:

```json
{
  "Logging": {
    "MinimumLevel": "Information",               // Minimum log level
    "EnableConsole": true,                       // Enable console output
    "EnableFile": false,                         // Enable file logging
    "LogFilePath": null,                         // Log file path (if file logging enabled)
    "MaxFileSizeMB": 50,                        // Maximum log file size before rotation
    "RetainedFileCountLimit": 5,                // Number of archived log files to keep
    "Categories": {                             // Category-specific log levels
      "DSharpPlus": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
      "DiceRoller": "Information"
    }
  }
}
```

**Log Levels:** `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

**Environment Variables:**
```bash
DICEROLLER_Logging__MinimumLevel=Debug
DICEROLLER_Logging__EnableFile=true
DICEROLLER_Logging__LogFilePath=/app/logs/diceroller.log
```

### Plugin Configuration

Controls the plugin system behavior:

```json
{
  "Plugins": {
    "Directory": "plugins",                      // Plugin directory path
    "AutoLoadAtStartup": true,                   // Load plugins at startup
    "EnableHotReload": false,                    // Enable plugin hot reloading
    "OperationTimeoutSeconds": 30,              // Plugin operation timeout
    "EnableSandboxing": true,                    // Enable plugin security sandboxing
    "MaxMemoryUsageMB": 100,                    // Maximum memory per plugin
    "AllowCommandRegistration": true,            // Allow plugins to register commands
    "AllowFunctionRegistration": true,           // Allow plugins to register functions
    "AllowedPlugins": [],                       // Explicitly allowed plugins
    "BlockedPlugins": []                        // Explicitly blocked plugins
  }
}
```

**Environment Variables:**
```bash
DICEROLLER_Plugins__Directory=/app/plugins
DICEROLLER_Plugins__EnableHotReload=true
DICEROLLER_Plugins__EnableSandboxing=false
```

### Security Configuration

Controls security and rate limiting:

```json
{
  "Security": {
    "MaxMessageLength": 2000,                   // Maximum Discord message length
    "EnableRateLimiting": true,                 // Enable rate limiting
    "CommandsPerMinute": 60,                    // Commands per user per minute
    "MaxExpressionComplexity": 5,               // Maximum dice expression complexity (1-5)
    "MaxDiceCount": 1000,                       // Maximum dice in single expression
    "MaxDieSides": 100000,                      // Maximum sides on a die
    "LogSecurityViolations": true,              // Log security violations
    "AutoBanViolators": false,                  // Automatically ban violators
    "ViolationsBeforeBan": 10                   // Violations before auto-ban
  }
}
```

**Environment Variables:**
```bash
DICEROLLER_Security__EnableRateLimiting=false
DICEROLLER_Security__MaxDiceCount=5000
DICEROLLER_Security__CommandsPerMinute=120
```

## Common Configuration Scenarios

### Development Environment

```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Development

# Configure for development
export DICEROLLER_Bot__Token=your-dev-bot-token
export DICEROLLER_Bot__UseGlobalCommands=false
export DICEROLLER_Bot__TestGuildId=your-test-guild-id
export DICEROLLER_Logging__MinimumLevel=Debug
export DICEROLLER_Security__EnableRateLimiting=false
export DICEROLLER_Plugins__EnableHotReload=true
```

### Production Environment

```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Production

# Configure for production
export DICEROLLER_Bot__Token=your-production-bot-token
export DICEROLLER_Logging__EnableFile=true
export DICEROLLER_Logging__LogFilePath=/app/logs/diceroller.log
export DICEROLLER_Security__EnableRateLimiting=true
export DICEROLLER_Plugins__EnableSandboxing=true
```

### Docker Deployment

```bash
# Use Docker-specific settings
export ASPNETCORE_ENVIRONMENT=Docker

# Override for container environment
export DICEROLLER_Bot__Token=your-docker-bot-token
export DICEROLLER_Plugins__Directory=/app/plugins
export DICEROLLER_Logging__LogFilePath=/app/logs/diceroller.log
export DICEROLLER_Security__MaxDiceCount=500
export DICEROLLER_Security__CommandsPerMinute=30
```

### Command Line Configuration

```bash
# Override via command line (highest priority)
dotnet run --project src/DiceRoller.Bot \
  --Bot:Token=your-token \
  --Logging:MinimumLevel=Debug \
  --Security:EnableRateLimiting=false
```

## Migration Guide

### From Legacy Configuration

If you're upgrading from the old configuration system:

#### Old Format (appsettings.json)
```json
{
  "BotConfig": {
    "Token": "your-token",
    "CommandPrefix": "!roll"
  }
}
```

#### New Format (appsettings.json)
```json
{
  "Bot": {
    "Token": "your-token",
    "CommandPrefix": "!roll",
    "EnableSlashCommands": true,
    "EnablePrefixCommands": false,
    "EnableButtonInteractions": true,
    "DiscordTimeoutSeconds": 30,
    "UseGlobalCommands": true,
    "TestGuildId": null
  }
}
```

#### Legacy Environment Variables (still supported)
```bash
# Old format (still works)
BotConfig_Token=your-token
BotConfig_CommandPrefix=!roll

# New format (recommended)
DICEROLLER_Bot__Token=your-token
DICEROLLER_Bot__CommandPrefix=!roll
```

### Migration Steps

1. **Update appsettings.json**: Restructure configuration using new section format
2. **Update environment variables**: Use new `DICEROLLER_` prefix format
3. **Add new sections**: Configure logging, plugins, and security sections
4. **Test thoroughly**: Verify all settings work as expected
5. **Remove legacy**: Eventually remove old `BotConfig` section (optional, maintained for compatibility)

## Configuration Validation

The configuration system includes comprehensive validation:

### Data Annotations

Configuration classes use data annotations for validation:

```csharp
public class BotConfiguration
{
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Range(1, 300)]
    public int DiscordTimeoutSeconds { get; set; } = 30;
}

public class SecurityConfiguration
{
    [Range(100, 2000)]
    public int MaxMessageLength { get; set; } = 2000;
    
    [Range(1, 1000)]
    public int CommandsPerMinute { get; set; } = 60;
}
```

### Runtime Validation

Configuration is validated at startup:

```bash
# Validation success
Configuration validation passed.

# Validation failure
Configuration validation failed: The Token field is required.
  - The field Token is required.
  - The field MaxDiceCount must be between 1 and 10000.
```

### Common Validation Errors

- **Missing required fields**: Bot.Token is required
- **Invalid ranges**: Security values must be within specified ranges
- **Invalid paths**: Plugin directory must exist or be creatable
- **Invalid log levels**: Must be valid LogLevel enum values

## Options Pattern Integration

The configuration system integrates with .NET's Options pattern:

### Dependency Injection

```csharp
// Register configuration with validation
services.AddOptions<BotConfiguration>()
    .Bind(configuration.GetSection(BotConfiguration.SectionName))
    .ValidateDataAnnotations();

// Inject via IOptions<T>
public class SomeService
{
    private readonly BotConfiguration _botConfig;
    
    public SomeService(IOptions<BotConfiguration> botConfig)
    {
        _botConfig = botConfig.Value;
    }
}

// Or inject directly (backward compatibility)
public class AnotherService
{
    public AnotherService(BotConfiguration botConfig)
    {
        // Configuration object injected directly
    }
}
```

### Configuration Change Monitoring

```csharp
// Monitor configuration changes (if reloadOnChange: true)
public class ConfigurableService
{
    private readonly IOptionsMonitor<BotConfiguration> _botConfig;
    
    public ConfigurableService(IOptionsMonitor<BotConfiguration> botConfig)
    {
        _botConfig = botConfig;
        _botConfig.OnChange(OnConfigurationChanged);
    }
    
    private void OnConfigurationChanged(BotConfiguration newConfig)
    {
        // React to configuration changes
    }
}
```

## Troubleshooting

### Common Issues

1. **Token not found**: Ensure Bot.Token is set via configuration or environment variable
2. **Invalid environment**: Check ASPNETCORE_ENVIRONMENT variable is set correctly
3. **Plugin directory not found**: Ensure Plugins.Directory path exists
4. **Validation errors**: Check console output for specific validation failures
5. **Environment variables not working**: Ensure correct format with double underscores

### Debug Configuration Loading

The application logs configuration loading details:

```bash
Looking for appsettings.json in: /app
Environment: Development
=== Configuration Summary ===
Environment: Development
Version: 1.0.0
Health Checks: True
Plugin Directory: plugins
Rate Limiting: False
```

### Verify Configuration

Use the `/plugin info` command in Discord to verify configuration is loaded correctly, or check the startup logs for configuration summary information.