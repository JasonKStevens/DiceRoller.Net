namespace DiceRoller.Core.Content
{
    /// <summary>
    /// Static class containing help content for all Discord commands
    /// </summary>
    public static class HelpContent
    {
        /// <summary>
        /// General help content for all available commands
        /// </summary>
        public static readonly string GeneralHelp = @"**DiceRoller.Net - Discord Dice Rolling Bot**

**Available Commands:**
• `/roll` - Roll dice expressions with complex notation support
• `/rollhelp` - Get detailed help for dice rolling commands
• `/dq` - DragonQuest game system commands
• `/heroes` - Heroes game system commands  
• `/party` - Party management commands
• `/settings` - User settings management
• `/plugin` - Plugin management commands

**Getting Started:**
Use `/roll d:help` or `/rollhelp` for dice notation help
Use any command with `help:true` parameter for specific help

**Features:**
• Progressive Disclosure with adaptive formatting
• Interactive buttons for re-roll and details
• Plugin system for extensible game systems
• User aliases and party management
• Smart output truncation for Discord limits";

        /// <summary>
        /// Help content for the /roll command
        /// </summary>
        public static readonly string RollCommandHelp = @"**Dice Rolling Command Help**

**Usage:** `/roll d:<expression> [ephemeral:true/false] [format:auto/terse/standard/detailed]`

**Basic Dice Expressions:**
• `d20` - Roll a 20-sided die
• `3d6` - Roll three 6-sided dice
• `2d10+5` - Roll two 10-sided dice and add 5
• `d100` - Roll percentile dice

**Advanced Expressions:**
• `3d6!` - Exploding dice (re-roll max values)
• `min(d10-4, 1)` - Use minimum value between roll and 1
• `max(3d6, 10)` - Use maximum value between roll and 10
• `repeat(d20, 5)` - Repeat a roll 5 times
• `(2d8 + 3) * 2` - Complex arithmetic with parentheses

**Comparison Operations:**
• `3d6 > 10` - Check if roll is greater than 10
• `d20 >= 15` - Check if roll meets or exceeds 15
• `2d6 == 7` - Check if roll equals exactly 7

**Comments:**
• `d20+5 # Attack roll` - Add comments to your rolls

**Format Options:**
• `auto` - Automatically choose based on complexity (default)
• `terse` - Minimal output
• `standard` - Standard detail level
• `detailed` - Full breakdown with tree structure

**Examples:**
• `/roll d:3d6+3` - Simple attribute roll
• `/roll d:repeat(min(3d6!+2, 20), 4)` - Complex expression
• `/roll d:d20+5 format:detailed` - Detailed breakdown
• `/roll d:backfire ephemeral:true` - Private game system roll
• `/roll d:help` - Show this help message";

        /// <summary>
        /// Help content for the /rollhelp command
        /// </summary>
        public static readonly string RollHelpCommandHelp = @"**Roll Help Command**

**Usage:** `/rollhelp`

**Description:**
Shows comprehensive dice notation help including basic expressions, 
advanced functions, game system commands, and formatting options.

This is equivalent to using `/roll help` but provides the full
reference in a convenient dedicated command.";

        /// <summary>
        /// Help content for the /dq command (DragonQuest)
        /// </summary>
        public static readonly string DragonQuestCommandHelp = @"**DragonQuest Game System Commands**

**Usage:** `/dq command:<action> [modifier:<value>] [ephemeral:true/false] [help:true]`

**Available Commands:**
• `backfire` - Roll on the backfire table
• `injury` or `specgrev` - Roll for grievous injury
• `fear` - Roll on the fear table  
• `hitloc [bodytype]` - Roll hit location
• `hitlocation [bodytype]` - Same as hitloc

**Hit Location Body Types:**
• `humanoid_mid` - Standard humanoid (default)
• `quadruped_mid` - Four-legged creatures
• `avian_mid` - Flying creatures
• `serpent_mid` - Snake-like creatures

**Examples:**
• `/dq command:backfire` - Roll backfire
• `/dq command:injury` - Roll grievous injury
• `/dq command:hitloc modifier:quadruped_mid` - Quadruped hit location
• `/dq command:fear ephemeral:true` - Private fear check";

        /// <summary>
        /// Help content for the /heroes command
        /// </summary>
        public static readonly string HeroesCommandHelp = @"**Heroes Game System Commands**

**Usage:** `/heroes command:<action> [modifier:<value>] [ephemeral:true/false] [help:true]`

**Available Commands:**
• `speed` - Roll on the speed table

**Examples:**
• `/heroes command:speed` - Roll for speed effect
• `/heroes command:speed ephemeral:true` - Private speed roll";

        /// <summary>
        /// Help content for the /party command
        /// </summary>
        public static readonly string PartyCommandHelp = @"**Party Management Commands**

**Usage:** `/party action:<action> [parameter:<value>] [ephemeral:true/false] [help:true]`

**Available Actions:**
• `create <party-name>` - Create a new party
• `join <party-name>` - Join an existing party
• `leave` - Leave your current party
• `list` - List all available parties
• `members` - Show members of your current party
• `delete <party-name>` - Delete a party (creator only)
• `invite <@user>` - Invite a user to your party
• `kick <@user>` - Remove a user from your party (creator only)

**Examples:**
• `/party action:create parameter:AwesomeAdventurers` - Create party
• `/party action:join parameter:AwesomeAdventurers` - Join party  
• `/party action:list` - See all parties
• `/party action:members` - See party members
• `/party action:leave` - Leave current party";

        /// <summary>
        /// Help content for the /settings command
        /// </summary>
        public static readonly string SettingsCommandHelp = @"**User Settings Management**

**Usage:** `/settings action:<action> [setting:<name>] [value:<value>] [help:true]`

**Available Actions:**
• `set <setting> <value>` - Set a setting value
• `get <setting>` - Get current setting value  
• `delete <setting>` - Remove a setting
• `list` - List all your settings

**Common Settings:**
• `showDiceButtons` - Show interactive dice buttons (true/false)
• `showHitLocations` - Show hit location details (true/false)
• `showSpecialButtons` - Show special action buttons (true/false)

**Examples:**
• `/settings action:set setting:showDiceButtons value:true`
• `/settings action:get setting:showDiceButtons`
• `/settings action:list` - Show all settings
• `/settings action:delete setting:showDiceButtons`";

        /// <summary>
        /// Help content for the /plugin command
        /// </summary>
        public static readonly string PluginCommandHelp = @"**Plugin Management Commands**

**Usage:** `/plugin action:<action> [parameter:<value>] [help:true]`

**Available Actions:**
• `list` - List all available plugins and their status
• `info <plugin-id>` - Show detailed information about a plugin
• `reload` - Reload all plugins
• `reload <plugin-id>` - Reload a specific plugin
• `help` - Show plugin command help

**Plugin Status Indicators:**
• 🟢 - Plugin loaded and running
• 🔴 - Plugin error or failed to load
• ⚫ - Plugin not loaded

**Examples:**
• `/plugin action:list` - See all plugins
• `/plugin action:info parameter:CallOfCthulhu` - Plugin details
• `/plugin action:reload` - Reload all plugins
• `/plugin action:reload parameter:CallOfCthulhu` - Reload specific plugin

**Note:** Plugin management requires appropriate permissions.";

        /// <summary>
        /// Comprehensive dice notation reference
        /// </summary>
        public static readonly string DiceNotationReference = @"**Complete Dice Notation Reference**

**Basic Dice:**
• `d4, d6, d8, d10, d12, d20, d100` - Standard dice
• `3d6` - Roll three 6-sided dice
• `2d10+5` - Roll with modifier

**Exploding Dice:**
• `3d6!` - Explode on maximum (re-roll and add)
• `d10!!` - Double exploding

**Functions:**
• `min(expression, value)` - Use minimum of roll or value
• `max(expression, value)` - Use maximum of roll or value  
• `repeat(expression, count)` - Repeat expression multiple times
• `step <value>` - Step function for thresholds

**Arithmetic:**
• `+, -, *, /` - Basic operations
• `()` - Parentheses for grouping
• `(2d8 + 3) * 2` - Complex expressions

**Comparisons:**
• `>, >=, <, <=, ==, !=` - Comparison operators
• `3d6 > 10` - Returns 1 if true, 0 if false
• `d20 >= 15` - Success/failure checks

**Game System Commands:**
• `backfire, injury, fear, speed, hitloc` - Game-specific rolls
• Can be used in `/roll` or game-specific commands

**User Aliases:**
• `addalias <name> <expression>` - Create custom alias
• `removealias <name>` - Remove alias
• `listalias` - Show all your aliases

**Comments:**
• `d20+5 # Attack roll` - Add descriptive comments

**Advanced Examples:**
• `repeat(min(3d6!+2, 20), 4)` - Four capped exploding rolls
• `(4d10! > 30) * 10` - Conditional bonus
• `3d6 + max(d8-2, 0) # Damage with minimum 0`";

        /// <summary>
        /// Help content for aliases and advanced features
        /// </summary>
        public static readonly string AdvancedFeaturesHelp = @"**Advanced Features**

**User Aliases:**
Create custom shortcuts for complex expressions:
• `addalias attack d20+5+2 # My attack roll`
• `addalias fireball 8d6 # Fireball damage`
• Use: `/roll attack` or `/roll fireball`

**Progressive Disclosure:**
The bot automatically adapts output complexity:
• **Simple rolls** → Minimal output
• **Complex expressions** → Detailed breakdowns
• **Very complex** → Tree structure with full details

**Format Control:**
• `format:auto` - Let the bot choose (recommended)
• `format:terse` - Just the result
• `format:standard` - Basic breakdown
• `format:detailed` - Full tree structure

**Interactive Features:**
• **Re-roll Button** - Roll the same expression again
• **Details Button** - Toggle between formats
• **Copy Button** - Copy result to clipboard

**Lucky Numbers:**
Add lucky numbers to your Discord nickname: `[7,13,20]`
These numbers will be highlighted with ✨ in your rolls!

**Tips:**
• Use `ephemeral:true` for private results
• Complex expressions work better with `format:detailed`
• Comments help explain what you're rolling for
• Aliases save time for frequently used expressions";
    }
}