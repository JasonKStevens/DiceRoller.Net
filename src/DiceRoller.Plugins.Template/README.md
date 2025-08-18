# DiceRoller.Net Plugin Template

A comprehensive template for creating game system plugins for DiceRoller.Net. This template provides examples of all major plugin features including commands, lookup tables, configuration management, and Discord integration.

## 🚀 Quick Start

### New to Plugin Development? 
👋 **Start with the [Plugin Development Quickstart](../../../PLUGIN_DEVELOPMENT_QUICKSTART.md)** for a simple HelloWorld example that gets you up and running in 10 minutes!

### Using This Template

1. **Copy the Template**
   ```bash
   cp -r DiceRoller.Plugins.Template DiceRoller.Plugins.YourGameSystem
   cd DiceRoller.Plugins.YourGameSystem
   ```

2. **Rename and Customize**
   - Update `Plugin.manifest.json` with your plugin information
   - Rename the namespace from `DiceRoller.Plugins.Template` to your plugin name
   - Replace all class names (TemplatePlugin → YourGameSystemPlugin)
   - Update project file metadata

3. **Implement Your Game System**
   - Modify commands in `Commands/` directory
   - Update lookup tables in `Tables/` directory
   - Customize game rules in `GameSystem/` directory

4. **Test and Deploy**
   ```bash
   dotnet build
   dotnet test
   # Copy output to DiceRoller.Bot plugins directory
   ```

## 📋 What's Included

This template demonstrates all major plugin capabilities:

### Core Components
- **Main Plugin Class** (`TemplatePlugin.cs`) - Entry point and lifecycle management
- **Game System** (`GameSystem/TemplateGameSystem.cs`) - Coordinates commands and tables
- **Commands** (`Commands/ExampleCommand.cs`) - Interactive gameplay commands
- **Tables** (`Tables/ExampleTable.cs`) - Lookup tables for game data
- **Manifest** (`Plugin.manifest.json`) - Plugin metadata and configuration

### Example Commands
- **skillcheck** - Skill checks with difficulty modifiers
- **initiative** - Combat initiative with advantage support
- **damage** - Damage calculation with resistance handling
- **healing** - Healing with type-specific bonuses

### Example Tables
- **criticals** - Critical hit effects (range-based)
- **fumbles** - Critical failure complications
- **weather** - Environmental conditions
- **encounters** - Random encounters

### Features Demonstrated
- ✅ Parameter validation and extraction
- ✅ Error handling and logging
- ✅ Configuration management
- ✅ Discord-optimized formatting
- ✅ Health monitoring
- ✅ Help system integration
- ✅ Range-based and exact lookup tables
- ✅ Game rule implementation
- ✅ Resource management

## 🏗️ Plugin Architecture

### Plugin Lifecycle
```
Unloaded → Loading → Loaded → Initializing → Initialized → Starting → Running
```

### Key Interfaces
- `IPlugin` - Base plugin functionality
- `IGameSystemPlugin` - Game system specific features

### Directory Structure
```
DiceRoller.Plugins.Template/
├── Plugin.manifest.json          # Plugin metadata
├── TemplatePlugin.cs             # Main plugin class
├── GameSystem/
│   └── TemplateGameSystem.cs     # Game system coordinator
├── Commands/
│   └── ExampleCommand.cs         # Command implementations
├── Tables/
│   └── ExampleTable.cs           # Lookup table implementations
├── README.md                     # This file
├── DEVELOPMENT.md                # Step-by-step guide
└── DiceRoller.Plugins.Template.csproj
```

## 🎮 Command Implementation Guide

### Basic Command Structure
```csharp
[Command("commandname", Description = "What this command does")]
public async Task<GameSystemCommandResult> ExecuteAsync(
    Dictionary<string, object> parameters,
    GameSystemContext context)
{
    try
    {
        // 1. Extract and validate parameters
        var value = ExtractIntParameter(parameters, "value", 0);
        
        // 2. Perform game logic
        var result = DoGameCalculation(value);
        
        // 3. Format result for Discord
        var description = FormatResult(result);
        
        // 4. Return success result
        return new GameSystemCommandResult(true, result, description);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in command");
        return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
    }
}
```

### Parameter Extraction Helpers
```csharp
// Required integer parameter
if (!ExtractIntParameter(parameters, "skill_value", out var skill))
    return ErrorResult("Skill value is required");

// Optional parameter with default
var modifier = ExtractIntParameter(parameters, "modifier", 0);
var difficulty = ExtractStringParameter(parameters, "difficulty", "normal");
var advantage = ExtractBoolParameter(parameters, "advantage", false);
```

