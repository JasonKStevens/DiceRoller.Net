# Plugin Development Guide

A step-by-step guide to creating your own DiceRoller.Net game system plugin using this template.

## 🎯 Overview

This guide will walk you through creating a complete plugin for your favorite tabletop RPG system. By the end, you'll have a fully functional plugin that integrates seamlessly with DiceRoller.Net and provides Discord users with game-specific commands and lookup tables.

## 📋 Prerequisites

Before starting, ensure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Basic C# programming knowledge
- Familiarity with your target game system rules
- Access to the DiceRoller.Net source code

## 🚀 Step 1: Project Setup

### 1.1 Copy the Template
```bash
# Navigate to the src directory
cd src

# Copy the template to your new plugin directory
cp -r DiceRoller.Plugins.Template DiceRoller.Plugins.YourGameSystem

# Enter your new plugin directory
cd DiceRoller.Plugins.YourGameSystem
```

### 1.2 Rename Project Files
1. Rename `DiceRoller.Plugins.Template.csproj` to `DiceRoller.Plugins.YourGameSystem.csproj`
2. Update the project file metadata:
   ```xml
   <PropertyGroup>
     <Title>DiceRoller YourGameSystem Plugin</Title>
     <Authors>Your Name</Authors>
     <Description>DiceRoller.Net plugin for YourGameSystem RPG</Description>
     <PackageProjectUrl>https://github.com/yourusername/diceroller-yourgamesystem</PackageProjectUrl>
   </PropertyGroup>
   ```

### 1.3 Update Namespace
Replace all instances of `DiceRoller.Plugins.Template` with `DiceRoller.Plugins.YourGameSystem`:

**Files to update:**
- `TemplatePlugin.cs` → `YourGameSystemPlugin.cs`
- `GameSystem/TemplateGameSystem.cs` → `GameSystem/YourGameSystemGameSystem.cs`
- `Commands/ExampleCommand.cs` → `Commands/YourGameSystemCommands.cs`
- `Tables/ExampleTable.cs` → `Tables/YourGameSystemTables.cs`

**Find and replace:**
- `namespace DiceRoller.Plugins.Template` → `namespace DiceRoller.Plugins.YourGameSystem`
- `TemplatePlugin` → `YourGameSystemPlugin`
- `TemplateGameSystem` → `YourGameSystemGameSystem`

## 🔧 Step 2: Configure Plugin Metadata

### 2.1 Update Plugin.manifest.json
```json
{
  "id": "yourgamesystem",
  "name": "YourGameSystem Game System",
  "version": "1.0.0",
  "description": "Provides YourGameSystem RPG support including skill checks, combat mechanics, and lookup tables.",
  "author": "Your Name",
  "website": "https://github.com/yourusername/diceroller-yourgamesystem",
  "license": "MIT",
  "tags": ["rpg", "yourgamesystem", "tabletop"],
  "minimumHostVersion": "1.0.0",
  "assemblyFileName": "DiceRoller.Plugins.YourGameSystem.dll",
  "pluginTypeName": "DiceRoller.Plugins.YourGameSystem.YourGameSystemPlugin",
  "defaultConfiguration": {
    "enableAdvancedRules": true,
    "defaultDifficulty": "normal",
    "useOptionalRules": false
  }
}
```

### 2.2 Update Plugin Attributes
In your main plugin class:
```csharp
[Plugin("yourgamesystem", "YourGameSystem Game System", "1.0.0",
    Description = "Provides YourGameSystem RPG support including skill checks, combat mechanics, and lookup tables.",
    Author = "Your Name",
    Tags = new[] { "rpg", "yourgamesystem", "tabletop" })]
[GameSystemPlugin("YourGameSystem", GameSystemVersion = "5th Edition", Publisher = "YourGameSystem Publishing")]
public class YourGameSystemPlugin : PluginBase, IGameSystemPlugin
```

## 🎮 Step 3: Design Your Game System

### 3.1 Identify Core Mechanics
Before coding, plan what your plugin will support:

**Common RPG mechanics to consider:**
- Skill/ability checks
- Combat resolution (initiative, attacks, damage)
- Character creation helpers
- Spell/magic systems
- Equipment and inventory
- Status effects and conditions

