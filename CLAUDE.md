# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DiceRoller.Net is a Discord-only dice rolling bot built on .NET 8.0 using the Irony language kit. It features a sophisticated Progressive Disclosure system that adapts output formatting based on expression complexity.

**For detailed architecture information, see [ARCHITECTURE.md](ARCHITECTURE.md).**

## Next Development Steps

**See [DICE_SYSTEM_REFACTORING_OPTIONS.md](DICE_SYSTEM_REFACTORING_OPTIONS.md) for the full refactoring approach.**

### Phase 1: Hybrid Minimal Refactor ✅ COMPLETED
- ✅ Added extension points to DiceRollVisitor with registry-first pattern
- ✅ Created IFunctionRegistry and IDiceFunction interfaces
- ✅ Implemented built-in functions (min, max, repeat) as IDiceFunction implementations
- ✅ Updated dependency injection configuration
- ✅ Fixed complexity analyzer scoring and added NodeType.Max support
- ✅ All 323 core tests passing

### Phase 2: Game System Refactoring ✅ COMPLETED
- ✅ Created IGameSystemRouter interface for command routing
- ✅ Implemented GameSystemRouter with type conversion and error handling
- ✅ Updated DiceRollVisitor with router-first fallback pattern
- ✅ Integrated with existing DragonQuestGameSystem
- ✅ All 323 tests passing with zero breaking changes
- ✅ Foundation established for plugin-based game systems

### Phase 3: Plugin Integration System ✅ COMPLETED
- ✅ Created PluginIntegrationService bridging plugins to Function Registry and Game System Router
- ✅ Added Discord commands for plugin management (/plugin list, info, reload)
- ✅ Fixed critical issues: blocking async operations, function unregistration
- ✅ Integrated with existing plugin infrastructure
- ✅ All 323 tests passing with zero breaking changes
- ✅ Example CallOfCthulhu plugin demonstrates full integration

### Phase 4: Advanced Plugin Features (Future)
- Enhanced plugin discovery and registration mechanisms
- Plugin template for community contributions
- Runtime plugin loading capabilities
- Advanced function metadata and documentation
- Timeline: 2-3 weeks

## Development Commands

### Build
```bash
dotnet build
dotnet build -c Release
```

### Test
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

### Run Application
```bash
# Run the Discord bot
dotnet run --project src/DiceRoller.Bot

# VS Code: Press F5 to debug the Discord bot
```

## Key Technical Details

### Progressive Disclosure System with Adaptive Formatting
The bot intelligently adapts dice roll output based on complexity:
- 5 verbosity levels (Terse, Standard, Detailed, Narrative, Technical)
- 5 complexity levels (Simple, Moderate, Complex, VeryComplex, Extreme) with automatic analysis
- Hierarchical tree formatting for complex expressions using Unicode symbols (├─, └─, │)
- Transforms unreadable output like "55 12 12 10 1 5 4 +" into structured breakdowns
- Smart truncation for Discord's 2000-character limit
- Interactive buttons for re-roll, details, and copy functionality
- Discord-optimized formatting with spoilers, markdown, and emojis

### Game System Router Architecture
The bot now features a sophisticated router pattern for game system commands:
- IGameSystemRouter interface enables dynamic command routing
- GameSystemRouter handles type conversion and error handling automatically
- Router-first fallback pattern maintains backward compatibility
- Eliminates code duplication between game systems
- Foundation for plugin-based game system architecture

### Plugin Integration System
The bot features a complete plugin integration system enabling extensible functionality:
- PluginIntegrationService bridges plugins to core systems
- Automatic plugin discovery and registration at startup
- Dynamic function and game system registration from plugins
- Discord commands for plugin management (/plugin list, info, reload)
- Example CallOfCthulhu plugin demonstrates full integration capabilities

### Discord Integration
- Slash commands only (`/roll`, `/rollhelp`, `/dq`, `/heroes`, `/plugin`)
- Named parameter "dice" required for Discord API
- Interactive button components for user actions
- Intelligent message truncation (no file attachments)
- Plugin management commands for runtime monitoring

### Testing Standards (323+ Tests)

Comprehensive test coverage with deterministic testing:
- Always use `DiceHelper.GetSequenceGenerator()` for deterministic RNG
- Test both `Value` and `Breakdown` properties
- Use NUnit `[TestCase]` for parameterized tests
- Include error cases and edge conditions

#### When Adding New Tests

1. **Create Test Fixtures**: Organize by feature area (e.g., `NewFeatureFixture.cs`)
2. **Follow Naming Conventions**: Use pattern `MethodName_Scenario_ExpectedResult`
3. **Use Deterministic RNG**: Mock `IRandomNumberGenerator` with `DiceHelper.GetSequenceGenerator()`
4. **Test Both Properties**: Validate both `Value` and `Breakdown` for dice expressions
5. **Include Edge Cases**: Test invalid inputs, boundary conditions, and error scenarios
6. **Use Parameterized Tests**: Leverage `[TestCase]` for multiple input scenarios

### Supported Dice Notation

- Basic dice: `d20`, `3d6`
- Arithmetic: `3d6 + 5`, `(2d8 + 3) * 2`
- Exploding dice: `3d6!`
- Functions: `min(d10-4, 1)`, `max(3d6, 10)`, `repeat(d20, 5)`, `step 15`
- Comments: `d20+5 # Attack roll`
- Game commands: `backfire`, `injury`, `fear`, `speed`, `hitloc`
- User aliases: `addalias`, `removealias`, `listalias`
- Comparisons: `3d6 > 10`, `d20 >= 15`
- Complex expressions: `repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10`