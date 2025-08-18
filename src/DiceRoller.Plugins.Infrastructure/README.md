# DiceRoller.Net Plugin Infrastructure

The core infrastructure that powers DiceRoller.Net's extensible plugin system. This library provides the foundation for creating, loading, managing, and executing game system plugins.

## 🏗️ Architecture Overview

The plugin infrastructure is designed around several key principles:

- **Plugin Isolation**: Each plugin runs in its own context with managed resources
- **Lifecycle Management**: Complete control over plugin initialization, startup, and shutdown
- **Type Safety**: Strongly-typed interfaces and robust error handling
- **Performance Monitoring**: Built-in health checks and performance tracking
- **Security**: Sandboxed execution with controlled access to system resources

## 📦 Core Components

### Plugin Management
- **`PluginManager`** - Central orchestrator for all plugin operations
- **`PluginLoader`** - Handles assembly loading and plugin instantiation
- **`PluginDiscovery`** - Discovers and validates plugins from directories
- **`PluginSandbox`** - Provides isolated execution environments

### Plugin Base Classes
- **`PluginBase`** - Abstract base class with common functionality
- **`IPlugin`** - Core plugin interface defining lifecycle and contracts
- **`IGameSystemPlugin`** - Specialized interface for game system plugins

### Support Infrastructure
- **`PluginContext`** - Provides plugins with access to services and configuration
- **`PluginMetadata`** - Stores and validates plugin information
- **`PluginResourceManager`** - Manages plugin resources and cleanup

## 🔧 Plugin Development SDK

### Attributes
```csharp
[Plugin("plugin-id", "Plugin Name", "1.0.0")]
[GameSystemPlugin("Game System Name")]
public class MyPlugin : PluginBase, IGameSystemPlugin
{
    // Plugin implementation
}
```

### Helper Classes
- **`DiceNotationHelper`** - Utilities for parsing and validating dice expressions
- **`TableLookupHelper`** - Simplified creation and management of lookup tables
- **`PluginBase`** - Base class providing common plugin functionality

## 🚀 Creating Your First Plugin

### 1. Minimal Plugin Example
```csharp
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;

[Plugin("helloworld", "Hello World", "1.0.0",
    Description = "A simple greeting plugin",
    Author = "Your Name")]
public class HelloWorldPlugin : PluginBase
{
    public override string Id => "helloworld";
    public override string Name => "Hello World";
    public override Version Version => new(1, 0, 0);
    public override string Description => "A simple greeting plugin";
    public override string Author => "Your Name";

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Hello World plugin initialized!");
        await Task.CompletedTask;
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Hello World plugin started!");
        await Task.CompletedTask;
    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Hello World plugin stopped!");
        await Task.CompletedTask;
    }
}
```