**Example for D&D 5e:**
- Ability checks with advantage/disadvantage
- Attack rolls with proficiency bonus
- Damage with resistance/vulnerability
- Spell save DCs
- Initiative tracking

### 3.2 Plan Commands
List the commands your plugin will provide:

```
skillcheck   - Make skill/ability checks
attack       - Attack rolls with modifiers
damage       - Damage calculation with types
save         - Saving throws
initiative   - Initiative rolls
spellsave    - Spell save DC calculations
```

### 3.3 Plan Lookup Tables
Identify reference tables players frequently use:

```
conditions     - Status effects and their rules
spells         - Quick spell reference
equipment      - Weapon and armor stats
encounters     - Random encounter generation
weather        - Environmental effects
treasures      - Random treasure generation
```

## ⚡ Step 4: Implement Commands

### 4.1 Create Your First Command
Replace the template skill check with your game's mechanics:

```csharp
// Commands/SkillCheckCommand.cs
public class SkillCheckCommand
{
    public async Task<GameSystemCommandResult> ExecuteAsync(
        Dictionary<string, object> parameters,
        GameSystemContext context)
    {
        // 1. Extract parameters specific to your game
        var abilityScore = ExtractIntParameter(parameters, "ability_score", 10);
        var proficiencyBonus = ExtractIntParameter(parameters, "proficiency", 0);
        var hasAdvantage = ExtractBoolParameter(parameters, "advantage", false);
        var hasDisadvantage = ExtractBoolParameter(parameters, "disadvantage", false);
        var modifier = ExtractIntParameter(parameters, "modifier", 0);

        // 2. Implement your game's dice mechanics
        var roll = RollD20(hasAdvantage, hasDisadvantage);
        var abilityMod = CalculateAbilityModifier(abilityScore);
        var total = roll + abilityMod + proficiencyBonus + modifier;

        // 3. Determine success based on your game's rules
        var dc = ExtractIntParameter(parameters, "dc", 15);
        var success = total >= dc;

        // 4. Create result object
        var result = new YourGameSkillCheckResult
        {
            Roll = roll,
            AbilityScore = abilityScore,
            AbilityModifier = abilityMod,
            ProficiencyBonus = proficiencyBonus,
            Modifier = modifier,
            Total = total,
            DC = dc,
            Success = success,
            HasAdvantage = hasAdvantage,
            HasDisadvantage = hasDisadvantage
        };

        // 5. Format for Discord
        var description = FormatSkillCheckResult(result);
        
        return new GameSystemCommandResult(true, result, description);
    }

    private int RollD20(bool advantage, bool disadvantage)
    {
        var roll1 = _random.Next(1, 21);
        
        if (advantage && !disadvantage)
        {
            var roll2 = _random.Next(1, 21);
            return Math.Max(roll1, roll2);
        }
        
        if (disadvantage && !advantage)
        {
            var roll2 = _random.Next(1, 21);
            return Math.Min(roll1, roll2);
        }
        
        return roll1; // Normal roll
    }

    private int CalculateAbilityModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2; // Standard D&D modifier calculation
    }
}
```

### 4.2 Add More Commands
Create additional command files for each major game mechanic:

- `AttackCommand.cs` - Attack rolls and hit calculations
- `DamageCommand.cs` - Damage with type resistance
- `SavingThrowCommand.cs` - Save vs. effects
- `InitiativeCommand.cs` - Combat order determination

### 4.3 Register Commands
Update your game system class to register all commands:

```csharp
private void RegisterCommands()
{
    _commands.Add(new GameSystemCommand(
        "skillcheck",
        "Make a skill or ability check",
        new List<GameSystemParameter>
        {
            new("ability_score", typeof(int), true, null, "Character's ability score (3-20)"),
            new("proficiency", typeof(int), false, 0, "Proficiency bonus (+2 to +6)"),
            new("advantage", typeof(bool), false, false, "Roll with advantage"),
            new("disadvantage", typeof(bool), false, false, "Roll with disadvantage"),
            new("modifier", typeof(int), false, 0, "Additional modifier"),
            new("dc", typeof(int), false, 15, "Difficulty Class to beat")
        },
        "Make skill checks and ability checks with your game system's mechanics.",
        true));
}
```

