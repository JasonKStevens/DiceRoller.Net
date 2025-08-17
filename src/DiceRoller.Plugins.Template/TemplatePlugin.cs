using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;
using DiceRoller.Plugins.Template.GameSystem;
using DiceRoller.Plugins.Template.Commands;
using DiceRoller.Plugins.Template.Tables;

namespace DiceRoller.Plugins.Template
{
    /// <summary>
    /// Template plugin demonstrating how to create a game system plugin for DiceRoller.Net
    /// 
    /// This plugin provides examples of:
    /// - Plugin initialization and lifecycle management
    /// - Command implementation with parameter handling
    /// - Lookup table implementation with range-based data
    /// - Help system integration
    /// - Error handling and logging
    /// - Configuration management
    /// 
    /// To create your own plugin:
    /// 1. Copy this template to a new directory
    /// 2. Rename the namespace and classes
    /// 3. Update the Plugin.manifest.json with your information
    /// 4. Implement your game-specific commands and tables
    /// 5. Test thoroughly before deployment
    /// </summary>
    [Plugin("template", "Template Game System", "1.0.0",
        Description = "A template plugin for creating DiceRoller.Net game system plugins. Provides examples of commands, tables, and game mechanics.",
        Author = "Your Name",
        Tags = new[] { "template", "example", "rpg", "generic" })]
    [GameSystemPlugin("Template System", GameSystemVersion = "1.0", Publisher = "Template Publishing")]
    public class TemplatePlugin : PluginBase, IGameSystemPlugin
    {
        private TemplateGameSystem? _gameSystem;
        private readonly List<GameSystemCommand> _commands = new();
        private readonly List<GameSystemTable> _tables = new();

        #region Plugin Properties

        /// <summary>
        /// Unique identifier for the plugin - should match manifest.json
        /// </summary>
        public override string Id => "template";

        /// <summary>
        /// Display name shown in plugin lists
        /// </summary>
        public override string Name => "Template Game System";

        /// <summary>
        /// Plugin version - should match manifest.json
        /// </summary>
        public override Version Version => new(1, 0, 0);

        /// <summary>
        /// Brief description of what this plugin does
        /// </summary>
        public override string Description => "A template plugin for creating DiceRoller.Net game system plugins. Provides examples of commands, tables, and game mechanics.";

        /// <summary>
        /// Plugin author name
        /// </summary>
        public override string Author => "Your Name";

        /// <summary>
        /// Game system name this plugin supports
        /// </summary>
        public string GameSystemName => "Template System";

        /// <summary>
        /// Version of the game system supported
        /// </summary>
        public string GameSystemVersion => "1.0";

        /// <summary>
        /// Available commands provided by this plugin
        /// </summary>
        public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();

        /// <summary>
        /// Available lookup tables provided by this plugin
        /// </summary>
        public IReadOnlyList<GameSystemTable> Tables => _tables.AsReadOnly();

        #endregion

        #region Plugin Lifecycle

        /// <summary>
        /// Initialize the plugin - called once when the plugin is loaded
        /// This is where you should:
        /// - Initialize your game system
        /// - Register commands and tables
        /// - Set up any required services
        /// - Load configuration settings
        /// </summary>
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Initializing Template game system plugin");

            try
            {
                // Load configuration settings
                var enableExampleFeatures = GetConfigurationValue("enableExampleFeatures", true);
                var defaultDifficulty = GetConfigurationValue("defaultDifficulty", "normal");
                
                Logger.LogDebug("Plugin configuration: EnableExample={EnableExample}, DefaultDifficulty={DefaultDifficulty}", 
                    enableExampleFeatures, defaultDifficulty);

                // Initialize the game system with configuration
                _gameSystem = new TemplateGameSystem(Logger, enableExampleFeatures, defaultDifficulty ?? "normal");

                // Register all available commands
                RegisterCommands();

                // Register all available lookup tables
                RegisterTables();

                Logger.LogInformation("Template plugin initialized with {CommandCount} commands and {TableCount} tables", 
                    _commands.Count, _tables.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize Template plugin");
                throw;
            }
        }

