# DiceRoller.Net
[![Build Status](https://dev.azure.com/jasonkstevens/PuzzleBox/_apis/build/status/JasonKStevens.DiceRoller.Net?branchName=main)](https://dev.azure.com/jasonkstevens/PuzzleBox/_build/latest?definitionId=9&branchName=main)

A comprehensive .NET 8.0 dice rolling system built on the [Irony](https://github.com/IronyProject/Irony) language kit for parsing dice notation expressions. The solution provides a Discord bot interface for rolling dice with support for complex expressions, game-specific systems, plugins, and an advanced Progressive Disclosure formatting system.

## Recent Updates (January 2025)
- ✅ **Fixed circular dependency** in DI container between `IGameSystemRouter` and `IDiceExpressionEvaluator`
- ✅ **Auto-loads system environment variables** for bot token when VS Code terminal doesn't inherit them
- ✅ **Plugin system fully integrated** with function registry and game system router
- ✅ **Cleaned up debug output** and removed test projects from root
- ✅ **Bot successfully connects** to Discord and registers slash commands

## Project Structure

### Final 8 Projects

#### DiceRoller.Core
The foundational library containing all dice rolling logic, expression parsing, and the Progressive Disclosure formatting system.
- **Parser/** - Irony-based grammar and expression evaluation
  - `ExpressionGrammar.cs` - Defines dice notation grammar
  - `DiceRollEvaluator.cs` - Main entry point for evaluating expressions
  - `DiceRollVisitor.cs` - AST visitor pattern for evaluation
- **Dice/** - Random number generation abstraction
  - `IRandomNumberGenerator.cs` - Interface for mockable RNG
  - `RandomNumberGenerator.cs` - Production RNG implementation
- **Formatting/** - Advanced Progressive Disclosure system with adaptive complexity analysis
  - `DiscordFormatter.cs` - Discord-specific formatting with spoilers, code blocks, emojis
  - `AdaptiveFormatterManager.cs` - Selects formatter based on expression complexity
  - `ComplexityAnalyzer.cs` - Analyzes expression complexity with configurable thresholds
  - `EnhancedTextFormatter.cs` - Hierarchical tree formatting for complex expressions
  - `TreeStructureFormatter.cs` - Unicode tree symbols (├─, └─, │) for structure display
  - `Core/FormattedResult.cs` - Result formatting structure
  - `Core/FormattingContext.cs` - Context for adaptive formatting decisions
  - `Core/ResultFormat.cs` - Verbosity levels (Terse, Standard, Detailed, Narrative, Technical)
- **DragonQuest/** - DragonQuest RPG system specific logic
- **Heroes/** - Heroes system specific logic
- **Services/** - Core services including user aliases and settings

#### DiceRoller.Bot
Discord bot application with hosted service architecture.
- Multiple command processors (roll, party, settings, game-specific)
- User settings and alias management
- **Enhanced configuration system** with environment-specific files, validation, and Options pattern
- Integration with Progressive Disclosure formatting

#### DiceRoller.Discord
Discord integration layer using DSharpPlus.
- Slash command support (`/roll`, `/rollhelp`, `/dq`, `/heroes`)
- Interactive button-based re-rolls
- File attachment for large results
- Discord-specific output formatting

#### DiceRoller.GameSystems
Extensible framework for game-specific systems.
- Base classes for implementing new game systems
- DragonQuest and Heroes system implementations
- Lookup tables and specialized mechanics

#### PartyDSL
Domain-specific language for managing game parties and characters.
- Custom parser for party management commands
- Character and party state management
- Integration with dice rolling for party-based operations

#### DiceRoller.Plugins.Infrastructure
Plugin framework for extensible game system support.
- Base classes and interfaces for plugin development
- Plugin discovery and loading mechanisms
- Dependency injection integration
- PluginIntegrationService for bridging to core systems

#### DiceRoller.Plugins.CallOfCthulhu
Example plugin implementation for Call of Cthulhu RPG system.
- Demonstrates plugin architecture patterns
- Call of Cthulhu specific mechanics and tables
- Shows function and game system registration via providers
- Accessible via `/roll dice:luck()` and `/plugin` commands

### Test Projects

#### DiceRoller.Core.Tests
Comprehensive unit tests for core functionality.
- Deterministic testing with mocked RNG
- Coverage of all expression types
- Edge case validation
- Progressive Disclosure formatting tests

#### PartyDSL.Tests
Tests for party management DSL.
- Party command parsing
- State management validation

## How It All Works Together

```
┌─────────────────────────────────────────────────────────┐
│                 Discord Bot Interface                     │
│                (DiceRoller.Bot)                          │
└─────────────────────┬───────────────────────────────────┘
                      │
          ┌───────────▼────────────┐
          │   Discord Integration  │
          │  (DiceRoller.Discord)  │
          └───────────┬────────────┘
                      │
       ┌──────────────┼──────────────┐
       │              │              │
┌──────▼──────┬───────▼──────┬──────▼──────┐
│ DiceRoller. │   PartyDSL   │ DiceRoller. │
│    Core     │              │GameSystems  │
│             │              │             │
│ Expression  │   Party      │   Game      │
│  Parsing    │ Management   │  Specific   │
│   & Dice    │              │   Rules     │
│  Rolling    │              │             │
│             │              │             │
│ Progressive │              │ Plugin      │
│ Disclosure  │              │ Framework   │
│ Formatting  │              │             │
└─────────────┴──────────────┴─────────────┘
```

The architecture follows a streamlined approach where:
1. **Discord bot** receives slash commands
2. **Discord integration layer** handles command routing and formatting
3. **Core libraries** handle dice rolling logic and Progressive Disclosure formatting
4. **Game systems** apply specific rules and tables
5. **Plugin framework** enables extensible game system support
6. Results flow back up with adaptive formatting based on complexity and user preferences

## Features

### Progressive Disclosure System with Adaptive Formatting

The bot features an intelligent Progressive Disclosure system with sophisticated complexity analysis that transforms unreadable complex dice expressions into clear, hierarchical structures:

- **5 Verbosity Levels**: Terse, Standard, Detailed, Narrative, Technical
- **Adaptive Complexity Analysis**: 5 complexity levels (Simple, Moderate, Complex, VeryComplex, Extreme)
- **Hierarchical Tree Formatting**: Transforms output like "55 12 12 10 1 5 4 + 11 11 9 1 3 5 +" into readable tree structures using Unicode symbols (├─, └─, │)
- **Intelligent Format Selection**: Automatically chooses between inline, code block, or full tree formatting
- **Interactive Buttons**: Re-roll, Details, and Copy functionality
- **Smart Truncation**: Keeps messages under Discord's 2000-character limit
- **Discord Features**: Spoilers, code blocks, bold text, and emojis
- **Configurable Thresholds**: Adjustable complexity scoring for different formatting needs

For detailed architecture information, see [ARCHITECTURE.md](ARCHITECTURE.md).

### Dice Expression Support

| Feature                | Example                    | Adaptive Formatting |
|------------------------|----------------------------|--------------------|
| Arithmetic             | (67 + 54 - 20)/15          | Simple: inline code |
| Dice rolling           | d4                         | Simple: inline code |
|                        | 3d6+2                      | Simple: inline code |
| Exploding dice         | 3d6!                       | Moderate: code block |
| Combo                  | (3d10 + 5) * d7 / 2 - 1d2! | Complex: tree structure |
| Comments               | 1d6+8 # Orc's Damage       | Simple: inline code |
| Minimum roll outcome   | min(d10-4,1)               | Moderate: structured block |
| Repeat rolls           | repeat(d10, 5)             | Complex: tree with iterations |
| Very Complex           | repeat(min(3d6!+2, 20), 4) | Tree: hierarchical breakdown |
| Extreme Complex        | repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10 | Full tree: multi-level structure |

### Discord Bot Commands

| Slash Command | Example | Description |
|--------------|---------|-------------|
| `/roll` | `/roll dice:3d6+2` | Roll dice with any expression |
| `/rollhelp` | `/rollhelp` | Get detailed help for dice commands |
| `/dq` | `/dq command:injury` | DragonQuest specific commands |
| `/heroes` | `/heroes command:speed` | Heroes system commands |
| `/plugin` | `/plugin list` | Manage and monitor loaded plugins |

| Custom Command         | Description                                       |
|------------------------|---------------------------------------------------|
| backfire               | Generates a DQ Backfire result                    |
| injury                 | Generates a DQ Grievous Injury                    |
| fear                   | Generates a DQ Fear Result                        |
| help                   | Usage details                                     |
| spelldmg               | Displays spell detail, with dmg at specified rank |
| skill                  | Displays skill detail, with roll calculations     |
| save <alias> <ins>     | Saves an alias for the instructions, for user     |


## REPL
<pre>
> d4
1 Reason: [1]

> 3d6
9 Reason: [1, 4, 4]

> 3/4
0.75 Reason: 3 / 4

> (3d10 + 5) / 2 + 1d2!
14.5 Reason: [9, 4, 5] + 5 / 2 + [2!, 1]

> 3d100 < 55  OR should this be: repeat(d100 < 55, 3) ???
2 successes Reason: [45, 87, 22]

> 3d10 > 55 OR alternate - need to decide which one
1 successes Reason: [45, 87, 22]

> min(d10-4,1) + 5
6 Reason: [2]-4 = -2, min 1 => 1 + 5

> repeat(d10,6)
24 Reason: [1] [2] [6] [1] [9] [5]

> backfire
32 Reason: The Adept becomes the target of the spell with some or all effects doubled.

> injury
99 Reason: Crushing blow to your pelvis breaks bone and tears tissue. Take 7 Damage Points immediately to Endurance and fall prone. Make a WP check to avoid falling unconscious. If you survive, you will be unable to move for D10 months.
</pre>

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Discord bot token (for Discord bot functionality)
- Visual Studio 2022 or VS Code (optional)

### Building the Solution
```bash
dotnet build src/DiceRoller.sln
```

### Configuration

The bot features a comprehensive configuration system with environment-specific settings, validation, and flexible configuration sources.

#### Quick Setup
1. **Configure Discord bot token**:
   ```bash
   # Environment variable (recommended)
   export DICEROLLER_Bot__Token=your-discord-bot-token
   
   # Or in appsettings.json
   "Bot": { "Token": "your-discord-bot-token" }
   ```

2. **Set environment** (optional):
   ```bash
   export ASPNETCORE_ENVIRONMENT=Development  # or Production, Docker
   ```

#### Configuration Features
- **Multi-environment support** with `appsettings.{Environment}.json` files
- **Environment variable support** with `DICEROLLER_` prefix
- **Backward compatibility** with legacy `BotConfig_` variables
- **Strong typing and validation** with data annotations
- **Configurable logging, plugins, and security settings**

For complete configuration documentation, see **[CONFIGURATION.md](CONFIGURATION.md)**.

### Running the Applications

#### Discord Bot
Run: `dotnet run --project src/DiceRoller.Bot`

The bot uses **slash commands** exclusively:
- Type `/roll` then select the "dice" parameter
- Provides autocomplete suggestions and built-in help
- Use `/rollhelp` for detailed command information
- Features Progressive Disclosure formatting for optimal Discord experience

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests without rebuilding
dotnet test --no-build

# Run specific test project
dotnet test tests/DiceRoller.Core.Tests/DiceRoller.Core.Tests.csproj

# Run specific test
dotnet test --filter "FullyQualifiedName~DiceRollFixture"
```

## Testing

The project includes comprehensive test coverage with **280+ tests** across multiple test fixtures:

### Test Organization

**Core Functionality Tests**
- `DiceRollFixture` - Basic dice rolling (d20, 3d6, etc.)
- `EquationFixture` - Arithmetic expressions
- `ExplodingDiceFixture` - Exploding dice mechanics (3d6!)
- `MinFixture` - Minimum function testing
- `RepeatFixture` - Repeat function testing
- `InequalityFixture` - Comparison operations
- `ComplexExpressionsFixture` - Nested and complex expressions

**Game System Tests**
- `DragonQuestCommandsFixture` - DragonQuest system commands (backfire, injury, fear)
- `HeroesCommandsFixture` - Heroes system commands (speed, location)
- `LocationTablesFixture` - All location table lookups for both systems

**Feature Tests**
- `UserAliasesFixture` - User-defined alias functionality
- `ErrorHandlingFixture` - Invalid input validation and error cases

**Integration Tests**
- `PartyDSL.Tests` - Party management system tests

### Testing Patterns

All tests follow consistent patterns for reliability:

```csharp
// Deterministic testing with sequence generator
var sequenceGenerator = DiceHelper.GetSequenceGenerator(10, 5, 15, 3, 8);
var evaluator = new DiceRollEvaluator(sequenceGenerator);

// Test both numeric result and breakdown
var result = evaluator.Evaluate("3d6+2");
Assert.That(result.Value, Is.EqualTo(expected));
Assert.That(result.Breakdown, Is.EqualTo(expectedBreakdown));
```

**Key Testing Features:**
- **Deterministic Results**: Uses `DiceHelper.GetSequenceGenerator()` for predictable test outcomes
- **Dual Validation**: Tests both numeric `Value` and descriptive `Breakdown` properties
- **Parameterized Tests**: Uses NUnit `[TestCase]` attributes for comprehensive coverage
- **Error Validation**: Comprehensive testing of invalid inputs and edge cases
- **Game System Coverage**: Complete testing of all DragonQuest and Heroes lookup tables

### Test Coverage Highlights

- **280+ Total Tests** across all projects
- **Complete DragonQuest System Coverage**: All injury, backfire, fear, and location tables
- **Complete Heroes System Coverage**: Speed and location mechanics
- **Adaptive Formatting Coverage**: All complexity levels and formatting strategies
- **Tree Structure Testing**: Unicode symbol formatting and hierarchical output
- **Comprehensive Error Handling**: Invalid syntax, empty inputs, malformed expressions
- **Edge Case Testing**: Complex nested expressions, extreme values, boundary conditions
- **User Feature Testing**: Alias creation, storage, and retrieval

## Development

### Visual Studio Code
Press F5 to launch the Discord bot with debugging enabled. The launch configuration is pre-configured in `.vscode/launch.json`.

### Adding New Game Systems

#### Using the Plugin Framework
1. Create a new project referencing `DiceRoller.Plugins.Infrastructure`
2. Implement `IPluginFunctionProvider` for custom dice functions
3. Implement `IPluginGameSystemProvider` for game-specific commands
4. Follow the pattern established in `DiceRoller.Plugins.CallOfCthulhu`
5. Functions and commands automatically available via `/roll` and Discord integration
6. Monitor plugin status via `/plugin list` command
7. Add comprehensive test coverage following existing patterns

**Example Plugin Functions:**
```csharp
// Available as /roll dice:luck() after plugin loads
public class LuckFunction : IDiceFunction { ... }

// Available as game command after plugin loads  
public class CallOfCthulhuGameSystem : IGameSystem { ... }
```

#### Direct Integration
1. Create a new class inheriting from `GameSystemBase` in `DiceRoller.GameSystems`
2. Implement game-specific lookup tables and mechanics
3. Register the system in the dependency injection container
4. Add comprehensive test coverage following existing patterns

### Test Development Guidelines

When adding new features, ensure comprehensive test coverage:

1. **Create Test Fixtures**: Organize tests by feature area (e.g., `NewFeatureFixture.cs`)
2. **Use Deterministic Testing**: Always use `DiceHelper.GetSequenceGenerator()` for predictable results
3. **Test Both Results**: Validate both `Value` and `Breakdown` properties
4. **Include Error Cases**: Test invalid inputs and edge conditions
5. **Use Parameterized Tests**: Leverage `[TestCase]` for multiple input scenarios

```csharp
[TestCase("d20+5", 15, "[10] + 5")]
[TestCase("3d6", 18, "[3, 6, 9]")]
public void NewFeature_ValidInputs_ReturnsExpectedResults(string input, int expectedValue, string expectedBreakdown)
{
    var result = _evaluator.Evaluate(input);
    Assert.That(result.Value, Is.EqualTo(expectedValue));
    Assert.That(result.Breakdown, Is.EqualTo(expectedBreakdown));
}
```

## Roadmap

### Completed Architecture Refactoring
- **Phase 1: Function Registry Pattern** ✅ COMPLETED
  - Added extension points to DiceRollVisitor for custom functions
  - Created IFunctionRegistry and IDiceFunction interfaces
  - Implemented built-in functions as IDiceFunction implementations
- **Phase 2: Game System Refactoring** ✅ COMPLETED
  - Created IGameSystemRouter for command routing
  - Implemented type conversion and error handling
  - Established foundation for plugin-based game systems
- **Phase 3: Plugin Integration System** ✅ COMPLETED
  - Created PluginIntegrationService bridging plugins to core systems
  - Added Discord commands for plugin management
  - Example CallOfCthulhu plugin demonstrates full integration
- **Configuration Enhancement** ✅ COMPLETED
  - Strongly-typed configuration with validation
  - Environment-specific appsettings files (Development, Production, Docker)
  - Options pattern with IOptions<T> dependency injection
  - Backward compatibility with legacy configuration

### Plugin System Features
- **Runtime Plugin Management**: Monitor and reload plugins via Discord commands
- **Dynamic Function Registration**: Plugins can add custom dice functions
- **Game System Integration**: Plugins can provide new game-specific commands
- **Zero Breaking Changes**: All 323 tests passing with existing functionality intact

### TODO
- Add --help support
- Implement additional game systems via plugin architecture
- Add more comprehensive API documentation
- Create plugin template and documentation
