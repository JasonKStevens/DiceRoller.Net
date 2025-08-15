# DiceRoller.Net Architecture

## Overview

DiceRoller.Net is a Discord-only dice rolling bot built on .NET 8.0 using the Irony language kit for parsing dice notation expressions. The system features a sophisticated Progressive Disclosure system that adapts output formatting based on expression complexity.

## Project Structure (8 Core Projects)

### 1. **DiceRoller.Core** - Core Library
Contains all dice rolling logic, expression parsing, and the Progressive Disclosure formatting system.

**Key Components:**
- `Parser/` - Irony-based grammar and expression evaluation
  - `ExpressionGrammar.cs` - Dice notation grammar definition
  - `DiceRollEvaluator.cs` - Main evaluation entry point
  - `DiceRollVisitor.cs` - AST visitor pattern implementation
- `Functions/` - Dynamic function registry system
  - `IFunctionRegistry.cs` - Interface for function registration and execution
  - `IDiceFunction.cs` - Interface for individual function implementations
  - `FunctionRegistry.cs` - Registry implementation with built-in functions
  - `BuiltInFunctions.cs` - Core functions (min, max, repeat)
- `Routing/` - Game system command routing
  - `IGameSystemRouter.cs` - Interface for routing game commands
  - `GameSystemRouter.cs` - Router implementation with type conversion
- `Abstractions/` - Core interfaces and contracts
  - `IGameSystem.cs` - Interface for game system implementations
- `Formatting/` - Progressive Disclosure system
  - `DiscordFormatter.cs` - Discord-specific formatting
  - `ResultNodeConverter.cs` - TypedResult to DiceResultNode conversion
- `Dice/` - Random number generation abstraction
- `DragonQuest/` & `Heroes/` - Game-specific logic

### 2. **DiceRoller.Bot** - Discord Bot Application
Main entry point and host for the Discord bot.
- Program startup and dependency injection
- Health check endpoints for container deployment
- Command processors coordination

### 3. **DiceRoller.Discord** - Discord Integration
Discord-specific functionality and slash commands.
- Slash command implementations
- Progressive Disclosure Discord adapter
- Interactive button handlers
- Discord-specific output formatting

### 4. **DiceRoller.GameSystems** - Game Systems
Extensible framework for game-specific mechanics.
- DragonQuest system (injury, backfire, fear tables)
- Heroes system (speed, location tables)
- Base classes for adding new systems

### 5. **PartyDSL** - Party Management
Domain-specific language for managing game parties and characters.
- Custom parser for party commands
- Character and party state management

### 6. **DiceRoller.Plugins.Infrastructure** - Plugin Framework
Infrastructure for extending functionality through plugins.
- Plugin loading and lifecycle management
- Security sandboxing
- Resource monitoring
- Plugin integration service for bridging to core systems

### 7. **DiceRoller.Plugins.CallOfCthulhu** - Example Plugin
Demonstrates plugin implementation for the Call of Cthulhu game system.
- Shows function registration via IPluginFunctionProvider
- Shows game system registration via IPluginGameSystemProvider
- Example of plugin metadata and lifecycle management

### 8. **Test Projects**
- `DiceRoller.Core.Tests` - Comprehensive unit tests (323+ tests)
- `PartyDSL.Tests` - Party management tests

## Progressive Disclosure System

The Progressive Disclosure system intelligently adapts dice roll output based on complexity, providing the right amount of detail for each situation.

### Verbosity Levels

1. **Terse** - Minimal output (e.g., "18")
2. **Standard** - Balanced information (e.g., "🎲 **18** ([4, 6, 3] + 5)")
3. **Detailed** - Full breakdown with all components
4. **Narrative** - Story-style presentation
5. **Technical** - Debug information with AST

### Adaptive Formatting System

A sophisticated complexity analysis and formatting system transforms unreadable complex expression output into hierarchical, easy-to-understand structures.

#### Complexity Analysis
- **5 Complexity Levels**: Simple, Moderate, Complex, VeryComplex, Extreme
- **Configurable Thresholds**: Adjustable scoring system (default: 3, 6, 10, 15)
- **Pattern Recognition**: Detects exploding dice, comparisons, large dice pools, nested operations
- **Depth Analysis**: Considers nesting levels and sub-node complexity