        /// <summary>
        /// Called when the plugin is started - plugin is ready to process requests
        /// </summary>
        protected override async Task OnStartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Template plugin started and ready for commands");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when the plugin is stopped - cleanup any running operations
        /// </summary>
        protected override async Task OnStopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Template plugin stopped");
            await Task.CompletedTask;
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Execute a game system command
        /// This method routes commands to appropriate handlers
        /// </summary>
        public async Task<GameSystemCommandResult> ExecuteCommandAsync(string commandName, 
            Dictionary<string, object> parameters, 
            GameSystemContext context)
        {
            if (_gameSystem == null)
                throw new InvalidOperationException("Game system not initialized");

            try
            {
                Logger.LogDebug("Executing Template command: {CommandName} for user {UserId}", 
                    commandName, context.UserId);

                return commandName.ToLowerInvariant() switch
                {
                    // Example commands - add your own here
                    "skillcheck" or "skill" => await _gameSystem.SkillCheckCommand.ExecuteAsync(parameters, context),
                    "initiative" or "init" => await _gameSystem.InitiativeCommand.ExecuteAsync(parameters, context),
                    "damage" => await _gameSystem.DamageCommand.ExecuteAsync(parameters, context),
                    "healing" or "heal" => await _gameSystem.HealingCommand.ExecuteAsync(parameters, context),
                    
                    // Unknown command
                    _ => new GameSystemCommandResult(false, null, string.Empty, $"Unknown command: {commandName}")
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing Template command: {CommandName}", commandName);
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        #endregion

        #region Table Lookup

        /// <summary>
        /// Lookup a value in a game system table
        /// This method routes table lookups to appropriate handlers
        /// </summary>
        public async Task<GameSystemTableResult> LookupTableAsync(string tableName, 
            object key, 
            GameSystemContext context)
        {
            if (_gameSystem == null)
                throw new InvalidOperationException("Game system not initialized");

            try
            {
                Logger.LogDebug("Looking up Template table: {TableName} with key: {Key}", tableName, key);

                return tableName.ToLowerInvariant() switch
                {
                    // Example tables - add your own here
                    "criticals" or "critical_hits" => await _gameSystem.CriticalHitsTable.LookupAsync(key, context),
                    "fumbles" or "critical_failures" => await _gameSystem.FumblesTable.LookupAsync(key, context),
                    "weather" => await _gameSystem.WeatherTable.LookupAsync(key, context),
                    "encounters" or "random_encounters" => await _gameSystem.EncountersTable.LookupAsync(key, context),
                    
                    // Unknown table
                    _ => new GameSystemTableResult(false, null, string.Empty, $"Unknown table: {tableName}")
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error looking up Template table: {TableName}", tableName);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        #endregion

        #region Help System

        /// <summary>
        /// Get help information for commands and tables
        /// This provides users with usage information
        /// </summary>
        public GameSystemHelpInfo GetHelp(string? commandName = null)
        {
            var commandHelps = new List<CommandHelpInfo>();
            var tableHelps = new List<TableHelpInfo>();

            if (commandName == null)
            {
                // Return help for all commands
                commandHelps.AddRange(new[]
                {
                    new CommandHelpInfo(
                        "skillcheck",
                        "Perform a skill check with difficulty modifiers",
                        "skillcheck <skill_value> [difficulty] [modifier]",
                        "skillcheck 75 - Standard skill check\nskillcheck 60 hard - Hard skill check\nskillcheck 80 normal +10 - Skill check with bonus",
                        new List<ParameterHelpInfo>
                        {
                            new("skill_value", "int", true, "Character's skill value (0-100)", null),
                            new("difficulty", "string", false, "Difficulty level (easy, normal, hard, extreme)", "normal"),
                            new("modifier", "int", false, "Bonus or penalty to apply", "0")
                        }),
                    new CommandHelpInfo(
                        "initiative",
                        "Roll initiative for combat order",
                        "initiative [modifier] [advantage]",
                        "initiative - Standard initiative\ninitiative +3 - Initiative with bonus\ninitiative 0 advantage - Initiative with advantage",
                        new List<ParameterHelpInfo>
                        {
                            new("modifier", "int", false, "Bonus or penalty to initiative", "0"),
                            new("advantage", "bool", false, "Roll with advantage (take higher of two d20s)", "false")
                        }),
                    new CommandHelpInfo(
                        "damage",
                        "Calculate damage with various modifiers",
                        "damage <dice_expression> [damage_type] [resistance]",
                        "damage 2d6+3 - Basic damage\ndamage 1d8+2 fire - Fire damage\ndamage 3d6 cold resistant - Cold damage vs resistance",
                        new List<ParameterHelpInfo>
                        {
                            new("dice_expression", "string", true, "Damage dice expression", null),
                            new("damage_type", "string", false, "Type of damage (physical, fire, cold, etc.)", "physical"),
                            new("resistance", "string", false, "Target's resistance (none, resistant, immune)", "none")
                        })
                });

                tableHelps.AddRange(new[]
                {
                    new TableHelpInfo(
                        "criticals",
                        "Critical hit effects for combat",
                        "criticals <d20_roll>",
                        "criticals 18 - Look up critical hit effect"),
                    new TableHelpInfo(
                        "fumbles",
                        "Critical failure effects",
                        "fumbles <d20_roll>",
                        "fumbles 3 - Look up fumble effect"),
                    new TableHelpInfo(
                        "weather",
                        "Random weather generation",
                        "weather <d100_roll>",
                        "weather 45 - Generate weather condition"),
                    new TableHelpInfo(
                        "encounters",
                        "Random encounter table",
                        "encounters <d20_roll>",
                        "encounters 12 - Generate random encounter")
                });
            }
            else
            {
                // Return help for specific command
                var allCommands = new[]
                {
                    commandHelps[0], // Will be populated above
                };
                
                var specificCommand = Array.Find(allCommands, c => 
                    c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
                
                if (specificCommand != null)
                {
                    commandHelps.Add(specificCommand);
                }
            }

            return new GameSystemHelpInfo(
                GameSystemName,
                "Template game system providing examples of common RPG mechanics including skill checks, initiative, damage calculation, and lookup tables.",
                commandHelps,
                tableHelps);
        }

        #endregion

        #region Registration Methods

        /// <summary>
        /// Register all commands provided by this plugin
        /// Add your own commands here following the same pattern
        /// </summary>
        private void RegisterCommands()
        {
            // Skill check command
            _commands.Add(new GameSystemCommand(
                "skillcheck",
                "Perform a skill check with difficulty modifiers",
                new List<GameSystemParameter>
                {
                    new("skill_value", typeof(int), true, null, "Character's skill value (0-100)"),
                    new("difficulty", typeof(string), false, "normal", "Difficulty level (easy, normal, hard, extreme)"),
                    new("modifier", typeof(int), false, 0, "Bonus or penalty to apply")
                },
                "Skill checks determine success or failure of character actions based on their skill values and situational modifiers.",
                true));

            // Initiative command
            _commands.Add(new GameSystemCommand(
                "initiative",
                "Roll initiative for combat order",
                new List<GameSystemParameter>
                {
                    new("modifier", typeof(int), false, 0, "Bonus or penalty to initiative"),
                    new("advantage", typeof(bool), false, false, "Roll with advantage (take higher of two d20s)")
                },
                "Initiative determines the order of actions in combat.",
                true));

            // Damage command
            _commands.Add(new GameSystemCommand(
                "damage",
                "Calculate damage with various modifiers",
                new List<GameSystemParameter>
                {
                    new("dice_expression", typeof(string), true, null, "Damage dice expression (e.g., 2d6+3)"),
                    new("damage_type", typeof(string), false, "physical", "Type of damage (physical, fire, cold, etc.)"),
                    new("resistance", typeof(string), false, "none", "Target's resistance (none, resistant, immune)")
                },
                "Calculate damage with type-specific modifiers and resistance effects.",
                true));

            // Healing command  
            _commands.Add(new GameSystemCommand(
                "healing",
                "Calculate healing with various modifiers",
                new List<GameSystemParameter>
                {
                    new("dice_expression", typeof(string), true, null, "Healing dice expression (e.g., 1d8+2)"),
                    new("healing_type", typeof(string), false, "magical", "Type of healing (magical, natural, potion)"),
                    new("enhancement", typeof(int), false, 0, "Enhancement bonus to healing")
                },
                "Calculate healing amounts with type-specific bonuses.",
                true));
        }

        /// <summary>
        /// Register all lookup tables provided by this plugin
        /// Add your own tables here following the same pattern
        /// </summary>
        private void RegisterTables()
        {
            // Critical hits table
            _tables.Add(new GameSystemTable(
                "criticals",
                "Critical hit effects for exceptional attack rolls",
                typeof(int),
                typeof(string),
                "Provides special effects when attacks score critical hits."));

            // Fumbles table
            _tables.Add(new GameSystemTable(
                "fumbles",
                "Critical failure effects for botched rolls",
                typeof(int),
                typeof(string),
                "Provides complications when attacks or actions fail critically."));

            // Weather table
            _tables.Add(new GameSystemTable(
                "weather",
                "Random weather conditions for outdoor adventures",
                typeof(int),
                typeof(string),
                "Generates weather conditions that can affect gameplay."));

            // Random encounters table
            _tables.Add(new GameSystemTable(
                "encounters",
                "Random encounters for wilderness travel",
                typeof(int),
                typeof(string),
                "Generates unexpected encounters during travel or exploration."));
        }

        #endregion

        #region Health and Configuration

        /// <summary>
        /// Provide detailed health status for monitoring
        /// </summary>
        protected override PluginHealthStatus? OnGetHealthStatus()
        {
            var isHealthy = _gameSystem != null && Status == PluginStatus.Running;
            
            return new PluginHealthStatus(
                isHealthy,
                isHealthy ? "Running normally" : "Not initialized or stopped",
                null,
                new Dictionary<string, object>
                {
                    ["gameSystemLoaded"] = _gameSystem != null,
                    ["commandCount"] = _commands.Count,
                    ["tableCount"] = _tables.Count,
                    ["gameSystemName"] = GameSystemName,
                    ["gameSystemVersion"] = GameSystemVersion
                });
        }

        /// <summary>
        /// Handle configuration changes at runtime
        /// </summary>
        protected override async Task OnConfigurationUpdatedAsync(object configuration)
        {
            Logger.LogInformation("Template plugin configuration updated");
            
            // Reload configuration-dependent components if needed
            if (_gameSystem != null)
            {
                var enableExampleFeatures = GetConfigurationValue("enableExampleFeatures", true);
                var defaultDifficulty = GetConfigurationValue("defaultDifficulty", "normal");
                
                _gameSystem.UpdateConfiguration(enableExampleFeatures, defaultDifficulty ?? "normal");
            }
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup resources when plugin is disposed
        /// </summary>
        protected override void OnDispose()
        {
            _gameSystem = null;
            _commands.Clear();
            _tables.Clear();
        }

        #endregion
    }
}