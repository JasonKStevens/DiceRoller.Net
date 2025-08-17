using System;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Template.Commands;
using DiceRoller.Plugins.Template.Tables;

namespace DiceRoller.Plugins.Template.GameSystem
{
    /// <summary>
    /// Template game system class that coordinates commands and tables
    /// 
    /// This class demonstrates:
    /// - How to organize game system components
    /// - Dependency injection patterns for commands and tables
    /// - Configuration management
    /// - Logging integration
    /// 
    /// In your own plugin:
    /// - Rename this class to match your game system
    /// - Add your specific commands and tables
    /// - Include game-specific logic and rules
    /// - Handle configuration updates appropriately
    /// </summary>
    public class TemplateGameSystem
    {
        private readonly ILogger _logger;
        private bool _enableExampleFeatures;
        private string _defaultDifficulty;

        #region Commands

        /// <summary>
        /// Skill check command handler
        /// </summary>
        public SkillCheckCommand SkillCheckCommand { get; private set; } = null!;

        /// <summary>
        /// Initiative command handler
        /// </summary>
        public InitiativeCommand InitiativeCommand { get; private set; } = null!;

        /// <summary>
        /// Damage calculation command handler
        /// </summary>
        public DamageCommand DamageCommand { get; private set; } = null!;

        /// <summary>
        /// Healing command handler
        /// </summary>
        public HealingCommand HealingCommand { get; private set; } = null!;

        #endregion

        #region Tables

        /// <summary>
        /// Critical hits lookup table
        /// </summary>
        public CriticalHitsTable CriticalHitsTable { get; private set; } = null!;

        /// <summary>
        /// Fumbles/critical failures lookup table
        /// </summary>
        public FumblesTable FumblesTable { get; private set; } = null!;

        /// <summary>
        /// Weather conditions lookup table
        /// </summary>
        public WeatherTable WeatherTable { get; private set; } = null!;

        /// <summary>
        /// Random encounters lookup table
        /// </summary>
        public EncountersTable EncountersTable { get; private set; } = null!;

        #endregion

        /// <summary>
        /// Initialize the template game system
        /// </summary>
        /// <param name="logger">Logger for the game system</param>
        /// <param name="enableExampleFeatures">Whether to enable example features</param>
        /// <param name="defaultDifficulty">Default difficulty for checks</param>
        public TemplateGameSystem(ILogger logger, bool enableExampleFeatures = true, string defaultDifficulty = "normal")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _enableExampleFeatures = enableExampleFeatures;
            _defaultDifficulty = defaultDifficulty ?? "normal";

            InitializeCommands();
            InitializeTables();

            _logger.LogDebug("Template game system initialized with default difficulty: {DefaultDifficulty}, features enabled: {FeaturesEnabled}",
                _defaultDifficulty, _enableExampleFeatures);
        }

        /// <summary>
        /// Update configuration at runtime
        /// </summary>
        /// <param name="enableExampleFeatures">Whether to enable example features</param>
        /// <param name="defaultDifficulty">Default difficulty for checks</param>
        public void UpdateConfiguration(bool enableExampleFeatures, string defaultDifficulty)
        {
            _logger.LogInformation("Updating template game system configuration");
            
            _enableExampleFeatures = enableExampleFeatures;
            _defaultDifficulty = defaultDifficulty ?? "normal";

            // Update commands with new configuration
            SkillCheckCommand.UpdateDefaultDifficulty(_defaultDifficulty);
            
            _logger.LogDebug("Configuration updated: default difficulty={DefaultDifficulty}, features enabled={FeaturesEnabled}",
                _defaultDifficulty, _enableExampleFeatures);
        }