### Result Formatting Best Practices
- Use Discord markdown for emphasis (**bold**, *italic*)
- Include emoji for visual appeal (⚔️ 🎲 ✅ ❌)
- Show calculation steps for transparency
- Keep within Discord's 2000 character limit
- Use consistent formatting across commands

## 📊 Table Implementation Guide

### Range-Based Tables
Perfect for dice roll results (d20, d100, etc.):

```csharp
private List<TableRangeEntry> InitializeTable()
{
    var tableData = new Dictionary<string, string>
    {
        ["01-05"] = "First range result",
        ["06-10"] = "Second range result",
        ["11-20"] = "Third range result"
    };
    
    return TableLookupHelper.CreateRangeTable(tableData);
}

public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
{
    if (!int.TryParse(key.ToString(), out var roll))
        return ErrorResult("Invalid roll value");
        
    return TableLookupHelper.LookupByRange(_table, roll, _logger);
}
```

### Exact Key Tables
Perfect for named lookups (skill names, spell names, etc.):

```csharp
private Dictionary<string, ResultType> _exactTable = new()
{
    ["fire"] = new FireResult(),
    ["ice"] = new IceResult(),
    ["lightning"] = new LightningResult()
};

public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
{
    var keyString = key.ToString()?.ToLowerInvariant() ?? "";
    
    if (!_exactTable.TryGetValue(keyString, out var result))
        return ErrorResult($"Unknown key: {key}");
        
    return new GameSystemTableResult(true, result, FormatResult(result));
}
```

## ⚙️ Configuration Management

### Plugin Manifest
The `Plugin.manifest.json` file defines your plugin's metadata:

```json
{
  "id": "yourgamesystem",
  "name": "Your Game System",
  "version": "1.0.0",
  "description": "Brief description of your game system support",
  "author": "Your Name",
  "defaultConfiguration": {
    "enableAdvancedFeatures": true,
    "defaultDifficulty": "normal"
  }
}
```

### Runtime Configuration Access
```csharp
// In your plugin class
var enableFeatures = GetConfigurationValue("enableAdvancedFeatures", true);
var defaultDiff = GetConfigurationValue("defaultDifficulty", "normal");

// Handle configuration updates
protected override async Task OnConfigurationUpdatedAsync(object configuration)
{
    // Reload settings and update components
    _gameSystem.UpdateConfiguration(newSettings);
}
```

## 🔧 Development Best Practices

### Error Handling
- Always wrap command/table logic in try-catch blocks
- Log errors with appropriate log levels
- Return meaningful error messages to users
- Validate all input parameters

### Logging
```csharp
// Use appropriate log levels
_logger.LogDebug("Detailed debugging info");
_logger.LogInformation("General information");
_logger.LogWarning("Potential issues");
_logger.LogError(ex, "Error details with exception");
```

### Testing
- Test all command parameter combinations
- Test table lookups with edge cases (min/max values)
- Test error conditions and invalid inputs
- Test configuration changes
- Use deterministic random number generation for reproducible tests