## 📊 Step 5: Implement Lookup Tables

### 5.1 Create Range-Based Tables
Perfect for random generation (rolling dice for results):

```csharp
// Tables/ConditionsTable.cs
public class ConditionsTable
{
    private List<TableRangeEntry> InitializeConditionsTable()
    {
        var tableData = new Dictionary<string, string>
        {
            ["01-10"] = "**Blinded** - Cannot see, attacks have disadvantage, attackers have advantage",
            ["11-20"] = "**Charmed** - Cannot attack charmer, charmer has advantage on social interactions",
            ["21-30"] = "**Frightened** - Disadvantage on ability checks and attacks while source is in sight",
            ["31-40"] = "**Grappled** - Speed becomes 0, cannot benefit from bonuses to speed",
            ["41-50"] = "**Incapacitated** - Cannot take actions or reactions",
            ["51-60"] = "**Paralyzed** - Incapacitated, cannot move or speak, auto-fail Str and Dex saves",
            ["61-70"] = "**Poisoned** - Disadvantage on attack rolls and ability checks",
            ["71-80"] = "**Prone** - Disadvantage on attacks, attacks within 5ft have advantage",
            ["81-90"] = "**Restrained** - Speed 0, disadvantage on attacks and Dex saves",
            ["91-00"] = "**Stunned** - Incapacitated, cannot move, can only speak falteringly"
        };

        return TableLookupHelper.CreateRangeTable(tableData);
    }
}
```

### 5.2 Create Reference Tables
Perfect for quick rule lookups:

```csharp
// Tables/SpellsTable.cs
public class SpellsTable
{
    private Dictionary<string, SpellInfo> InitializeSpellsTable()
    {
        return new Dictionary<string, SpellInfo>
        {
            ["fireball"] = new SpellInfo(
                "Fireball",
                3,
                "Evocation",
                "8d6 fire damage in 20-foot radius",
                "Dex save for half damage"),
            ["cure wounds"] = new SpellInfo(
                "Cure Wounds",
                1,
                "Evocation",
                "1d8 + spellcasting modifier healing",
                "Touch range, bonus action"),
            // Add more spells...
        };
    }
}

public record SpellInfo(
    string Name,
    int Level,
    string School,
    string Effect,
    string Notes);
```

### 5.3 Register Tables
```csharp
private void RegisterTables()
{
    _tables.Add(new GameSystemTable(
        "conditions",
        "Status conditions and their effects",
        typeof(int),
        typeof(string),
        "Provides quick reference for game conditions and their mechanical effects."));

    _tables.Add(new GameSystemTable(
        "spells",
        "Quick spell reference",
        typeof(string),
        typeof(SpellInfo),
        "Look up basic information about spells by name."));
}
```

## 🎨 Step 6: Format Discord Output

### 6.1 Design Consistent Formatting
Create a style guide for your plugin's output:

```csharp
private string FormatSkillCheckResult(YourGameSkillCheckResult result)
{
    var advantageText = result.HasAdvantage ? " (ADV)" : result.HasDisadvantage ? " (DIS)" : "";
    var successEmoji = result.Success ? "✅" : "❌";
    
    var description = $"{successEmoji} **Skill Check{advantageText}:** ";
    description += result.Success ? "**SUCCESS**" : "**FAILURE**";
    description += $"\n🎲 **Roll:** {result.Roll}";
    
    // Show calculation breakdown
    var parts = new List<string>();
    if (result.AbilityModifier != 0)
        parts.Add($"{result.AbilityModifier:+0;-#} (ability)");
    if (result.ProficiencyBonus != 0)
        parts.Add($"{result.ProficiencyBonus:+0;-#} (proficiency)");
    if (result.Modifier != 0)
        parts.Add($"{result.Modifier:+0;-#} (modifier)");
    
    if (parts.Any())
        description += $" {string.Join(" ", parts)}";
        
    description += $" = **{result.Total}** vs DC {result.DC}";
    
    return description;
}
```