### 2. Game System Plugin Example
```csharp
[Plugin("mygame", "My Game System", "1.0.0")]
[GameSystemPlugin("My Game System")]
public class MyGamePlugin : PluginBase, IGameSystemPlugin
{
    private readonly List<GameSystemCommand> _commands = new();
    private readonly List<GameSystemTable> _tables = new();

    public string GameSystemName => "My Game System";
    public string GameSystemVersion => "1.0";
    public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();
    public IReadOnlyList<GameSystemTable> Tables => _tables.AsReadOnly();

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        // Register commands
        _commands.Add(new GameSystemCommand(
            "skillcheck",
            "Perform a skill check",
            new List<GameSystemParameter>
            {
                new("skill", typeof(int), true, null, "Skill level (1-100)"),
                new("difficulty", typeof(string), false, "normal", "Difficulty level")
            },
            "Roll against a skill to determine success or failure.",
            true));

        // Register tables
        _tables.Add(new GameSystemTable(
            "criticals",
            "Critical hit effects",
            typeof(int),
            typeof(string),
            "Random critical hit effects for combat."));

        await Task.CompletedTask;
    }

    public async Task<GameSystemCommandResult> ExecuteCommandAsync(
        string commandName, 
        Dictionary<string, object> parameters, 
        GameSystemContext context)
    {
        return commandName.ToLowerInvariant() switch
        {
            "skillcheck" => await ExecuteSkillCheck(parameters, context),
            _ => new GameSystemCommandResult(false, null, "", $"Unknown command: {commandName}")
        };
    }

    public async Task<GameSystemTableResult> LookupTableAsync(
        string tableName, 
        object key, 
        GameSystemContext context)
    {
        return tableName.ToLowerInvariant() switch
        {
            "criticals" => await LookupCriticalHit(key, context),
            _ => new GameSystemTableResult(false, null, "", $"Unknown table: {tableName}")
        };
    }

    public GameSystemHelpInfo GetHelp(string? commandName = null)
    {
        // Return help information for commands and tables
        return new GameSystemHelpInfo(GameSystemName, "My game system description", 
            new List<CommandHelpInfo>(), new List<TableHelpInfo>());
    }

    private async Task<GameSystemCommandResult> ExecuteSkillCheck(
        Dictionary<string, object> parameters, GameSystemContext context)
    {
        // Implementation details...
        await Task.CompletedTask;
        return new GameSystemCommandResult(true, "Success!", "🎲 Skill check succeeded!");
    }

    private async Task<GameSystemTableResult> LookupCriticalHit(
        object key, GameSystemContext context)
    {
        // Implementation details...
        await Task.CompletedTask;
        return new GameSystemTableResult(true, "Critical hit!", "💥 Devastating blow!");
    }
}
```

## 🔄 Plugin Lifecycle

### Lifecycle States
```
Unloaded → Loading → Loaded → Initializing → Initialized → Starting → Running
    ↓
Stopping → Stopped → Disposed
```

### State Transitions
1. **Unloaded**: Plugin assembly not yet loaded
2. **Loading**: Assembly being loaded and validated
3. **Loaded**: Assembly loaded, plugin instance created
4. **Initializing**: `OnInitializeAsync()` being called
5. **Initialized**: Plugin successfully initialized
6. **Starting**: `OnStartAsync()` being called
7. **Running**: Plugin active and handling requests
8. **Stopping**: `OnStopAsync()` being called
9. **Stopped**: Plugin stopped, ready for disposal
10. **Disposed**: Plugin disposed and resources cleaned up

### Lifecycle Methods
```csharp
protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
{
    // One-time initialization
    // Register commands and tables
    // Load configuration
    // Initialize services
}

protected override async Task OnStartAsync(CancellationToken cancellationToken)
{
    // Start background services
    // Begin processing requests
    // Connect to external resources
}

protected override async Task OnStopAsync(CancellationToken cancellationToken)
{
    // Stop background services
    // Disconnect from external resources
    // Cleanup active operations
}

protected override void OnDispose()
{
    // Final cleanup
    // Dispose of resources
    // Clear collections
}
```

## ⚙️ Configuration System

### Plugin Manifest (Plugin.manifest.json)
```json
{
  "id": "mygame",
  "name": "My Game System",
  "version": "1.0.0",
  "description": "Support for My Game System RPG",
  "author": "Your Name",
  "website": "https://example.com",
  "license": "MIT",
  "tags": ["rpg", "fantasy", "dice"],
  "minimumHostVersion": "1.0.0",
  "assemblyFileName": "DiceRoller.Plugins.MyGame.dll",
  "pluginTypeName": "DiceRoller.Plugins.MyGame.MyGamePlugin",
  "defaultConfiguration": {
    "enableAdvancedRules": true,
    "defaultDifficulty": "normal"
  },
  "dependencies": [
    {
      "pluginId": "core-utilities",
      "minimumVersion": "1.0.0",
      "isOptional": false
    }
  ]
}
```

