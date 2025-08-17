# DiceRoller.Net Plugin Template

A comprehensive template for creating game system plugins for DiceRoller.Net. This template provides examples of all major plugin features including commands, lookup tables, configuration management, and Discord integration.

## 🚀 Quick Start

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

## 🚨 Common Pitfalls to Avoid

1. **Blocking Operations**: Always use async/await properly
2. **Memory Leaks**: Dispose resources in OnDispose()
3. **Exception Swallowing**: Log errors and return proper error results
4. **Configuration Ignoring**: Handle configuration updates properly
5. **Parameter Validation**: Validate all user input thoroughly
6. **Hardcoded Values**: Use configuration for customizable behavior
7. **Poor Error Messages**: Provide helpful, actionable error text
8. **Discord Formatting**: Test output in Discord for readability

## 📚 Additional Resources

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