**See the [Plugin Testing Guide](#-plugin-testing-guide) below for detailed testing patterns and examples.**

### Performance
- Initialize expensive operations once during plugin startup
- Cache frequently accessed data
- Use efficient data structures for table lookups
- Monitor memory usage in health checks

## 🎯 Discord Integration

### Formatting Guidelines
- **Commands**: Use emoji and bold text for clarity
- **Results**: Show calculation steps and final values
- **Tables**: Include context about what was rolled
- **Errors**: Be helpful and suggest correct usage

### Example Discord Output
```
⚔️ Critical Hit! (rolled 18)
**Vital Strike** - 1.5x damage and target has disadvantage on next attack

🎲 Skill Check (hard): ✅ Success
Roll: 45 vs 55 (base 75, difficulty -20)
📊 Result: Success - Standard success
```

### Message Length Management
Discord has a 2000 character limit. For long results:
- Prioritize most important information
- Use abbreviations where appropriate
- Consider splitting complex results
- Implement truncation with "..." indicator

## 🔍 Help System Integration

Your plugin should provide comprehensive help information:

```csharp
public GameSystemHelpInfo GetHelp(string? commandName = null)
{
    return new GameSystemHelpInfo(
        GameSystemName,
        "Description of your game system",
        commandHelps,    // List of CommandHelpInfo
        tableHelps       // List of TableHelpInfo
    );
}
```

Help information appears in:
- `/rollhelp` command responses
- Plugin management commands
- Error messages with usage suggestions

## 🧪 Plugin Testing Guide

Comprehensive testing ensures your plugin works reliably across all scenarios. Here are testing patterns specifically designed for DiceRoller.Net plugins.

### Test Project Setup

Create a test project alongside your plugin:

```xml
<!-- Tests/DiceRoller.Plugins.YourGame.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NUnit" Version="3.13.3" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.7.1" />
    <PackageReference Include="Moq" Version="4.20.69" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../DiceRoller.Plugins.YourGame.csproj" />
  </ItemGroup>
</Project>
```

### Unit Testing Patterns

#### 1. Plugin Lifecycle Testing
```csharp
[TestFixture]
public class YourGamePluginLifecycleTests
{
    private YourGamePlugin _plugin;
    private Mock<ILogger<YourGamePlugin>> _mockLogger;
    private PluginContext _mockContext;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<YourGamePlugin>>();
        _plugin = new YourGamePlugin();
        _mockContext = CreateMockPluginContext();
    }

    [Test]
    public async Task Initialize_WithValidContext_SetsUpCommandsAndTables()
    {
        // Act
        await _plugin.InitializeAsync(_mockContext);

        // Assert
        Assert.That(_plugin.Commands.Count, Is.GreaterThan(0));
        Assert.That(_plugin.Tables.Count, Is.GreaterThanOrEqualTo(0));
        Assert.That(_plugin.Status, Is.EqualTo(PluginStatus.Initialized));
    }

    [Test]
    public async Task StartStop_Lifecycle_CompletesSuccessfully()
    {
        // Arrange
        await _plugin.InitializeAsync(_mockContext);

        // Act & Assert - Start
        await _plugin.StartAsync();
        Assert.That(_plugin.Status, Is.EqualTo(PluginStatus.Running));

        // Act & Assert - Stop
        await _plugin.StopAsync();
        Assert.That(_plugin.Status, Is.EqualTo(PluginStatus.Stopped));
    }

    private PluginContext CreateMockPluginContext()
    {
        var mockContext = new Mock<PluginContext>();
        // Set up mock context properties as needed
        return mockContext.Object;
    }
}
```

#### 2. Command Testing
```csharp
[TestFixture]
public class YourGameCommandTests
{
    private YourGamePlugin _plugin;
    private GameSystemContext _gameContext;

    [SetUp]
    public async Task Setup()
    {
        _plugin = new YourGamePlugin();
        await _plugin.InitializeAsync(CreateMockPluginContext());
        await _plugin.StartAsync();
        _gameContext = new GameSystemContext("test-user-123");
    }

    [TestCase(75, "normal", 0, ExpectedResult = true)]
    [TestCase(30, "hard", -10, ExpectedResult = false)]
    [TestCase(90, "easy", 5, ExpectedResult = true)]
    public async Task<bool> SkillCheck_WithVariousParameters_ReturnsExpectedSuccess(
        int skillValue, string difficulty, int modifier)
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["skill_value"] = skillValue,
            ["difficulty"] = difficulty,
            ["modifier"] = modifier
        };

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, _gameContext);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Description, Is.Not.Empty);
        
        // Extract success from result (depends on your implementation)
        var skillResult = result.Value as SkillCheckResult;
        return skillResult?.Success ?? false;
    }

    [Test]
    public async Task Command_WithMissingRequiredParameter_ReturnsError()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            // Missing required "skill_value" parameter
            ["difficulty"] = "normal"
        };

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, _gameContext);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("skill_value"));
    }

    [Test]
    public async Task Command_WithInvalidParameterType_ReturnsError()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["skill_value"] = "not-a-number", // Invalid type
            ["difficulty"] = "normal"
        };

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, _gameContext);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("invalid"));
    }

    [Test]
    public async Task Command_WithUnknownCommand_ReturnsError()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act
        var result = await _plugin.ExecuteCommandAsync("nonexistent", parameters, _gameContext);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("Unknown command"));
    }
}
```

#### 3. Table Testing
```csharp
[TestFixture]
public class YourGameTableTests
{
    private YourGamePlugin _plugin;
    private GameSystemContext _gameContext;

    [SetUp]
    public async Task Setup()
    {
        _plugin = new YourGamePlugin();
        await _plugin.InitializeAsync(CreateMockPluginContext());
        await _plugin.StartAsync();
        _gameContext = new GameSystemContext("test-user-123");
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(20)]
    public async Task CriticalHitsTable_WithValidRolls_ReturnsResults(int roll)
    {
        // Act
        var result = await _plugin.LookupTableAsync("criticals", roll, _gameContext);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Description, Is.Not.Empty);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(21)]
    public async Task CriticalHitsTable_WithInvalidRolls_ReturnsError(int roll)
    {
        // Act
        var result = await _plugin.LookupTableAsync("criticals", roll, _gameContext);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    [Test]
    public async Task Table_WithUnknownTable_ReturnsError()
    {
        // Act
        var result = await _plugin.LookupTableAsync("nonexistent", 1, _gameContext);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("Unknown table"));
    }
}
```

#### 4. Deterministic Testing with Mocked RNG
```csharp
[TestFixture]
public class DeterministicGameTests
{
    private YourGamePlugin _plugin;
    private Mock<IRandomNumberGenerator> _mockRandom;

    [SetUp]
    public async Task Setup()
    {
        _mockRandom = new Mock<IRandomNumberGenerator>();
        _plugin = new YourGamePlugin(_mockRandom.Object);
        await _plugin.InitializeAsync(CreateMockPluginContext());
        await _plugin.StartAsync();
    }

    [Test]
    public async Task SkillCheck_WithMockedRNG_ProducesExpectedResult()
    {
        // Arrange
        _mockRandom.Setup(r => r.Next(1, 101)).Returns(75); // Mock d100 roll
        var parameters = new Dictionary<string, object>
        {
            ["skill_value"] = 80,
            ["difficulty"] = "normal"
        };

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, _gameContext);

        // Assert
        Assert.That(result.Success, Is.True);
        var skillResult = result.Value as SkillCheckResult;
        Assert.That(skillResult.Roll, Is.EqualTo(75));
        Assert.That(skillResult.Success, Is.True); // 75 <= 80
    }
}
```

### Integration Testing

#### 1. End-to-End Plugin Testing
```csharp
[TestFixture]
public class PluginIntegrationTests
{
    private PluginManager _pluginManager;
    private YourGamePlugin _plugin;

    [SetUp]
    public async Task Setup()
    {
        _pluginManager = new PluginManager();
        
        // Load plugin through manager (simulates real loading)
        var pluginPath = GetTestPluginPath();
        _plugin = await _pluginManager.LoadPluginAsync<YourGamePlugin>(pluginPath);
    }

    [Test]
    public async Task LoadedPlugin_ExecutesCommands_Successfully()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["skill_value"] = 70,
            ["difficulty"] = "normal"
        };
        var context = new GameSystemContext("integration-test-user");

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, context);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public void LoadedPlugin_HealthCheck_ReturnsHealthy()
    {
        // Act
        var health = _plugin.GetHealthStatus();

        // Assert
        Assert.That(health.IsHealthy, Is.True);
        Assert.That(health.Status, Is.Not.Empty);
    }
}
```

#### 2. Configuration Testing
```csharp
[TestFixture]
public class ConfigurationTests
{
    [Test]
    public async Task Plugin_WithCustomConfiguration_UpdatesBehavior()
    {
        // Arrange
        var customConfig = new
        {
            enableAdvancedRules = false,
            defaultDifficulty = "hard"
        };
        
        var plugin = new YourGamePlugin();
        await plugin.InitializeAsync(CreateMockPluginContext());

        // Act
        await plugin.OnConfigurationChangedAsync(customConfig);

        // Assert
        // Verify configuration was applied (depends on your implementation)
        var configValue = plugin.GetConfigurationValue("defaultDifficulty", "normal");
        Assert.That(configValue, Is.EqualTo("hard"));
    }
}
```

### Performance Testing

```csharp
[TestFixture]
public class PerformanceTests
{
    private YourGamePlugin _plugin;

    [SetUp]
    public async Task Setup()
    {
        _plugin = new YourGamePlugin();
        await _plugin.InitializeAsync(CreateMockPluginContext());
        await _plugin.StartAsync();
    }

    [Test]
    public async Task Command_ExecutionTime_IsWithinAcceptableLimits()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["skill_value"] = 75
        };
        var context = new GameSystemContext("perf-test-user");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, context);

        // Assert
        stopwatch.Stop();
        Assert.That(result.Success, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100)); // < 100ms
    }

    [Test]
    public async Task BulkCommands_MemoryUsage_RemainsStable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var parameters = new Dictionary<string, object> { ["skill_value"] = 75 };
        var context = new GameSystemContext("bulk-test-user");

        // Act - Execute many commands
        for (int i = 0; i < 1000; i++)
        {
            await _plugin.ExecuteCommandAsync("skillcheck", parameters, context);
        }

        // Assert
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        Assert.That(memoryIncrease, Is.LessThan(1024 * 1024)); // < 1MB increase
    }
}
```

### Testing Best Practices

1. **Use Descriptive Test Names**: `Command_WithInvalidParameter_ReturnsError`
2. **Follow AAA Pattern**: Arrange, Act, Assert
3. **Test Edge Cases**: Empty inputs, boundary values, null parameters
4. **Mock External Dependencies**: Random number generators, HTTP clients
5. **Test Error Conditions**: Invalid inputs, missing parameters, exceptions
6. **Verify Output Format**: Discord-compatible formatting, character limits
7. **Performance Testing**: Response times, memory usage, bulk operations
8. **Integration Testing**: Full plugin lifecycle, real-world scenarios

### Common Test Scenarios

- ✅ Valid parameter combinations
- ✅ Missing required parameters
- ✅ Invalid parameter types
- ✅ Boundary value testing (min/max values)
- ✅ Error handling and logging
- ✅ Configuration changes
- ✅ Plugin lifecycle (init, start, stop)
- ✅ Health status reporting
- ✅ Performance under load
- ✅ Memory usage stability
- ✅ Discord formatting compliance

## 🚨 Common Pitfalls to Avoid

1. **Blocking Operations**: Always use async/await properly
2. **Memory Leaks**: Dispose resources in OnDispose()
3. **Exception Swallowing**: Log errors and return proper error results
4. **Configuration Ignoring**: Handle configuration updates properly
5. **Parameter Validation**: Validate all user input thoroughly
6. **Hardcoded Values**: Use configuration for customizable behavior
7. **Poor Error Messages**: Provide helpful, actionable error text
8. **Discord Formatting**: Test output in Discord for readability

## 🌟 Simple HelloWorld Example

Want to see a minimal plugin before diving into the full template? Here's a complete working plugin with just one command:

```csharp
[Plugin("helloworld", "HelloWorld Game System", "1.0.0")]
[GameSystemPlugin("HelloWorld")]
public class HelloWorldPlugin : PluginBase, IGameSystemPlugin
{
    private readonly List<GameSystemCommand> _commands = new();
    
    public override string Id => "helloworld";
    public override string Name => "HelloWorld Game System";
    public string GameSystemName => "HelloWorld";
    public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();
    public IReadOnlyList<GameSystemTable> Tables => new List<GameSystemTable>().AsReadOnly();

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        _commands.Add(new GameSystemCommand(
            "hello",
            "Say hello to the world!",
            new List<GameSystemParameter>
            {
                new("name", typeof(string), false, "World", "Name to greet")
            },
            "A simple greeting command.",
            true));
        await Task.CompletedTask;
    }

    public async Task<GameSystemCommandResult> ExecuteCommandAsync(
        string commandName, Dictionary<string, object> parameters, GameSystemContext context)
    {
        if (commandName == "hello")
        {
            var name = parameters.TryGetValue("name", out var nameValue) ? nameValue?.ToString() : "World";
            var message = $"🌟 Hello, **{name}**! Welcome to DiceRoller.Net! 🎲";
            return new GameSystemCommandResult(true, message, message);
        }
        return new GameSystemCommandResult(false, null, "", $"Unknown command: {commandName}");
    }

    // Other required methods with simple implementations...
}
```

**Result in Discord:** `/helloworld hello name:Alice` → "🌟 Hello, **Alice**! Welcome to DiceRoller.Net! 🎲"

This 40-line plugin demonstrates the core concepts. See the [Plugin Development Quickstart](../../../PLUGIN_DEVELOPMENT_QUICKSTART.md) for the complete walkthrough!

## 📚 Additional Resources

- [Plugin Development Quickstart](../../../PLUGIN_DEVELOPMENT_QUICKSTART.md) - Beginner-friendly 10-minute tutorial
- [Plugin Infrastructure Documentation](../DiceRoller.Plugins.Infrastructure/README.md)
- [DEVELOPMENT.md](DEVELOPMENT.md) - Step-by-step creation guide
- [DiceRoller.Net Core Documentation](../../README.md)
- [Discord API Documentation](https://discord.com/developers/docs)

## 🤝 Contributing

When creating plugins for the community:

1. Follow the naming convention: `DiceRoller.Plugins.{GameSystemName}`
2. Include comprehensive tests
3. Document all commands and tables
4. Provide example usage
5. Include proper error handling
6. Test thoroughly in Discord environment

## 📄 License

This template is provided under the MIT License. When creating your own plugin, you may choose your own license terms.

---

**Happy Plugin Development!** 🎲

For questions or assistance, please refer to the main DiceRoller.Net documentation or community resources.