### 6.2 Use Appropriate Emojis
Create a consistent emoji vocabulary:

```csharp
public static class GameEmojis
{
    public const string Dice = "🎲";
    public const string Success = "✅";
    public const string Failure = "❌";
    public const string Critical = "💥";
    public const string Attack = "⚔️";
    public const string Shield = "🛡️";
    public const string Magic = "✨";
    public const string Healing = "💚";
    public const string Fire = "🔥";
    public const string Ice = "❄️";
    public const string Lightning = "⚡";
}
```

## 🧪 Step 7: Test Your Plugin

### 7.1 Unit Testing
Create test files for your commands and tables:

```csharp
// Tests/SkillCheckCommandTests.cs
[TestFixture]
public class SkillCheckCommandTests
{
    private SkillCheckCommand _command;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger>().Object;
        _command = new SkillCheckCommand(_logger);
    }

    [TestCase(15, 2, 0, 15, true)]  // Should succeed
    [TestCase(10, 0, 0, 15, false)] // Should fail
    public async Task SkillCheck_WithVariousInputs_ReturnsExpectedSuccess(
        int abilityScore, int proficiency, int modifier, int dc, bool expectedSuccess)
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["ability_score"] = abilityScore,
            ["proficiency"] = proficiency,
            ["modifier"] = modifier,
            ["dc"] = dc
        };
        var context = new GameSystemContext("test-user");

        // Act
        var result = await _command.ExecuteAsync(parameters, context);

        // Assert
        Assert.IsTrue(result.Success);
        var skillResult = result.Value as YourGameSkillCheckResult;
        Assert.IsNotNull(skillResult);
        // Note: This test would need deterministic random for exact result testing
    }
}
```

### 7.2 Integration Testing
Test the complete plugin lifecycle:

```csharp
[TestFixture]
public class YourGameSystemPluginIntegrationTests
{
    private YourGameSystemPlugin _plugin;

    [SetUp]
    public async Task Setup()
    {
        _plugin = new YourGameSystemPlugin();
        var context = CreateMockPluginContext();
        await _plugin.InitializeAsync(context);
        await _plugin.StartAsync();
    }

    [Test]
    public async Task ExecuteCommand_SkillCheck_ReturnsValidResult()
    {
        // Test that commands work end-to-end
        var parameters = new Dictionary<string, object>
        {
            ["ability_score"] = 15,
            ["dc"] = 12
        };
        var context = new GameSystemContext("test-user");

        var result = await _plugin.ExecuteCommandAsync("skillcheck", parameters, context);

        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(result.Description);
    }
}
```

### 7.3 Manual Testing
Test your plugin in the actual Discord environment:

1. Build your plugin
2. Copy the output to the bot's plugins directory
3. Restart the bot
4. Use `/plugin list` to verify it loaded
5. Test each command with various parameters
6. Test error conditions and edge cases

## 🚀 Step 8: Build and Deploy

### 8.1 Build the Plugin
```bash
# Build in release mode
dotnet build -c Release

# Verify output
ls bin/Release/net8.0/
```

### 8.2 Deploy to DiceRoller.Net
```bash
# Copy plugin files to bot directory
cp bin/Release/net8.0/* /path/to/DiceRoller.Bot/plugins/

# Restart the bot to load the plugin
# The plugin should appear in /plugin list
```

### 8.3 Verify Deployment
In Discord, test that your plugin loaded correctly:

```
/plugin list
/plugin info yourgamesystem
/yourgamesystem skillcheck ability_score:15 dc:12
```

## 📋 Step 9: Documentation and Distribution

### 9.1 Update Documentation
- Complete the README.md with your game system specifics
- Document all commands with examples
- Include table descriptions and usage
- Add troubleshooting section

### 9.2 Create Examples
Include practical examples in your documentation:

```markdown
## Command Examples

### Skill Checks
- `/yourgamesystem skillcheck ability_score:16 proficiency:3` - Proficient skill check
- `/yourgamesystem skillcheck ability_score:12 advantage:true` - Skill check with advantage
- `/yourgamesystem skillcheck ability_score:14 dc:18 modifier:2` - Difficult check with bonus

### Combat
- `/yourgamesystem attack bonus:5 target_ac:15` - Standard attack
- `/yourgamesystem damage dice:1d8+3 type:slashing` - Weapon damage
```

