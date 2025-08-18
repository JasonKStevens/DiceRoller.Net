# DiceRoller.Net API Documentation

## Overview

DiceRoller.Net provides a comprehensive set of APIs for dice rolling, expression parsing, game system integration, and plugin development. This document covers all major APIs and interfaces available for developers working with or extending the system.

## Table of Contents

1. [Core APIs](#core-apis)
   - [Dice Rolling Service APIs](#dice-rolling-service-apis)
   - [Dice Expression APIs](#dice-expression-apis)
   - [Parser APIs](#parser-apis)
   - [Function Registry APIs](#function-registry-apis)
   - [Help Service APIs](#help-service-apis)
2. [Game System APIs](#game-system-apis)
   - [Game System Interface](#game-system-interface)
   - [Game System Router](#game-system-router)
3. [Plugin Development APIs](#plugin-development-apis)
   - [Base Plugin Interface](#base-plugin-interface)
   - [Game System Plugins](#game-system-plugins)
   - [Dice Notation Plugins](#dice-notation-plugins)
   - [Plugin Integration Service](#plugin-integration-service)
   - [Plugin Development Workflow](#plugin-development-workflow)
   - [Complete Plugin Example](#complete-plugin-example)
4. [Formatting APIs](#formatting-apis)
   - [Progressive Disclosure System](#progressive-disclosure-system)
   - [Complexity Analysis](#complexity-analysis)
5. [Discord Integration APIs](#discord-integration-apis)
6. [Testing Patterns and Utilities](#testing-patterns-and-utilities)
   - [DiceHelper Utilities](#dicehelper-utilities)
   - [Test Fixture Patterns](#test-fixture-patterns)
   - [Deterministic Testing](#deterministic-testing)
7. [Configuration and Dependency Injection](#configuration-and-dependency-injection)
   - [Service Registration Patterns](#service-registration-patterns)
   - [Configuration Management](#configuration-management)
   - [Circular Dependency Resolution](#circular-dependency-resolution)
8. [Usage Examples](#usage-examples)
9. [Extension Points](#extension-points)
10. [Best Practices](#best-practices)

---

## Core APIs

### Dice Rolling Service APIs

#### IDiceRollingService

High-level service interface for dice rolling operations that provides a simplified API for common use cases.

```csharp
namespace DiceRoller.Core.Abstractions

public interface IDiceRollingService
{
    /// <summary>
    /// Rolls dice based on the provided request
    /// </summary>
    /// <param name="request">The dice roll request</param>
    /// <returns>The result of the dice roll</returns>
    Task<DiceRollResult> RollAsync(DiceRollRequest request);

    /// <summary>
    /// Parses a dice expression without evaluating it
    /// </summary>
    /// <param name="expression">The dice expression to parse</param>
    /// <returns>True if the expression is valid</returns>
    bool IsValidExpression(string expression);
}
```

#### DiceRollRequest

Request model for dice rolling operations.

```csharp
namespace DiceRoller.Core.Models

public class DiceRollRequest
{
    /// <summary>
    /// Gets or sets the dice expression to evaluate
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// Gets or sets the user ID making the request
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the game system context for the roll
    /// </summary>
    public string GameSystem { get; set; }

    /// <summary>
    /// Gets or sets additional context information
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// Gets or sets whether to include detailed breakdown in the result
    /// </summary>
    public bool IncludeBreakdown { get; set; } = true;
}
```

#### DiceRollResult

Result model for dice rolling operations.

```csharp
namespace DiceRoller.Core.Models

public class DiceRollResult
{
    /// <summary>
    /// Gets or sets the numeric value of the roll
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the human-readable breakdown of the roll
    /// </summary>
    public string Breakdown { get; set; }

    /// <summary>
    /// Gets or sets the typed result for formatted output
    /// </summary>
    public TypedResult TypedResult { get; set; }

    /// <summary>
    /// Gets or sets the original expression that was rolled
    /// </summary>
    public string OriginalExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the roll was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Gets or sets any error message if the roll failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Creates a DiceRollResult from a DiceResultNode
    /// </summary>
    public static DiceRollResult FromDiceResultNode(DiceResultNode node, string originalExpression)

    /// <summary>
    /// Creates a failed DiceRollResult
    /// </summary>
    public static DiceRollResult Failed(string expression, string errorMessage)
}
```

**Usage Example:**
```csharp
// High-level service usage
var diceService = serviceProvider.GetService<IDiceRollingService>();
var request = new DiceRollRequest
{
    Expression = "3d6 + 5",
    UserId = "user123",
    IncludeBreakdown = true
};

var result = await diceService.RollAsync(request);
if (result.Success)
{
    Console.WriteLine($"Result: {result.Value}");
    Console.WriteLine($"Breakdown: {result.Breakdown}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### Dice Expression APIs

#### IDiceExpressionParser

Interface for parsing dice expressions into syntax trees.

```csharp
namespace DiceRoller.Core.Abstractions

public interface IDiceExpressionParser
{
    /// <summary>
    /// Parses a dice expression into a syntax tree
    /// </summary>
    /// <param name="expression">The dice expression to parse</param>
    /// <returns>The parsed syntax tree</returns>
    ParseTree Parse(string expression);

    /// <summary>
    /// Determines if the parser has special handling for EarthDawn step functions
    /// </summary>
    bool SupportsStepFunctions { get; }

    /// <summary>
    /// Evaluates an EarthDawn step number if supported
    /// </summary>
    /// <param name="stepNumber">The step number to evaluate</param>
    /// <returns>The parsed syntax tree for the step</returns>
    ParseTree ParseStep(int stepNumber);
}
```

**Usage Example:**
```csharp
var parser = serviceProvider.GetService<IDiceExpressionParser>();
var parseTree = parser.Parse("3d6 + 5");
if (parser.SupportsStepFunctions)
{
    var stepTree = parser.ParseStep(15);
}
```

#### IDiceExpressionEvaluator

Interface for evaluating parsed dice expressions.

```csharp
namespace DiceRoller.Core.Abstractions

public interface IDiceExpressionEvaluator
{
    /// <summary>
    /// Evaluates a parsed syntax tree
    /// </summary>
    /// <param name="parseTree">The parsed syntax tree</param>
    /// <returns>The result of the dice roll</returns>
    DiceResultNode Evaluate(ParseTree parseTree);

    /// <summary>
    /// Evaluates a parsed syntax tree root node
    /// </summary>
    /// <param name="rootNode">The root node of the syntax tree</param>
    /// <returns>The result of the dice roll</returns>
    DiceResultNode Evaluate(ParseTreeNode rootNode);
}
```

**Usage Example:**
```csharp
var evaluator = serviceProvider.GetService<IDiceExpressionEvaluator>();
var result = evaluator.Evaluate(parseTree);
Console.WriteLine($"Result: {result.Value}, Breakdown: {result.Breakdown}");
```

### Parser APIs

#### DiceResultNode

Primary result container for dice roll evaluations.

```csharp
namespace DiceRoller.Core.Parser

public class DiceResultNode
{
    public float Value { get; private set; }
    public string Breakdown { get; private set; }
    public TypedResult TypedResult { get; private set; }

    public DiceResultNode(float value, string breakdown, TypedResult typedResult = null)
}
```

#### TypedResult

Hierarchical structure for representing complex dice expressions.

```csharp
namespace DiceRoller.Core.Parser

public class TypedResult
{
    public NodeType NodeType { get; set; }
    public string Text { get; set; }
    public List<TypedResult> SubText { get; set; }

    // Factory methods for creating typed results
    public static TypedResult NewSimpleResult(string text)
    public static TypedResult NewSimpleResult(NodeType nodeType, string text)
    public static TypedResult NewUnnamedTripartComposite(NodeType nodeType, 
        TypedResult leftText, TypedResult middleText, TypedResult rightText)
}
```

### Function Registry APIs

#### IFunctionRegistry

Registry for dynamically registering and executing dice functions.

```csharp
namespace DiceRoller.Core.Functions

public interface IFunctionRegistry
{
    /// <summary>
    /// Gets all registered function names
    /// </summary>
    IEnumerable<string> RegisteredFunctions { get; }

    /// <summary>
    /// Determines if a function is registered
    /// </summary>
    bool IsRegistered(string functionName);

    /// <summary>
    /// Registers a function with the registry
    /// </summary>
    void RegisterFunction(string functionName, IDiceFunction function);

    /// <summary>
    /// Unregisters a function from the registry
    /// </summary>
    bool UnregisterFunction(string functionName);

    /// <summary>
    /// Executes a registered function
    /// </summary>
    DiceResultNode ExecuteFunction(string functionName, ParseTreeNode node, IFunctionContext context);
}
```

#### IDiceFunction

Interface for implementing custom dice functions.

```csharp
namespace DiceRoller.Core.Functions

public interface IDiceFunction
{
    /// <summary>
    /// Gets the name of the function
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the function with the given parameters
    /// </summary>
    DiceResultNode Execute(ParseTreeNode node, IFunctionContext context);
}
```

**Usage Example:**
```csharp
public class CustomFunction : IDiceFunction
{
    public string Name => "custom";

    public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
    {
        // Function implementation
        var result = PerformCustomLogic(node, context);
        return new DiceResultNode(result, $"custom({node}) = {result}");
    }
}

// Registration
var registry = serviceProvider.GetService<IFunctionRegistry>();
registry.RegisterFunction("custom", new CustomFunction());
```

### Help Service APIs

#### ICommandHelpService

Service for managing command help content and providing help responses.

```csharp
namespace DiceRoller.Core.Services.Help

public interface ICommandHelpService
{
    /// <summary>
    /// Gets help content for a specific command
    /// </summary>
    /// <param name="commandName">The name of the command (e.g., "roll", "dq", "plugin")</param>
    /// <returns>Help content formatted for Discord, or null if command not found</returns>
    string GetCommandHelp(string commandName);

    /// <summary>
    /// Gets general help content for all available commands
    /// </summary>
    /// <returns>General help content formatted for Discord</returns>
    string GetGeneralHelp();

    /// <summary>
    /// Registers custom help content from a plugin
    /// </summary>
    /// <param name="commandName">The command name to register help for</param>
    /// <param name="helpContent">The help content formatted for Discord</param>
    /// <param name="pluginId">The ID of the plugin registering the help</param>
    void RegisterPluginHelp(string commandName, string helpContent, string pluginId);

    /// <summary>
    /// Unregisters help content for a plugin
    /// </summary>
    /// <param name="pluginId">The ID of the plugin to unregister help for</param>
    void UnregisterPluginHelp(string pluginId);

    /// <summary>
    /// Checks if help is available for a specific command
    /// </summary>
    /// <param name="commandName">The command name to check</param>
    /// <returns>True if help is available, false otherwise</returns>
    bool HasHelpFor(string commandName);
}
```

**Usage Example:**
```csharp
var helpService = serviceProvider.GetService<ICommandHelpService>();

// Get help for a specific command
var rollHelp = helpService.GetCommandHelp("roll");
if (rollHelp != null)
{
    Console.WriteLine(rollHelp);
}

// Check if help is available
if (helpService.HasHelpFor("custom"))
{
    var customHelp = helpService.GetCommandHelp("custom");
}

// Register plugin help
helpService.RegisterPluginHelp("coc", "Call of Cthulhu commands help...", "coc.plugin");
```

---

## Game System APIs

### Game System Interface

#### IGameSystem

Base interface for all game system implementations.

```csharp
namespace DiceRoller.Core.Abstractions

public interface IGameSystem
{
    /// <summary>
    /// Gets the name of the game system
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the supported command types for this game system
    /// </summary>
    IEnumerable<string> SupportedCommands { get; }

    /// <summary>
    /// Determines if this game system can handle the specified command
    /// </summary>
    bool SupportsCommand(string commandType);

    /// <summary>
    /// Processes a game system specific command
    /// </summary>
    TypedResult ProcessCommand(string commandType, string[] parameters, string userId);
}
```

### Game System Router

#### IGameSystemRouter

Router for managing multiple game systems and routing commands.

```csharp
namespace DiceRoller.Core.Abstractions

public interface IGameSystemRouter
{
    /// <summary>
    /// Gets all registered game systems
    /// </summary>
    IEnumerable<IGameSystem> RegisteredSystems { get; }

    /// <summary>
    /// Determines if a command can be handled by any registered game system
    /// </summary>
    bool CanHandle(string commandType);

    /// <summary>
    /// Routes a command to the appropriate game system
    /// </summary>
    DiceResultNode RouteCommand(string commandType, string[] parameters = null, string userId = null);

    /// <summary>
    /// Registers a game system with the router
    /// </summary>
    void RegisterSystem(IGameSystem gameSystem);

    /// <summary>
    /// Unregisters a game system from the router
    /// </summary>
    void UnregisterSystem(IGameSystem gameSystem);
}
```

**Usage Example:**
```csharp
public class MyGameSystem : IGameSystem
{
    public string Name => "MyGame";
    public IEnumerable<string> SupportedCommands => new[] { "skill", "attack", "damage" };

    public bool SupportsCommand(string commandType) => SupportedCommands.Contains(commandType);

    public TypedResult ProcessCommand(string commandType, string[] parameters, string userId)
    {
        return commandType switch
        {
            "skill" => ProcessSkillCheck(parameters),
            "attack" => ProcessAttackRoll(parameters),
            "damage" => ProcessDamageRoll(parameters),
            _ => TypedResult.NewSimpleResult("Unknown command")
        };
    }
}

// Registration
var router = serviceProvider.GetService<IGameSystemRouter>();
router.RegisterSystem(new MyGameSystem());
```

---

## Plugin Development APIs

### Base Plugin Interface

#### IPlugin

Base interface for all DiceRoller plugins.

```csharp
namespace DiceRoller.Plugins.Infrastructure.Core

public interface IPlugin : IDisposable
{
    string Id { get; }
    string Name { get; }
    Version Version { get; }
    string Description { get; }
    string Author { get; }
    Version MinimumHostVersion { get; }
    IReadOnlyList<PluginDependency> Dependencies { get; }
    string? ConfigurationSchema { get; }

    Task InitializeAsync(PluginContext context, CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    PluginHealthStatus GetHealthStatus();
    Task OnConfigurationChangedAsync(object configuration);

    event EventHandler<PluginErrorEventArgs>? ErrorOccurred;
    event EventHandler<PluginStatusChangedEventArgs>? StatusChanged;
}
```

### Game System Plugins

#### IGameSystemPlugin

Interface for plugins that provide game system functionality.

```csharp
namespace DiceRoller.Plugins.Infrastructure.Core

public interface IGameSystemPlugin : IPlugin
{
    string GameSystemName { get; }
    string GameSystemVersion { get; }
    IReadOnlyList<GameSystemCommand> Commands { get; }
    IReadOnlyList<GameSystemTable> Tables { get; }

    Task<GameSystemCommandResult> ExecuteCommandAsync(string commandName, 
        Dictionary<string, object> parameters, GameSystemContext context);

    Task<GameSystemTableResult> LookupTableAsync(string tableName, 
        object key, GameSystemContext context);

    GameSystemHelpInfo GetHelp(string? commandName = null);
}
```

**Plugin Implementation Example:**
```csharp
public class CallOfCthulhuPlugin : IGameSystemPlugin
{
    public string Id => "coc.plugin";
    public string Name => "Call of Cthulhu";
    public string GameSystemName => "Call of Cthulhu";
    public Version Version => new Version(1, 0, 0);

    public async Task<GameSystemCommandResult> ExecuteCommandAsync(
        string commandName, Dictionary<string, object> parameters, GameSystemContext context)
    {
        return commandName switch
        {
            "luck" => ExecuteLuckRoll(context),
            "sanity" => ExecuteSanityCheck(parameters, context),
            _ => new GameSystemCommandResult(false, null, "", $"Unknown command: {commandName}")
        };
    }

    private GameSystemCommandResult ExecuteLuckRoll(GameSystemContext context)
    {
        var roll = new Random().Next(1, 101);
        return new GameSystemCommandResult(true, roll, $"Luck roll: {roll}");
    }
}
```

### Dice Notation Plugins

#### IDiceNotationPlugin

Interface for plugins that extend dice notation grammar.

```csharp
namespace DiceRoller.Plugins.Infrastructure.Core

public interface IDiceNotationPlugin : IPlugin
{
    int Priority { get; }
    IReadOnlyList<DiceNotationExtension> GrammarExtensions { get; }
    IReadOnlyList<DiceNotationFunction> FunctionExtensions { get; }
    IReadOnlyList<DiceNotationOperator> OperatorExtensions { get; }

    bool CanHandle(string notation);
    Task<DiceNotationResult> EvaluateAsync(string notation, DiceNotationContext context);
    Task<string> TransformNotationAsync(string notation, DiceNotationContext context);
    Task<DiceNotationResult> PostProcessResultAsync(DiceNotationResult result, DiceNotationContext context);
}
```

### Plugin Integration Service

#### IPluginIntegrationService

Service for integrating plugins with the DiceRoller core functionality.

```csharp
namespace DiceRoller.Bot.Services

public interface IPluginIntegrationService
{
    /// <summary>
    /// Gets all loaded plugins
    /// </summary>
    IReadOnlyDictionary<string, LoadedPlugin> LoadedPlugins { get; }

    /// <summary>
    /// Gets all available plugins (loaded and unloaded)
    /// </summary>
    IReadOnlyDictionary<string, PluginMetadata> AvailablePlugins { get; }

    /// <summary>
    /// Event raised when plugins are loaded or reloaded
    /// </summary>
    event EventHandler<PluginIntegrationEventArgs>? PluginsChanged;

    /// <summary>
    /// Initialize the plugin integration service
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load plugins from the plugins directory and register them with core services
    /// </summary>
    Task LoadPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reload all plugins
    /// </summary>
    Task ReloadPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load a specific plugin by ID
    /// </summary>
    Task<bool> LoadPluginAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unload a specific plugin by ID
    /// </summary>
    Task<bool> UnloadPluginAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed information about loaded plugins
    /// </summary>
    IEnumerable<PluginInfo> GetPluginInfo();

    /// <summary>
    /// Get plugin health status
    /// </summary>
    PluginHealthStatus? GetPluginHealth(string pluginId);

    /// <summary>
    /// Check if the service is properly initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Register help content for a plugin command
    /// </summary>
    void RegisterPluginHelp(string commandName, string helpContent, string pluginId);

    /// <summary>
    /// Unregister all help content for a plugin
    /// </summary>
    void UnregisterPluginHelp(string pluginId);
}
```

**Usage Example:**
```csharp
var pluginService = serviceProvider.GetService<IPluginIntegrationService>();

// Initialize the service
await pluginService.InitializeAsync();

// Load all plugins
await pluginService.LoadPluginsAsync();

// Get plugin information
var plugins = pluginService.GetPluginInfo();
foreach (var plugin in plugins)
{
    Console.WriteLine($"Plugin: {plugin.Name} v{plugin.Version} - Status: {plugin.Status}");
}

// Monitor plugin changes
pluginService.PluginsChanged += (sender, args) =>
{
    Console.WriteLine($"Plugin {args.PluginId} {args.Action}");
};

// Check plugin health
var health = pluginService.GetPluginHealth("coc.plugin");
if (health != null && health.IsHealthy)
{
    Console.WriteLine("Plugin is healthy");
}
```

### Plugin Development Workflow

#### Step 1: Create Plugin Project

1. Create a new .NET class library project
2. Reference `DiceRoller.Plugins.Infrastructure`
3. Create a `Plugin.manifest.json` file

**Plugin.manifest.json:**
```json
{
  "id": "mygame.plugin",
  "name": "My Game System",
  "version": "1.0.0",
  "description": "Custom game system plugin",
  "author": "Your Name",
  "minimumHostVersion": "2.0.0",
  "dependencies": [],
  "tags": ["rpg", "custom"]
}
```

#### Step 2: Implement Plugin Interface

```csharp
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

[Plugin("mygame", "My Game System", "1.0.0",
    Description = "Custom game system with unique mechanics",
    Author = "Your Name")]
public class MyGamePlugin : PluginBase, IGameSystemPlugin
{
    // Implementation details...
}
```

#### Step 3: Implement Game System Commands

```csharp
public async Task<GameSystemCommandResult> ExecuteCommandAsync(
    string commandName, Dictionary<string, object> parameters, GameSystemContext context)
{
    return commandName.ToLowerInvariant() switch
    {
        "skill" => await ExecuteSkillCheck(parameters, context),
        "attack" => await ExecuteAttackRoll(parameters, context),
        _ => new GameSystemCommandResult(false, null, string.Empty, $"Unknown command: {commandName}")
    };
}

private async Task<GameSystemCommandResult> ExecuteSkillCheck(
    Dictionary<string, object> parameters, GameSystemContext context)
{
    var skillValue = (int)parameters["skill_value"];
    var roll = context.RandomNumberGenerator.Next(1, 101);
    var success = roll <= skillValue;
    
    var result = $"Skill Check: {roll} vs {skillValue} - {(success ? "Success" : "Failure")}";
    return new GameSystemCommandResult(true, success ? 1 : 0, result);
}
```

#### Step 4: Plugin Directory Structure

```
MyGamePlugin/
├── MyGamePlugin.cs          # Main plugin class
├── Plugin.manifest.json     # Plugin metadata
├── Commands/               # Command implementations
│   ├── SkillCommand.cs
│   └── AttackCommand.cs
├── Tables/                 # Lookup tables
│   ├── CriticalHitsTable.cs
│   └── FumbleTable.cs
├── GameSystem/             # Game system logic
│   └── MyGameSystem.cs
└── README.md              # Plugin documentation
```

### Complete Plugin Example

Here's a complete, working plugin based on the Call of Cthulhu plugin:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

[Plugin("callofcthulhu", "Call of Cthulhu Game System", "1.0.0",
    Description = "Provides Call of Cthulhu RPG game system support including sanity checks, luck rolls, and madness tables.",
    Author = "DiceRoller.Net Team",
    Tags = new[] { "rpg", "cthulhu", "horror", "chaosium" })]
[GameSystemPlugin("Call of Cthulhu", GameSystemVersion = "7th Edition", Publisher = "Chaosium Inc.")]
public class CallOfCthulhuPlugin : PluginBase, IGameSystemPlugin
{
    private CallOfCthulhuGameSystem? _gameSystem;
    private readonly List<GameSystemCommand> _commands = new();
    private readonly List<GameSystemTable> _tables = new();

    public override string Id => "callofcthulhu";
    public override string Name => "Call of Cthulhu Game System";
    public override Version Version => new(1, 0, 0);
    public override string Description => "Provides Call of Cthulhu RPG game system support.";
    public override string Author => "DiceRoller.Net Team";

    public string GameSystemName => "Call of Cthulhu";
    public string GameSystemVersion => "7th Edition";
    public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();
    public IReadOnlyList<GameSystemTable> Tables => _tables.AsReadOnly();

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Initializing Call of Cthulhu game system plugin");

        try
        {
            // Initialize game system
            _gameSystem = new CallOfCthulhuGameSystem(Logger);

            // Register commands
            RegisterCommands();

            // Register tables
            RegisterTables();

            Logger.LogInformation("Call of Cthulhu plugin initialized with {CommandCount} commands and {TableCount} tables", 
                _commands.Count, _tables.Count);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize Call of Cthulhu plugin");
            throw;
        }
    }

    public async Task<GameSystemCommandResult> ExecuteCommandAsync(string commandName, 
        Dictionary<string, object> parameters, 
        GameSystemContext context)
    {
        if (_gameSystem == null)
            throw new InvalidOperationException("Game system not initialized");

        try
        {
            Logger.LogDebug("Executing Call of Cthulhu command: {CommandName}", commandName);

            return commandName.ToLowerInvariant() switch
            {
                "sanity" => await _gameSystem.SanityCommand.ExecuteAsync(parameters, context),
                "luck" => await _gameSystem.LuckCommand.ExecuteAsync(parameters, context),
                "madness" => await _gameSystem.MadnessCommand.ExecuteAsync(parameters, context),
                "fear" => await _gameSystem.FearCommand.ExecuteAsync(parameters, context),
                _ => new GameSystemCommandResult(false, null, string.Empty, $"Unknown command: {commandName}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing Call of Cthulhu command: {CommandName}", commandName);
            return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
        }
    }

    public async Task<GameSystemTableResult> LookupTableAsync(string tableName, 
        object key, 
        GameSystemContext context)
    {
        if (_gameSystem == null)
            throw new InvalidOperationException("Game system not initialized");

        try
        {
            return tableName.ToLowerInvariant() switch
            {
                "sanityloss" or "sanity_loss" => await _gameSystem.SanityLossTable.LookupAsync(key, context),
                "madness" or "temporary_madness" => await _gameSystem.MadnessTable.LookupAsync(key, context),
                _ => new GameSystemTableResult(false, null, string.Empty, $"Unknown table: {tableName}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error looking up Call of Cthulhu table: {TableName}", tableName);
            return new GameSystemTableResult(false, null, string.Empty, ex.Message);
        }
    }

    public GameSystemHelpInfo GetHelp(string? commandName = null)
    {
        var commandHelps = new List<CommandHelpInfo>();
        
        // Add command help information
        commandHelps.Add(new CommandHelpInfo(
            "sanity",
            "Perform a sanity check with optional loss calculation",
            "sanity <current_sanity> [sanity_loss] [--roll_for_loss]",
            "sanity 65 - Roll sanity check\nsanity 65 5 - Apply 5 sanity loss",
            new List<ParameterHelpInfo>
            {
                new("current_sanity", "int", true, "Current sanity value", null),
                new("sanity_loss", "string", false, "Sanity loss amount or dice expression", "0")
            }));

        return new GameSystemHelpInfo(
            GameSystemName,
            "Call of Cthulhu 7th Edition game system providing sanity mechanics.",
            commandHelps,
            new List<TableHelpInfo>());
    }

    private void RegisterCommands()
    {
        _commands.Add(new GameSystemCommand(
            "sanity",
            "Perform a sanity check with optional loss calculation",
            new List<GameSystemParameter>
            {
                new("current_sanity", typeof(int), true),
                new("sanity_loss", typeof(string), false, "0")
            },
            "Sanity checks determine if an investigator loses sanity when confronting the unknown.",
            true));
    }

    private void RegisterTables()
    {
        _tables.Add(new GameSystemTable(
            "sanityloss",
            "Sanity loss amounts for various threats",
            typeof(string),
            typeof(string),
            "Provides sanity loss amounts based on threat severity."));
    }

    protected override PluginHealthStatus? OnGetHealthStatus()
    {
        var isHealthy = _gameSystem != null && Status == PluginStatus.Running;
        
        return new PluginHealthStatus(
            isHealthy,
            isHealthy ? "Running" : "Not initialized",
            null,
            new Dictionary<string, object>
            {
                ["gameSystemLoaded"] = _gameSystem != null,
                ["commandCount"] = _commands.Count,
                ["tableCount"] = _tables.Count
            });
    }

    protected override void OnDispose()
    {
        _gameSystem = null;
        _commands.Clear();
        _tables.Clear();
    }
}
```

---

## Formatting APIs

### Progressive Disclosure System

#### IAdaptiveBreakdownFormatter

Interface for adaptive complexity-based formatting.

```csharp
namespace DiceRoller.Core.Formatting

public interface IAdaptiveBreakdownFormatter
{
    /// <summary>
    /// Formats a dice expression breakdown based on its TypedResult structure
    /// </summary>
    string FormatBreakdown(TypedResult typedResult, string breakdown, string originalExpression);
}
```

### Complexity Analysis

#### IComplexityAnalyzer

Interface for analyzing expression complexity.

```csharp
namespace DiceRoller.Core.Formatting

public interface IComplexityAnalyzer
{
    /// <summary>
    /// Analyzes the complexity of a TypedResult structure
    /// </summary>
    ComplexityLevel AnalyzeComplexity(TypedResult typedResult);
}
```

**Complexity Levels:**
```csharp
public enum ComplexityLevel
{
    Simple,    // Basic dice rolls (d20, 3d6+2)
    Moderate,  // Functions or comparisons (min(3d6, 15))
    Complex,   // Multiple operations (3d6+2d8+5)
    VeryComplex, // Nested functions (repeat(min(3d6!, 18), 3))
    Extreme    // Highly nested or repeated expressions
}
```

**Usage Example:**
```csharp
var analyzer = serviceProvider.GetService<IComplexityAnalyzer>();
var formatter = serviceProvider.GetService<IAdaptiveBreakdownFormatter>();

var complexity = analyzer.AnalyzeComplexity(result.TypedResult);
var formattedBreakdown = formatter.FormatBreakdown(
    result.TypedResult, result.Breakdown, originalExpression);
```

---

## Discord Integration APIs

### IDiscordApi

Interface for Discord bot lifecycle management.

```csharp
namespace DiscordRollerBot

public interface IDiscordApi
{
    Task<bool> Start();
    Task<bool> Stop();
    DiscordApiStatus State { get; }
}
```

### ICommandProcessor

Interface for processing Discord commands.

```csharp
namespace DiscordRollerBot

public interface ICommandProcessor
{
    string Prefix { get; }
    (bool, TypedResult) ProcessTyped(string userId, string commandText);
}
```

---

## Testing Patterns and Utilities

### DiceHelper Utilities

The `DiceHelper` class provides essential utilities for deterministic testing of dice-related functionality.

```csharp
namespace DiceRoller.Test.Helpers

public static class DiceHelper
{
    /// <summary>
    /// Creates a mock random number generator that returns a predetermined sequence
    /// </summary>
    public static IRandomNumberGenerator GetSequenceGenerator(params int[] sequence)

    /// <summary>
    /// Creates a mock random number generator that always returns the same value
    /// </summary>
    public static IRandomNumberGenerator GetMoqGeneratorForStaticResult(int alwaysReturn)

    /// <summary>
    /// Creates a test adaptive formatter manager for testing formatting
    /// </summary>
    public static AdaptiveFormatterManager CreateTestAdaptiveFormatterManager()

    /// <summary>
    /// Creates a test Discord formatter with specified context and format
    /// </summary>
    public static DiscordFormatter CreateTestDiscordFormatter(FormattingContext context, ResultFormat format)
}
```

### Test Fixture Patterns

#### Basic Test Fixture Structure

```csharp
using NUnit.Framework;
using DiceRoller.Test.Helpers;
using DiceRoller.Core.Parser;
using DiceRoller.Core.Functions;

[TestFixture]
public class CustomFunctionFixture
{
    private DiceRollEvaluator _evaluator;
    private IFunctionRegistry _functionRegistry;
    private IRandomNumberGenerator _rng;

    [SetUp]
    public void Setup()
    {
        _rng = DiceHelper.GetSequenceGenerator(6, 6, 6); // Deterministic results
        _functionRegistry = new FunctionRegistry();
        _evaluator = new DiceRollEvaluator(_rng, _functionRegistry, null);
        
        // Register test functions
        _functionRegistry.RegisterFunction("test", new TestFunction());
    }

    [TestCase("test(3d6)", 18, "test(3d6) = 18")]
    [TestCase("test(d20)", 6, "test(d20) = 6")]
    public void TestFunction_WithValidInput_ReturnsExpectedResult(string expression, float expectedValue, string expectedBreakdown)
    {
        // Act
        var result = _evaluator.EvaluateExpression(expression);

        // Assert
        Assert.That(result.Value, Is.EqualTo(expectedValue));
        Assert.That(result.Breakdown, Is.EqualTo(expectedBreakdown));
    }
}
```

#### Game System Test Fixture

```csharp
[TestFixture]
public class GameSystemFixture
{
    private IGameSystemRouter _router;
    private TestGameSystem _gameSystem;

    [SetUp]
    public void Setup()
    {
        _router = new GameSystemRouter();
        _gameSystem = new TestGameSystem();
        _router.RegisterSystem(_gameSystem);
    }

    [TestCase("skill", new[] { "75" }, "Skill check: 75")]
    [TestCase("attack", new[] { "12" }, "Attack roll: 12")]
    public void GameSystem_ProcessCommand_ReturnsExpectedResult(
        string command, string[] parameters, string expectedResult)
    {
        // Act
        var result = _router.RouteCommand(command, parameters, "testuser");

        // Assert
        Assert.That(result.Breakdown, Does.Contain(expectedResult));
    }
}
```

### Deterministic Testing

#### Using Sequence Generators

```csharp
[Test]
public void DiceRoll_WithSequenceGenerator_ProducesExpectedSequence()
{
    // Arrange - Create a generator that returns 1, 6, 3, 5 in sequence
    var rng = DiceHelper.GetSequenceGenerator(1, 6, 3, 5);
    var evaluator = new DiceRollEvaluator(rng, new FunctionRegistry(), null);

    // Act - Roll 4d6
    var result = evaluator.EvaluateExpression("4d6");

    // Assert
    Assert.That(result.Value, Is.EqualTo(15)); // 1 + 6 + 3 + 5
    Assert.That(result.Breakdown, Is.EqualTo("4d6: [1, 6, 3, 5] = 15"));
}
```

#### Testing Complex Expressions

```csharp
[Test]
public void ComplexExpression_WithDeterministicRng_ProducesExpectedResult()
{
    // Arrange - Sequence for min(3d6!, 18) with exploding dice
    // First 3d6: 6(explodes), 4, 6(explodes), 2, 5 = 23
    // min(23, 18) = 18
    var rng = DiceHelper.GetSequenceGenerator(6, 4, 6, 2, 5);
    var evaluator = new DiceRollEvaluator(rng, CreateFunctionRegistry(), null);

    // Act
    var result = evaluator.EvaluateExpression("min(3d6!, 18)");

    // Assert
    Assert.That(result.Value, Is.EqualTo(18));
    Assert.That(result.Breakdown, Does.Contain("min("));
    Assert.That(result.Breakdown, Does.Contain("= 18"));
}
```

#### Testing Error Conditions

```csharp
[Test]
public void InvalidExpression_ThrowsExpectedException()
{
    // Arrange
    var rng = DiceHelper.GetMoqGeneratorForStaticResult(10);
    var evaluator = new DiceRollEvaluator(rng, new FunctionRegistry(), null);

    // Act & Assert
    Assert.Throws<ParseException>(() => evaluator.EvaluateExpression("invalid_expression"));
}

[Test]
public void DivisionByZero_ReturnsErrorResult()
{
    // Arrange
    var rng = DiceHelper.GetMoqGeneratorForStaticResult(0);
    var evaluator = new DiceRollEvaluator(rng, new FunctionRegistry(), null);

    // Act
    var result = evaluator.EvaluateExpression("1d6 / 0");

    // Assert
    Assert.That(result.Value, Is.EqualTo(float.PositiveInfinity));
    Assert.That(result.Breakdown, Does.Contain("∞"));
}
```

#### Performance Testing

```csharp
[Test]
public void ComplexExpression_PerformanceTest()
{
    // Arrange
    var rng = DiceHelper.GetMoqGeneratorForStaticResult(10);
    var evaluator = new DiceRollEvaluator(rng, CreateFunctionRegistry(), null);
    var complexExpression = "repeat(min(3d6!+2, 20), 100) + max(10d10!, 50)";

    // Act
    var stopwatch = Stopwatch.StartNew();
    var result = evaluator.EvaluateExpression(complexExpression);
    stopwatch.Stop();

    // Assert
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Should complete in under 1 second
    Assert.That(result.Value, Is.GreaterThan(0));
}
```

#### Testing Formatting

```csharp
[Test]
public void ComplexExpression_FormattingTest()
{
    // Arrange
    var rng = DiceHelper.GetSequenceGenerator(6, 5, 4, 3, 2, 1);
    var evaluator = new DiceRollEvaluator(rng, CreateFunctionRegistry(), null);
    var formatter = DiceHelper.CreateTestAdaptiveFormatterManager();

    // Act
    var result = evaluator.EvaluateExpression("repeat(2d6, 3)");
    var formattedBreakdown = formatter.FormatBreakdown(result.TypedResult, result.Breakdown, "repeat(2d6, 3)");

    // Assert
    Assert.That(formattedBreakdown, Does.Contain("repeat("));
    Assert.That(formattedBreakdown, Does.Contain("├─"));
    Assert.That(formattedBreakdown, Does.Contain("└─"));
}
```

---

## Configuration and Dependency Injection

### Service Registration Patterns

The DiceRoller.Net system uses a comprehensive dependency injection pattern. Here's how services are registered:

#### Core Services Registration

```csharp
public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Bot Configuration
    services.Configure<BotConfiguration>(configuration.GetSection("Bot"));
    services.Configure<PluginConfiguration>(configuration.GetSection("Plugins"));
    services.Configure<SecurityConfiguration>(configuration.GetSection("Security"));

    // Core dice rolling services
    services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
    services.AddSingleton<IDiceExpressionParser, DiceExpressionParser>();
    services.AddScoped<IDiceRollingService, DiceRollingService>();

    // Function Registry with built-in functions
    services.AddSingleton<IFunctionRegistry>(provider =>
    {
        var registry = new FunctionRegistry();
        foreach (var function in BuiltInFunctions.GetBuiltInFunctions())
        {
            registry.RegisterFunction(function.Name, function);
        }
        return registry;
    });

    // Game System Router
    services.AddSingleton<IGameSystemRouter>(provider =>
    {
        var router = new GameSystemRouter();
        var evaluator = provider.GetRequiredService<IDiceExpressionEvaluator>();
        
        // Register built-in game systems
        var dqTables = provider.GetRequiredService<DQLookupTables>();
        var dragonQuestSystem = new DragonQuestGameSystem(evaluator, dqTables);
        router.RegisterSystem(dragonQuestSystem);
        
        return router;
    });

    // Formatting services
    services.AddSingleton<IComplexityAnalyzer, ComplexityAnalyzer>();
    services.AddSingleton<IAdaptiveBreakdownFormatter, EnhancedTextFormatter>();
    services.AddSingleton<AdaptiveFormatterManager>();

    // Help services
    services.AddSingleton<ICommandHelpService, CommandHelpService>();

    // Plugin services
    services.AddSingleton<IPluginIntegrationService, PluginIntegrationService>();
    services.AddSingleton<PluginManager>();
    services.AddSingleton<PluginDiscovery>();
    services.AddSingleton<PluginLoader>();

    // Discord services
    services.AddSingleton<IDiscordApi, DiscordApi>();
    services.AddSingleton<InteractionHandler>();

    // Command processors
    services.AddSingleton<ICommandProcessor, RollCommandProcessor>();
    services.AddSingleton<ICommandProcessor, DQCommandProcessor>();
    services.AddSingleton<ICommandProcessor, PluginCommandProcessor>();

    // Hosted services
    services.AddHostedService<BotHost>();
    services.AddHostedService<PluginStartupService>();
    services.AddHostedService<SlashCommandRegistrationService>();
}
```

### Configuration Management

#### Configuration Structure

```json
{
  "Bot": {
    "Token": "your-discord-bot-token",
    "CommandPrefix": "!roll",
    "EnableSlashCommands": true,
    "EnablePrefixCommands": false,
    "EnableButtonInteractions": true
  },
  "Logging": {
    "MinimumLevel": "Information",
    "Categories": {
      "DSharpPlus": "Warning",
      "DiceRoller.Plugins": "Debug",
      "DiceRoller.Core": "Information"
    }
  },
  "Plugins": {
    "PluginsDirectory": "plugins",
    "AutoLoadPlugins": true,
    "EnableSandboxing": true,
    "MaxPluginLoadTime": "00:00:30",
    "EnablePerformanceMonitoring": true
  },
  "Security": {
    "EnableSandboxing": true,
    "AllowedApiCalls": ["System.Random", "System.Math"],
    "MaxMemoryUsage": "100MB",
    "MaxExecutionTime": "00:00:05"
  }
}
```

#### Configuration Classes

```csharp
public class BotConfiguration
{
    public string Token { get; set; } = string.Empty;
    public string CommandPrefix { get; set; } = "!roll";
    public bool EnableSlashCommands { get; set; } = true;
    public bool EnablePrefixCommands { get; set; } = false;
    public bool EnableButtonInteractions { get; set; } = true;
}

public class PluginConfiguration
{
    public string PluginsDirectory { get; set; } = "plugins";
    public bool AutoLoadPlugins { get; set; } = true;
    public bool EnableSandboxing { get; set; } = true;
    public TimeSpan MaxPluginLoadTime { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnablePerformanceMonitoring { get; set; } = true;
}

public class SecurityConfiguration
{
    public bool EnableSandboxing { get; set; } = true;
    public List<string> AllowedApiCalls { get; set; } = new();
    public string MaxMemoryUsage { get; set; } = "100MB";
    public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.FromSeconds(5);
}
```

#### Using Configuration in Services

```csharp
public class PluginIntegrationService : IPluginIntegrationService
{
    private readonly IOptions<PluginConfiguration> _pluginConfig;
    private readonly IOptions<SecurityConfiguration> _securityConfig;

    public PluginIntegrationService(
        IOptions<PluginConfiguration> pluginConfig,
        IOptions<SecurityConfiguration> securityConfig)
    {
        _pluginConfig = pluginConfig;
        _securityConfig = securityConfig;
    }

    public async Task LoadPluginsAsync(CancellationToken cancellationToken = default)
    {
        var pluginsDirectory = _pluginConfig.Value.PluginsDirectory;
        var maxLoadTime = _pluginConfig.Value.MaxPluginLoadTime;
        
        // Use configuration values...
    }
}
```

### Circular Dependency Resolution

The system uses several patterns to resolve circular dependencies:

#### Lazy Initialization Pattern

```csharp
// Problem: IDiceExpressionEvaluator needs IGameSystemRouter, but IGameSystemRouter needs IDiceExpressionEvaluator

// Solution: Use lazy initialization
services.AddSingleton<IDiceExpressionEvaluator>(provider =>
{
    var rng = provider.GetRequiredService<IRandomNumberGenerator>();
    var parser = provider.GetRequiredService<IDiceExpressionParser>();
    var functionRegistry = provider.GetRequiredService<IFunctionRegistry>();
    
    // Create evaluator without router initially
    var evaluator = new DiceExpressionEvaluator(rng, parser, functionRegistry, null);
    
    // Router will be injected later through visitor pattern
    return evaluator;
});

services.AddSingleton<IGameSystemRouter>(provider =>
{
    var router = new GameSystemRouter();
    var evaluator = provider.GetRequiredService<IDiceExpressionEvaluator>();
    
    // Register game systems that need the evaluator
    var dragonQuestSystem = new DragonQuestGameSystem(evaluator, /* other deps */);
    router.RegisterSystem(dragonQuestSystem);
    
    return router;
});
```

#### Factory Pattern for Complex Dependencies

```csharp
public interface IEvaluatorFactory
{
    IDiceExpressionEvaluator CreateEvaluator(IGameSystemRouter? router = null);
}

public class EvaluatorFactory : IEvaluatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EvaluatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDiceExpressionEvaluator CreateEvaluator(IGameSystemRouter? router = null)
    {
        var rng = _serviceProvider.GetRequiredService<IRandomNumberGenerator>();
        var parser = _serviceProvider.GetRequiredService<IDiceExpressionParser>();
        var functionRegistry = _serviceProvider.GetRequiredService<IFunctionRegistry>();
        
        return new DiceExpressionEvaluator(rng, parser, functionRegistry, router);
    }
}

// Registration
services.AddSingleton<IEvaluatorFactory, EvaluatorFactory>();
```

#### Service Locator Pattern for Runtime Dependencies

```csharp
public class DiceRollVisitor
{
    private readonly IServiceProvider _serviceProvider;
    private IGameSystemRouter? _gameSystemRouter;

    public DiceRollVisitor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IGameSystemRouter GameSystemRouter =>
        _gameSystemRouter ??= _serviceProvider.GetRequiredService<IGameSystemRouter>();

    public DiceResultNode VisitGameCommand(string command, string[] parameters)
    {
        return GameSystemRouter.RouteCommand(command, parameters);
    }
}
```

---

## Usage Examples

### Basic Dice Rolling

```csharp
// Service setup
var serviceProvider = ConfigureServices();
var parser = serviceProvider.GetService<IDiceExpressionParser>();
var evaluator = serviceProvider.GetService<IDiceExpressionEvaluator>();

// Parse and evaluate expression
var parseTree = parser.Parse("3d6 + 2d8 + 5");
var result = evaluator.Evaluate(parseTree);

Console.WriteLine($"Result: {result.Value}");
Console.WriteLine($"Breakdown: {result.Breakdown}");
```

### Custom Function Registration

```csharp
public class AdvantageFunction : IDiceFunction
{
    public string Name => "advantage";

    public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
    {
        // Roll 2d20, take higher
        var roll1 = context.RandomNumberGenerator.GetRandomNumber(1, 20);
        var roll2 = context.RandomNumberGenerator.GetRandomNumber(1, 20);
        var result = Math.Max(roll1, roll2);
        
        return new DiceResultNode(result, 
            $"advantage: max({roll1}, {roll2}) = {result}");
    }
}

// Register the function
var registry = serviceProvider.GetService<IFunctionRegistry>();
registry.RegisterFunction("advantage", new AdvantageFunction());

// Use in expression
var result = evaluator.Evaluate(parser.Parse("advantage() + 5"));
```

### Game System Integration

```csharp
public class D20System : IGameSystem
{
    public string Name => "D20";
    public IEnumerable<string> SupportedCommands => new[] { "skill", "save", "attack" };

    public bool SupportsCommand(string commandType) => 
        SupportedCommands.Contains(commandType);

    public TypedResult ProcessCommand(string commandType, string[] parameters, string userId)
    {
        var roll = new Random().Next(1, 21);
        var modifier = parameters.Length > 0 ? int.Parse(parameters[0]) : 0;
        var total = roll + modifier;

        return TypedResult.NewSimpleResult(
            $"{commandType}: d20({roll}) + {modifier} = {total}");
    }
}

// Register with router
var router = serviceProvider.GetService<IGameSystemRouter>();
router.RegisterSystem(new D20System());

// Use game system command
var result = router.RouteCommand("skill", new[] { "5" }, "user123");
```

---

## Extension Points

### Key Extension Interfaces

1. **IDiceFunction** - Add custom functions to dice notation
2. **IGameSystem** - Implement game-specific commands and tables
3. **IPlugin** - Create modular extensions with full lifecycle management
4. **IAdaptiveBreakdownFormatter** - Customize result formatting
5. **IComplexityAnalyzer** - Modify complexity analysis logic

### Plugin Integration Points

- **Function Registry**: Register custom functions via plugins
- **Game System Router**: Add game systems through plugin loading
- **Discord Commands**: Extend bot commands through plugin system
- **Formatting Pipeline**: Custom formatters for specialized output

---

## Best Practices

### Function Development

1. **Deterministic Testing**: Use `IRandomNumberGenerator` for mockable randomness
2. **Error Handling**: Return meaningful error messages in breakdowns
3. **Performance**: Avoid expensive operations in frequently called functions
4. **Documentation**: Provide clear function descriptions and examples

### Game System Development

1. **Command Naming**: Use consistent, intuitive command names
2. **Parameter Validation**: Validate all input parameters before processing
3. **User Context**: Leverage userId for personalized behavior
4. **Error Messages**: Provide helpful error messages for invalid commands

### Plugin Development

1. **Lifecycle Management**: Properly implement async lifecycle methods
2. **Resource Cleanup**: Dispose resources in Dispose() method
3. **Health Monitoring**: Implement meaningful health status reporting
4. **Configuration**: Support configuration changes without restart

### API Stability

- **Core Interfaces**: Marked as stable - breaking changes will be versioned
- **Plugin Interfaces**: May evolve with backward compatibility when possible
- **Internal APIs**: Subject to change without notice
- **Extension Points**: Designed for forward compatibility

### Error Handling

```csharp
public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
{
    try
    {
        // Function logic here
        return new DiceResultNode(result, breakdown);
    }
    catch (Exception ex)
    {
        return new DiceResultNode(0, $"Error in {Name}: {ex.Message}");
    }
}
```

### Dependency Injection

All major interfaces are registered in the DI container:

```csharp
services.AddScoped<IDiceExpressionParser, DiceExpressionParser>();
services.AddScoped<IDiceExpressionEvaluator, DiceExpressionEvaluator>();
services.AddSingleton<IFunctionRegistry, FunctionRegistry>();
services.AddSingleton<IGameSystemRouter, GameSystemRouter>();
services.AddScoped<IComplexityAnalyzer, ComplexityAnalyzer>();
services.AddScoped<IAdaptiveBreakdownFormatter, AdaptiveFormatterManager>();
```

---

## API Versioning and Compatibility

- **Current Version**: 2.0 (January 2025)
- **Compatibility Policy**: Semantic versioning for public APIs
- **Breaking Changes**: Major version increments only
- **Deprecated APIs**: Marked with `[Obsolete]` attribute with migration guidance

For more information, see:
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture details
- [DICE_SYSTEM_REFACTORING_OPTIONS.md](DICE_SYSTEM_REFACTORING_OPTIONS.md) - Extension patterns
- Plugin examples in `src/DiceRoller.Plugins.CallOfCthulhu/`