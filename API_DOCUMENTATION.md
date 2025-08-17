# DiceRoller.Net API Documentation

## Overview

DiceRoller.Net provides a comprehensive set of APIs for dice rolling, expression parsing, game system integration, and plugin development. This document covers all major APIs and interfaces available for developers working with or extending the system.

## Table of Contents

1. [Core APIs](#core-apis)
   - [Dice Expression APIs](#dice-expression-apis)
   - [Parser APIs](#parser-apis)
   - [Function Registry APIs](#function-registry-apis)
2. [Game System APIs](#game-system-apis)
   - [Game System Interface](#game-system-interface)
   - [Game System Router](#game-system-router)
3. [Plugin Development APIs](#plugin-development-apis)
   - [Base Plugin Interface](#base-plugin-interface)
   - [Game System Plugins](#game-system-plugins)
   - [Dice Notation Plugins](#dice-notation-plugins)
4. [Formatting APIs](#formatting-apis)
   - [Progressive Disclosure System](#progressive-disclosure-system)
   - [Complexity Analysis](#complexity-analysis)
5. [Discord Integration APIs](#discord-integration-apis)
6. [Usage Examples](#usage-examples)
7. [Extension Points](#extension-points)
8. [Best Practices](#best-practices)

---

## Core APIs

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