#### Enhanced Text Formatting
- **Tree Structure**: Uses Unicode tree symbols (├─, └─, │) for hierarchical display
- **Node-Type Specific**: Custom formatting for Repeat, Min, DiceRoll, Operator, Lookup, StepFunc
- **Intelligent Fallback**: Graceful degradation when TypedResult unavailable
- **Before/After Example**:
  ```
  Before: "55 12 12 10 1 5 4 + 11 11 9 1 3 5 +"
  After:
  🎲 repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10
  ├─ repeat(min(3d6!+2, 20), 4)
  │  ├─ Iteration 1: min([5, 5, 2]! + 2, 20) = 20
  │  ├─ Iteration 2: min([1, 2, 1] + 2, 20) = 6
  │  └─ Total: 26
  └─ (4d10! > 30) * 10
     ├─ 4d10!: [1, 1, 9, 1, 3, 5] = 20
     ├─ 20 > 30 = False
     └─ False * 10 = 0
  ```

#### Complexity-Based Formatting Strategy
- **Simple**: Inline code formatting (`result`)
- **Moderate**: Code blocks with basic structure
- **Complex/VeryComplex/Extreme**: Full tree formatting with EnhancedTextFormatter
- **Automatic Selection**: AdaptiveFormatterManager chooses appropriate formatter

### Key Features

- **Automatic Complexity Detection**: Analyzes expressions to determine optimal format
- **Smart Truncation**: Keeps messages under Discord's 2000-character limit
- **Interactive Components**: Re-roll, Details, and Copy buttons
- **Discord Optimization**: Uses spoilers, markdown, and emojis appropriately
- **Hierarchical Tree Output**: Transforms complex breakdowns into readable structures

### Implementation Flow

```
User Input → Command Processor → TypedResult → ComplexityAnalyzer
→ AdaptiveFormatterManager → EnhancedTextFormatter/Fallback
→ DiscordFormatter → FormattedResult → Discord Message
```

### Game System Command Flow

```
Game Command → DiceRollVisitor → IGameSystemRouter → GameSystemRouter
→ IGameSystem.ProcessCommand → TypedResult → DiceResultNode
→ Progressive Disclosure → Discord Message
```

## Supported Dice Notation

### Basic Operations
- Simple dice: `d20`, `3d6`, `10d10`
- Arithmetic: `3d6+5`, `(2d8+3)*2`
- Comparisons: `3d6 > 10`, `d20 >= 15`

### Advanced Features
- **Exploding dice**: `3d6!` (reroll on max value)
- **Min function**: `min(d10-4, 1)` (take minimum value)
- **Max function**: `max(3d6, 10)` (take maximum value)
- **Repeat function**: `repeat(d20, 5)` (roll multiple times)
- **Step function**: `step 15` (EarthDawn system)
- **Comments**: `d20+5 # Attack roll`

### Game System Commands
- DragonQuest: `injury`, `backfire`, `fear`
- Heroes: `speed`, `hitloc [bodytype]`
- User aliases: `addalias`, `removealias`, `listalias`

## Function Registry Pattern

The Function Registry Pattern enables dynamic registration and execution of dice functions, providing extensibility while maintaining performance and type safety.

### Architecture

#### Core Interfaces

**IFunctionRegistry** - Central registry for function management
- Manages function registration and lookup
- Provides execution context for function calls
- Thread-safe implementation for concurrent access

**IDiceFunction** - Individual function implementation
- Standardized interface for all dice functions
- Receives ParseTreeNode and IFunctionContext for execution
- Returns DiceResultNode for consistent formatting

**IFunctionContext** - Execution context provider
- Provides access to DiceRollVisitor for sub-expression evaluation
- Encapsulates execution state and dependencies

### Implementation Details

#### Registry-First Pattern with Fallback
The DiceRollVisitor implements a hybrid approach:
1. **Primary**: Check function registry for custom functions
2. **Fallback**: Execute built-in hardcoded functions if not found
3. **Graceful**: Maintains backward compatibility during transition