### Runtime Configuration Access
```csharp
// In your plugin class
protected override async Task OnConfigurationUpdatedAsync(object configuration)
{
    var advancedRules = GetConfigurationValue("enableAdvancedRules", true);
    var difficulty = GetConfigurationValue("defaultDifficulty", "normal");
    
    // Update plugin behavior based on new configuration
    UpdateGameRules(advancedRules, difficulty);
    
    Logger.LogInformation("Configuration updated: Advanced={Advanced}, Difficulty={Difficulty}",
        advancedRules, difficulty);
}

// Access configuration during normal operation
private void SomeMethod()
{
    var setting = GetConfigurationValue("someSetting", "defaultValue");
    // Use setting...
}
```

## 🛡️ Security and Isolation

### Resource Management
- **Memory Limits**: Plugins run with controlled memory allocation
- **CPU Throttling**: Prevents plugins from consuming excessive CPU
- **File System Access**: Limited to plugin directory and designated areas
- **Network Access**: Controlled through permission system

### Security Best Practices
```csharp
// Input validation
protected string ValidateAndSanitizeInput(string input, int maxLength = 100)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be empty");
    
    if (input.Length > maxLength)
        throw new ArgumentException($"Input too long (max {maxLength} characters)");
    
    // Additional sanitization as needed
    return input.Trim();
}

// Safe resource handling
protected override void OnDispose()
{
    try
    {
        // Cleanup resources
        _httpClient?.Dispose();
        _fileStreams?.ForEach(fs => fs.Dispose());
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "Error during plugin disposal");
    }
}
```

## 📊 Health Monitoring

### Health Status Implementation
```csharp
protected override PluginHealthStatus? OnGetHealthStatus()
{
    var responseTime = MeasureResponseTime();
    var isHealthy = CheckHealthConditions();
    
    return new PluginHealthStatus(
        isHealthy,
        isHealthy ? "Running normally" : "Experiencing issues",
        responseTime,
        new Dictionary<string, object>
        {
            ["commandCount"] = Commands.Count,
            ["tableCount"] = Tables.Count,
            ["lastCommandTime"] = _lastCommandTime,
            ["errorCount"] = _errorCount,
            ["memoryUsage"] = GC.GetTotalMemory(false)
        });
}

private bool CheckHealthConditions()
{
    // Check various health indicators
    return _errorCount < 10 && 
           DateTime.UtcNow - _lastCommandTime < TimeSpan.FromMinutes(30);
}
```

### Performance Monitoring
```csharp
public async Task<GameSystemCommandResult> ExecuteCommandAsync(
    string commandName, 
    Dictionary<string, object> parameters, 
    GameSystemContext context)
{
    using var activity = StartActivity($"Execute-{commandName}");
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var result = await ExecuteCommandInternal(commandName, parameters, context);
        
        Logger.LogDebug("Command {Command} executed in {Duration}ms", 
            commandName, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error executing command {Command}", commandName);
        _errorCount++;
        throw;
    }
    finally
    {
        stopwatch.Stop();
        RecordPerformanceMetric(commandName, stopwatch.Elapsed);
    }
}
```

## 🧪 Testing Your Plugin

### Unit Testing Setup
```csharp
[TestFixture]
public class MyGamePluginTests
{
    private MyGamePlugin _plugin;
    private Mock<ILogger<MyGamePlugin>> _mockLogger;
    private PluginContext _context;

    [SetUp]
    public async Task Setup()
    {
        _mockLogger = new Mock<ILogger<MyGamePlugin>>();
        _plugin = new MyGamePlugin();
        
        // Create mock context
        _context = CreateMockPluginContext();
        
        await _plugin.InitializeAsync(_context);
        await _plugin.StartAsync();
    }

    [Test]
    public async Task ExecuteCommand_SkillCheck_ReturnsSuccess()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["skill"] = 75,
            ["difficulty"] = "normal"
        };
        var gameContext = new GameSystemContext("test-user");

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, gameContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(result.Description);
    }

    [TearDown]
    public async Task Teardown()
    {
        await _plugin.StopAsync();
        _plugin.Dispose();
    }
}
```