        /// <summary>
        /// Initialize all command handlers
        /// </summary>
        private void InitializeCommands()
        {
            _logger.LogDebug("Initializing template game system commands");

            // Initialize commands with dependencies
            SkillCheckCommand = new SkillCheckCommand(_logger, _defaultDifficulty);
            InitiativeCommand = new InitiativeCommand(_logger);
            DamageCommand = new DamageCommand(_logger);
            HealingCommand = new HealingCommand(_logger);

            _logger.LogDebug("Initialized {CommandCount} commands", 4);
        }

        /// <summary>
        /// Initialize all lookup tables
        /// </summary>
        private void InitializeTables()
        {
            _logger.LogDebug("Initializing template game system tables");

            // Initialize tables with logger
            CriticalHitsTable = new CriticalHitsTable(_logger);
            FumblesTable = new FumblesTable(_logger);
            WeatherTable = new WeatherTable(_logger);
            EncountersTable = new EncountersTable(_logger);

            _logger.LogDebug("Initialized {TableCount} tables", 4);
        }

        #region Game System Rules and Utilities

        /// <summary>
        /// Calculate difficulty modifier for skill checks
        /// This is an example of game-specific logic that commands can use
        /// </summary>
        /// <param name="difficulty">Difficulty level name</param>
        /// <returns>Modifier to apply to the target number</returns>
        public static int GetDifficultyModifier(string difficulty)
        {
            return difficulty.ToLowerInvariant() switch
            {
                "trivial" => +30,
                "easy" => +20,
                "normal" or "standard" => 0,
                "hard" => -20,
                "extreme" => -30,
                "impossible" => -50,
                _ => 0
            };
        }

        /// <summary>
        /// Determine success level based on roll and target
        /// This shows how to implement game-specific success mechanics
        /// </summary>
        /// <param name="roll">The dice roll result</param>
        /// <param name="target">The target number to beat</param>
        /// <returns>Success level description</returns>
        public static string GetSuccessLevel(int roll, int target)
        {
            if (roll >= 96) // Always fail on 96+
                return "Critical Failure";
            
            if (roll <= 5) // Always succeed on 5 or less
                return "Critical Success";

            var margin = target - roll;
            
            return margin switch
            {
                >= 30 => "Critical Success",
                >= 20 => "Hard Success", 
                >= 0 => "Success",
                >= -10 => "Failure",
                _ => "Critical Failure"
            };
        }

        /// <summary>
        /// Calculate initiative order value
        /// Example of combat mechanics that could be game-specific
        /// </summary>
        /// <param name="baseRoll">Base d20 roll</param>
        /// <param name="modifier">Character's initiative modifier</param>
        /// <param name="advantage">Whether rolling with advantage</param>
        /// <returns>Final initiative value</returns>
        public static int CalculateInitiative(int baseRoll, int modifier, bool advantage = false)
        {
            var finalRoll = baseRoll;
            
            if (advantage)
            {
                // With advantage, roll a second d20 and take the higher
                var secondRoll = new Random().Next(1, 21);
                finalRoll = Math.Max(baseRoll, secondRoll);
            }
            
            return finalRoll + modifier;
        }

        /// <summary>
        /// Apply damage resistance/immunity
        /// Example of damage calculation with type-specific modifiers
        /// </summary>
        /// <param name="baseDamage">Base damage amount</param>
        /// <param name="damageType">Type of damage</param>
        /// <param name="resistance">Target's resistance level</param>
        /// <returns>Final damage after resistance</returns>
        public static int ApplyDamageResistance(int baseDamage, string damageType, string resistance)
        {
            var finalDamage = baseDamage;
            
            switch (resistance.ToLowerInvariant())
            {
                case "resistant":
                    finalDamage = finalDamage / 2; // Half damage
                    break;
                case "immune":
                    finalDamage = 0; // No damage
                    break;
                case "vulnerable":
                    finalDamage = finalDamage * 2; // Double damage
                    break;
                default:
                    // No resistance - full damage
                    break;
            }
            
            return Math.Max(0, finalDamage); // Never negative damage
        }

        #endregion
    }
}