#### Built-in Function Migration
Core functions migrated to IDiceFunction implementations:
- **MinFunction**: `min(expression, value)` - Returns minimum value
- **MaxFunction**: `max(expression, value)` - Returns maximum value  
- **RepeatFunction**: `repeat(expression, count)` - Executes expression multiple times

#### Dependency Injection Integration
- `IFunctionRegistry` registered as singleton
- `IFunctionContext` registered as scoped
- Built-in functions auto-registered during startup
- Plugin functions can be registered at runtime

### Usage Examples

#### Registering Custom Functions
```csharp
// In Program.cs or plugin initialization
services.AddSingleton<IFunctionRegistry>(provider =>
{
    var registry = new FunctionRegistry();
    registry.RegisterFunction("custom", new CustomDiceFunction());
    return registry;
});
```

#### Implementing Custom Functions
```csharp
public class CustomDiceFunction : IDiceFunction
{
    public string Name => "custom";
    
    public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
    {
        // Access parameters from node.ChildNodes
        // Use context.Visitor.Visit() for sub-expressions
        // Return DiceResultNode with result
    }
}
```

### Benefits

1. **Extensibility**: Add new functions without modifying core code
2. **Plugin Support**: Enable third-party function development
3. **Testability**: Mock individual functions for isolated testing
4. **Performance**: Lazy loading and caching of function instances
5. **Type Safety**: Compile-time validation of function implementations

## Game System Router Pattern

The Game System Router Pattern enables dynamic routing of game-specific commands to appropriate handlers, providing a clean separation between core dice logic and game system implementations.

### Architecture

#### Core Interfaces

**IGameSystemRouter** - Central routing system for game commands
- Routes commands to appropriate game system handlers
- Provides automatic type conversion from TypedResult to DiceResultNode
- Thread-safe implementation for concurrent access
- Error handling with graceful fallback

**IGameSystem** - Individual game system implementation
- Standardized interface for game system commands
- Returns TypedResult for consistent data structure
- Supports command validation and parameter handling

### Implementation Details

#### Router-First Pattern with Fallback
The DiceRollVisitor implements a hybrid approach for game commands:
1. **Primary**: Check game system router for registered handlers
2. **Fallback**: Execute hardcoded implementations if not found
3. **Graceful**: Maintains backward compatibility during transition

#### Type Conversion System
The GameSystemRouter automatically converts between type systems:
- **Input**: TypedResult from game systems (existing format)
- **Output**: DiceResultNode for core parser compatibility
- **Smart Extraction**: Handles lookup results, dice rolls, and simple values
- **Error Handling**: Converts exceptions to error result nodes

#### Dependency Injection Integration
- `IGameSystemRouter` registered as singleton
- Game systems registered as implementations of `IGameSystem`
- Router automatically discovers and registers available systems
- Plugin systems can be registered at runtime

### Usage Examples

#### Implementing Game Systems
```csharp
public class CustomGameSystem : IGameSystem
{
    public string Name => "CustomGame";
    
    public bool SupportsCommand(string commandType)
    {
        return commandType.ToLower() switch
        {
            "customroll" => true,
            "customtable" => true,
            _ => false
        };
    }
    
    public TypedResult ProcessCommand(string commandType, string[] parameters, string userId)
    {
        return commandType.ToLower() switch
        {
            "customroll" => ProcessCustomRoll(parameters),
            "customtable" => ProcessCustomTable(parameters),
            _ => throw new NotSupportedException($"Command {commandType} not supported")
        };
    }
}
```

#### Router Integration in Visitor
```csharp
private DiceResultNode TryRouteGameCommand(string commandType, string[] parameters = null, string userId = null)
{
    return _gameSystemRouter?.RouteCommand(commandType, parameters, userId);
}

// Usage in visitor
case "injury":
    return TryRouteGameCommand("injury") ?? Generate(new DragonQuest.GrievousInjuries());
```

### Benefits

1. **Separation of Concerns**: Game logic separated from core dice parsing
2. **Extensibility**: Add new game systems without modifying core code
3. **Plugin Support**: Enable third-party game system development
4. **Type Safety**: Automatic conversion between type systems
5. **Error Resilience**: Graceful handling of system failures
6. **Backward Compatibility**: Maintains existing functionality during transition
7. **Code Elimination**: Removes duplication between game systems