### 9.3 Prepare for Distribution
If sharing with the community:

1. Create a GitHub repository
2. Include comprehensive documentation
3. Add usage examples and screenshots
4. Provide installation instructions
5. Include license information
6. Set up issues tracking for bug reports

## 🔧 Step 10: Advanced Features

### 10.1 Configuration Management
Allow users to customize your plugin:

```csharp
protected override async Task OnConfigurationUpdatedAsync(object configuration)
{
    var useOptionalRules = GetConfigurationValue("useOptionalRules", false);
    var defaultDifficulty = GetConfigurationValue("defaultDifficulty", "normal");
    
    _gameSystem.UpdateConfiguration(useOptionalRules, defaultDifficulty);
    
    _logger.LogInformation("Configuration updated: OptionalRules={Optional}, DefaultDC={DC}",
        useOptionalRules, defaultDifficulty);
}
```

### 10.2 State Management
For complex features like initiative tracking:

```csharp
public class InitiativeTracker
{
    private readonly Dictionary<string, List<InitiativeEntry>> _combats = new();
    
    public void StartCombat(string channelId, List<InitiativeEntry> participants)
    {
        _combats[channelId] = participants.OrderByDescending(p => p.Initiative).ToList();
    }
    
    public InitiativeEntry? GetCurrentTurn(string channelId)
    {
        return _combats.TryGetValue(channelId, out var combat) ? combat.FirstOrDefault() : null;
    }
}
```

### 10.3 External Data Integration
Load game data from external sources:

```csharp
public async Task LoadSpellDataAsync()
{
    try
    {
        var spellDataUrl = GetConfigurationValue("spellDataUrl", "");
        if (!string.IsNullOrEmpty(spellDataUrl))
        {
            var spellData = await _httpClient.GetFromJsonAsync<SpellData>(spellDataUrl);
            UpdateSpellDatabase(spellData);
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to load external spell data, using defaults");
    }
}
```

## 🎯 Tips for Success

### 1. Start Simple
- Implement 2-3 core commands first
- Add one essential lookup table
- Test thoroughly before adding more features

### 2. Follow Game Rules Precisely
- Reference official rulebooks
- Include rule citations in comments
- Test edge cases and corner cases

### 3. Focus on User Experience
- Make commands intuitive and easy to remember
- Provide helpful error messages
- Include usage examples in help text

### 4. Optimize for Discord
- Keep output concise but informative
- Use formatting that's readable on mobile
- Test in actual Discord channels

### 5. Plan for Growth
- Design extensible architecture
- Use configuration for customizable behavior
- Document your code for future maintainers

## 🆘 Troubleshooting

### Common Issues

**Plugin doesn't load:**
- Check manifest.json syntax
- Verify assembly name matches manifest
- Check logs for initialization errors

**Commands don't work:**
- Verify command registration
- Check parameter validation
- Test with simple inputs first

**Tables return errors:**
- Validate lookup key types
- Check range definitions
- Test with known good values

**Discord formatting issues:**
- Test output length (2000 char limit)
- Verify markdown syntax
- Check emoji compatibility

### Debug Logging
Add detailed logging to troubleshoot issues:

```csharp
_logger.LogDebug("Processing command {Command} with parameters: {@Parameters}", 
    commandName, parameters);
_logger.LogDebug("Calculated result: {@Result}", result);
_logger.LogDebug("Formatted output length: {Length}", description.Length);
```

## 🎉 Congratulations!

You've created a complete DiceRoller.Net plugin! Your game system is now available to Discord users worldwide through the DiceRoller bot.

### Next Steps
- Gather user feedback
- Add more advanced features
- Consider contributing to the DiceRoller.Net ecosystem
- Share your plugin with the community

### Join the Community
- Submit your plugin to the official plugin registry
- Help other developers with their plugins
- Contribute improvements to the core framework

Happy coding! 🎲