### Integration Testing
```csharp
[TestFixture]
public class PluginIntegrationTests
{
    [Test]
    public async Task PluginManager_LoadPlugin_Success()
    {
        // Test complete plugin loading workflow
        var manager = new PluginManager();
        var pluginPath = "path/to/plugin";
        
        var plugin = await manager.LoadPluginAsync(pluginPath);
        
        Assert.IsNotNull(plugin);
        Assert.AreEqual(PluginStatus.Running, plugin.Status);
    }
}
```

## 🔧 Advanced Features

### Custom Plugin Types
```csharp
public interface ICustomPlugin : IPlugin
{
    Task<CustomResult> ProcessCustomRequestAsync(CustomRequest request);
}

[Plugin("custom", "Custom Plugin", "1.0.0")]
public class CustomPlugin : PluginBase, ICustomPlugin
{
    public async Task<CustomResult> ProcessCustomRequestAsync(CustomRequest request)
    {
        // Custom implementation
        return new CustomResult();
    }
}
```

### Event-Driven Architecture
```csharp
public class EventDrivenPlugin : PluginBase
{
    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        // Subscribe to system events
        Context.EventAggregator.Subscribe<GameSystemEvent>(OnGameSystemEvent);
        Context.EventAggregator.Subscribe<UserActionEvent>(OnUserAction);
    }

    private async Task OnGameSystemEvent(GameSystemEvent gameEvent)
    {
        Logger.LogInformation("Game system event received: {Event}", gameEvent.Type);
        // Handle event
    }

    private async Task OnUserAction(UserActionEvent userEvent)
    {
        Logger.LogInformation("User action: {Action} by {User}", userEvent.Action, userEvent.UserId);
        // Handle user action
    }
}
```

### External Resource Integration
```csharp
public class ExternalDataPlugin : PluginBase
{
    private HttpClient _httpClient;
    private Timer _refreshTimer;

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        _httpClient = new HttpClient();
        
        // Load initial data
        await LoadExternalDataAsync();
        
        // Set up periodic refresh
        _refreshTimer = new Timer(RefreshData, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }

    private async Task LoadExternalDataAsync()
    {
        try
        {
            var dataUrl = GetConfigurationValue("externalDataUrl", "");
            if (!string.IsNullOrEmpty(dataUrl))
            {
                var response = await _httpClient.GetStringAsync(dataUrl);
                ProcessExternalData(response);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load external data");
        }
    }

    protected override void OnDispose()
    {
        _refreshTimer?.Dispose();
        _httpClient?.Dispose();
        base.OnDispose();
    }
}
```

## 📚 Additional Resources

### Documentation
- **[Plugin Development Quickstart](../../../PLUGIN_DEVELOPMENT_QUICKSTART.md)** - Get started in 10 minutes
- **[Template Plugin](../DiceRoller.Plugins.Template/)** - Comprehensive example
- **[CallOfCthulhu Plugin](../DiceRoller.Plugins.CallOfCthulhu/)** - Real-world implementation

### API Reference
- **Core Interfaces**: `IPlugin`, `IGameSystemPlugin`, `PluginContext`
- **Base Classes**: `PluginBase`, helper classes in SDK
- **Management**: `PluginManager`, `PluginLoader`, `PluginDiscovery`

### Best Practices
1. **Error Handling**: Always wrap operations in try-catch blocks
2. **Logging**: Use structured logging with appropriate log levels
3. **Resource Management**: Dispose of resources properly
4. **Testing**: Write comprehensive unit and integration tests
5. **Performance**: Monitor and optimize plugin performance
6. **Security**: Validate all inputs and limit resource usage

---

**Ready to build amazing plugins for DiceRoller.Net!** 🎲

*Next: Check out the [Plugin Development Quickstart](../../../PLUGIN_DEVELOPMENT_QUICKSTART.md) for hands-on experience.*