### Migration Strategy

The router pattern was implemented with zero breaking changes:

1. **Preserved Existing Logic**: All hardcoded commands continue to work
2. **Added Router Layer**: New router sits between visitor and game systems
3. **Graceful Fallback**: If router fails, falls back to original implementation
4. **Incremental Migration**: Game systems can be moved to router over time

## Design Patterns

1. **Visitor Pattern**: AST traversal for expression evaluation
2. **Registry Pattern**: Dynamic function registration and execution
3. **Router Pattern**: Game system command routing and type conversion
4. **Adapter Pattern**: Bridges TypedResult and Progressive Disclosure
5. **Strategy Pattern**: Multiple formatting strategies based on complexity
6. **Dependency Injection**: Testable architecture with mockable components
7. **Chain of Responsibility**: Complexity analysis flows through configurable thresholds
8. **Factory Pattern**: AdaptiveFormatterManager selects appropriate formatter implementation

## Plugin Integration Architecture

The bot features a complete plugin integration system that bridges plugin functionality to core systems:

### Core Components

#### PluginIntegrationService
- **Central Bridge**: Connects plugins to Function Registry and Game System Router
- **Automatic Discovery**: Scans loaded plugins for function and game system providers
- **Dynamic Registration**: Registers plugin functions and game systems at startup
- **Lifecycle Management**: Handles plugin reload and cleanup operations
- **Error Resilience**: Graceful handling of plugin registration failures

#### Plugin Provider Interfaces
- **IPluginFunctionProvider**: Plugins implement to provide custom dice functions
- **IPluginGameSystemProvider**: Plugins implement to provide game system commands
- **Plugin Metadata**: Version information, descriptions, and compatibility data

#### Discord Plugin Commands
- `/plugin list` - Shows all loaded plugins with status and metadata
- `/plugin info <name>` - Detailed information about specific plugin
- `/plugin reload <name>` - Reloads specific plugin (development support)

### Integration Flow

```
Plugin Load → PluginIntegrationService → Function Registry
                                      → Game System Router
                                      → Discord Commands
```

### Example: CallOfCthulhu Plugin

The CallOfCthulhu plugin demonstrates full integration capabilities:
- **Custom Functions**: `luck()`, `sanity()`, `characteristic()` functions
- **Game Commands**: `fear`, `madness`, `backstory` commands
- **Automatic Registration**: Functions and commands available immediately
- **Discord Integration**: Accessible via `/roll dice:luck()` and `/plugin` commands

### Benefits

1. **Zero Code Changes**: Plugins integrate without modifying core systems
2. **Runtime Management**: Plugins can be monitored and reloaded via Discord
3. **Type Safety**: Full integration with existing type conversion systems
4. **Error Isolation**: Plugin failures don't affect core functionality
5. **Extensibility**: Community can add new game systems and functions

## Container Deployment

The bot includes health check endpoints for container orchestration:
- `/health/live` - Liveness check (process health)
- `/health/ready` - Readiness check (Discord connection status)

Exposed on port 8080 for monitoring while Discord bot runs on standard ports.

## Adaptive Formatting Implementation Details

### Core Components

#### ComplexityAnalyzer
- **Configurable Thresholds**: Default values (Simple: 3, Moderate: 6, Complex: 10, VeryComplex: 15)
- **Node Type Scoring**: Different complexity weights for NodeType enum values
- **Pattern Detection**: Identifies exploding dice (!), comparisons (>, <, >=, <=), large dice pools (10+ dice)
- **Depth Penalty**: Considers nesting levels in expression trees
- **Sub-node Analysis**: Recursive complexity calculation with sub-node count penalties

#### EnhancedTextFormatter
- **Tree Structure Generation**: Creates hierarchical output using TreeStructureFormatter
- **Node-Specific Formatting**: Specialized formatters for Repeat, Min, DiceRoll, Operator, Lookup, StepFunc
- **Callback Architecture**: Uses NodeTypeFormatters with configurable formatting callbacks
- **Graceful Fallback**: Handles cases where TypedResult is unavailable

#### AdaptiveFormatterManager
- **Strategy Selection**: Chooses formatter based on ComplexityLevel analysis
- **Regex Compatibility**: Maintains backward compatibility with existing regex-based formatting
- **Performance Optimization**: Uses appropriate formatter complexity for each expression type

### Dependency Injection Integration

All components are registered with full DI support:
- `IComplexityAnalyzer` → `ComplexityAnalyzer`
- `IAdaptiveBreakdownFormatter` → `EnhancedTextFormatter`
- `ComplexityThresholdOptions` → Configuration-based registration
- `AdaptiveFormatterManager` → Manages formatter selection

### Configuration

```json
{
  "ComplexityThresholds": {
    "SimpleThreshold": 3,
    "ModerateThreshold": 6,
    "ComplexThreshold": 10,
    "VeryComplexThreshold": 15
  }
}
```

## Future Enhancements

### Dice System Architecture Refactoring

See [DICE_SYSTEM_REFACTORING_OPTIONS.md](DICE_SYSTEM_REFACTORING_OPTIONS.md) for comprehensive analysis and implementation plan.

**Phase 1: Function Registry Pattern** ✅ COMPLETED
- ✅ Added extension points to DiceRollVisitor for custom functions
- ✅ Created IFunctionRegistry and IDiceFunction interfaces
- ✅ Implemented built-in functions (min, max, repeat) as IDiceFunction implementations
- ✅ Updated dependency injection configuration and complexity analyzer
- ✅ All 323 core tests passing

**Phase 2: Game System Refactoring** ✅ COMPLETED
- ✅ Created IGameSystemRouter interface for command routing
- ✅ Implemented GameSystemRouter with type conversion and error handling
- ✅ Updated DiceRollVisitor with router-first fallback pattern
- ✅ Integrated with existing DragonQuestGameSystem
- ✅ All 323 tests passing with zero breaking changes
- ✅ Foundation established for plugin-based game systems

**Phase 3: Plugin Integration System** ✅ COMPLETED
- ✅ Created PluginIntegrationService bridging plugins to Function Registry and Game System Router
- ✅ Added Discord commands for plugin management (/plugin list, info, reload)
- ✅ Fixed critical issues: blocking async operations, function unregistration
- ✅ Integrated with existing plugin infrastructure
- ✅ All 323 tests passing with zero breaking changes
- ✅ Example CallOfCthulhu plugin demonstrates full integration

**Phase 4: Advanced Plugin Features** (Future)
- Enhanced plugin discovery and registration mechanisms
- Plugin template for community contributions
- Runtime plugin loading capabilities
- Advanced function metadata and documentation

### Adaptive Formatting System

Based on the recommended hybrid approach from the complex results presentation research:

### Phase 1: Enhanced Text Formatting (✅ COMPLETED)
- ✅ Adaptive complexity analysis with 5 levels
- ✅ Hierarchical tree formatting with Unicode symbols
- ✅ Configurable complexity thresholds
- ✅ Node-type specific formatters
- ✅ Full dependency injection support

### Phase 2: Interactive Tree Navigation (Planned)
- Expandable/collapsible result trees via Discord buttons
- State management for progressive revelation
- Enhanced tree navigation for extreme complexity expressions

### Phase 3: Table Format (Future)
- Structured tables for repeated operations
- Export capabilities
- Optimized Discord display

### Phase 4: Visual Graphs (Optional)
- Generate computation graph images
- SVG/PNG output as Discord attachments
- Premium feature consideration

## Performance Considerations

- **Lazy Evaluation**: Progressive Disclosure only formats requested detail levels
- **Intelligent Caching**: Recent formats cached for 15 minutes
- **Truncation Strategy**: Preserves essential information while meeting Discord limits
- **Component Limits**: Adapts button count based on result size

## Testing Strategy

- **323+ Unit Tests**: Comprehensive coverage of all dice notation features
- **Function Registry Testing**: Individual function implementations tested in isolation
- **Behavioral Testing**: Tests focus on user-facing functionality
- **Deterministic RNG**: Mockable random generation for predictable tests
- **Integration Tests**: Discord-specific functionality validation
- **Plugin Integration Tests**: Validates plugin loading and integration flows