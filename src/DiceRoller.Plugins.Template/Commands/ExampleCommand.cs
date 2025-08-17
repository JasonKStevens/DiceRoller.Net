using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;
using DiceRoller.Plugins.Template.GameSystem;

namespace DiceRoller.Plugins.Template.Commands
{
    /// <summary>
    /// Example skill check command demonstrating parameter handling and game logic
    /// 
    /// This command shows:
    /// - Parameter extraction and validation
    /// - Game-specific logic implementation
    /// - Error handling and logging
    /// - Result formatting for Discord
    /// - Configuration integration
    /// </summary>
    public class SkillCheckCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();
        private string _defaultDifficulty;

        public SkillCheckCommand(ILogger logger, string defaultDifficulty = "normal")
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultDifficulty = defaultDifficulty ?? "normal";
        }

        /// <summary>
        /// Update the default difficulty setting
        /// </summary>
        public void UpdateDefaultDifficulty(string defaultDifficulty)
        {
            _defaultDifficulty = defaultDifficulty ?? "normal";
        }

        /// <summary>
        /// Execute a skill check command
        /// Example: skillcheck 75 hard +10
        /// </summary>
        [Command("skillcheck", Description = "Perform a skill check with difficulty modifiers")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            [CommandParameter(Description = "Parameters for skill check")] Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                // Extract and validate skill value (required)
                if (!ExtractIntParameter(parameters, "skill_value", out var skillValue))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, 
                        "Skill value is required (0-100)");
                }

                if (skillValue < 0 || skillValue > 100)
                {
                    return new GameSystemCommandResult(false, null, string.Empty, 
                        "Skill value must be between 0 and 100");
                }

                // Extract optional parameters
                var difficulty = ExtractStringParameter(parameters, "difficulty", _defaultDifficulty);
                var modifier = ExtractIntParameter(parameters, "modifier", 0);

                // Perform the skill check
                var roll = _random.Next(1, 101); // d100
                var difficultyModifier = TemplateGameSystem.GetDifficultyModifier(difficulty);
                var adjustedTarget = Math.Min(95, Math.Max(5, skillValue + difficultyModifier + modifier));
                
                var success = roll <= adjustedTarget;
                var successLevel = TemplateGameSystem.GetSuccessLevel(roll, adjustedTarget);

                var result = new SkillCheckResult
                {
                    Roll = roll,
                    OriginalSkill = skillValue,
                    AdjustedTarget = adjustedTarget,
                    Difficulty = difficulty,
                    Modifier = modifier,
                    DifficultyModifier = difficultyModifier,
                    Success = success,
                    SuccessLevel = successLevel
                };

                var description = FormatSkillCheckResult(result);

                _logger.LogDebug("Skill check executed: Skill={Skill}, Roll={Roll}, Target={Target}, Success={Success}", 
                    skillValue, roll, adjustedTarget, success);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing skill check command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string FormatSkillCheckResult(SkillCheckResult result)
        {
            var difficultyText = result.Difficulty != "normal" ? $" ({result.Difficulty})" : "";
            var modifierText = result.Modifier != 0 ? $" {result.Modifier:+0;-#}" : "";
            var statusEmoji = result.Success ? "✅" : "❌";
            
            var description = $"{statusEmoji} **Skill Check{difficultyText}:** {result.SuccessLevel}\n";
            description += $"🎲 **Roll:** {result.Roll} vs {result.AdjustedTarget}";
            
            if (result.OriginalSkill != result.AdjustedTarget)
            {
                description += $" (base {result.OriginalSkill}";
                if (result.DifficultyModifier != 0)
                    description += $", difficulty {result.DifficultyModifier:+0;-#}";
                if (result.Modifier != 0)
                    description += $", modifier {result.Modifier:+0;-#}";
                description += ")";
            }

            // Add success level explanation
            description += $"\n📊 **Result:** {result.SuccessLevel}";
            
            // Add additional context based on success level
            switch (result.SuccessLevel)
            {
                case "Critical Success":
                    description += " - Exceptional result with bonus effects!";
                    break;
                case "Hard Success":
                    description += " - Success with style!";
                    break;
                case "Success":
                    description += " - Standard success";
                    break;
                case "Failure":
                    description += " - No significant consequences";
                    break;
                case "Critical Failure":
                    description += " - Something went very wrong!";
                    break;
            }

            return description;
        }

        #region Parameter Extraction Helpers

        private bool ExtractIntParameter(Dictionary<string, object> parameters, string key, out int value)
        {
            value = 0;
            if (!parameters.TryGetValue(key, out var obj))
                return false;

            return int.TryParse(obj.ToString(), out value);
        }

        private int ExtractIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (ExtractIntParameter(parameters, key, out var value))
                return value;
            return defaultValue;
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        #endregion
    }

    /// <summary>
    /// Example initiative command for combat ordering
    /// </summary>
    public class InitiativeCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public InitiativeCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Command("initiative", Description = "Roll initiative for combat order")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                var modifier = ExtractIntParameter(parameters, "modifier", 0);
                var advantage = ExtractBoolParameter(parameters, "advantage", false);

                var baseRoll = _random.Next(1, 21); // d20
                var finalInitiative = TemplateGameSystem.CalculateInitiative(baseRoll, modifier, advantage);

                var result = new InitiativeResult
                {
                    BaseRoll = baseRoll,
                    Modifier = modifier,
                    Advantage = advantage,
                    FinalInitiative = finalInitiative
                };

                var description = FormatInitiativeResult(result);

                _logger.LogDebug("Initiative rolled: Roll={Roll}, Modifier={Modifier}, Final={Final}", 
                    baseRoll, modifier, finalInitiative);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing initiative command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string FormatInitiativeResult(InitiativeResult result)
        {
            var description = $"⚔️ **Initiative:** {result.FinalInitiative}\n";
            description += $"🎲 **Roll:** {result.BaseRoll}";
            
            if (result.Advantage)
                description += " (with advantage)";
            
            if (result.Modifier != 0)
                description += $" + {result.Modifier} = {result.FinalInitiative}";

            return description;
        }

        private int ExtractIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var obj))
                return defaultValue;

            if (int.TryParse(obj.ToString(), out var value))
                return value;

            return defaultValue;
        }

        private bool ExtractBoolParameter(Dictionary<string, object> parameters, string key, bool defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            if (value is bool boolValue)
                return boolValue;

            if (bool.TryParse(value.ToString(), out var parsedBool))
                return parsedBool;

            return defaultValue;
        }
    }

    /// <summary>
    /// Example damage calculation command with resistance handling
    /// </summary>
    public class DamageCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public DamageCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Command("damage", Description = "Calculate damage with various modifiers")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                if (!parameters.TryGetValue("dice_expression", out var diceObj))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, 
                        "Dice expression is required (e.g., 2d6+3)");
                }

                var diceExpression = diceObj.ToString() ?? "";
                var damageType = ExtractStringParameter(parameters, "damage_type", "physical");
                var resistance = ExtractStringParameter(parameters, "resistance", "none");

                // Simple dice parsing (in a real plugin, use proper dice parser)
                var baseDamage = ParseDiceExpression(diceExpression);
                var finalDamage = TemplateGameSystem.ApplyDamageResistance(baseDamage, damageType, resistance);

                var result = new DamageResult
                {
                    DiceExpression = diceExpression,
                    BaseDamage = baseDamage,
                    DamageType = damageType,
                    Resistance = resistance,
                    FinalDamage = finalDamage
                };

                var description = FormatDamageResult(result);

                _logger.LogDebug("Damage calculated: Expression={Expression}, Base={Base}, Final={Final}", 
                    diceExpression, baseDamage, finalDamage);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing damage command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string FormatDamageResult(DamageResult result)
        {
            var typeEmoji = result.DamageType.ToLowerInvariant() switch
            {
                "fire" => "🔥",
                "cold" or "ice" => "❄️",
                "lightning" or "electric" => "⚡",
                "poison" => "☠️",
                "acid" => "🧪",
                _ => "⚔️"
            };

            var description = $"{typeEmoji} **{result.DamageType.ToTitleCase()} Damage:** {result.FinalDamage}\n";
            description += $"🎲 **Roll:** {result.DiceExpression} = {result.BaseDamage}";
            
            if (result.BaseDamage != result.FinalDamage)
            {
                var resistanceText = result.Resistance switch
                {
                    "resistant" => " (halved by resistance)",
                    "immune" => " (negated by immunity)",
                    "vulnerable" => " (doubled by vulnerability)",
                    _ => ""
                };
                description += $" → {result.FinalDamage}{resistanceText}";
            }

            return description;
        }

        private int ParseDiceExpression(string expression)
        {
            // Simplified dice parsing - in a real implementation, use proper dice parser
            try
            {
                // Handle simple patterns like "2d6+3", "1d8", "3d4-1"
                var parts = expression.ToLowerInvariant().Replace(" ", "");
                var total = 0;
                var modifier = 0;

                // Extract modifier
                if (parts.Contains('+'))
                {
                    var splitPlus = parts.Split('+');
                    parts = splitPlus[0];
                    if (splitPlus.Length > 1 && int.TryParse(splitPlus[1], out var plusMod))
                        modifier = plusMod;
                }
                else if (parts.Contains('-'))
                {
                    var splitMinus = parts.Split('-');
                    parts = splitMinus[0];
                    if (splitMinus.Length > 1 && int.TryParse(splitMinus[1], out var minusMod))
                        modifier = -minusMod;
                }

                // Parse dice
                if (parts.Contains('d'))
                {
                    var diceParts = parts.Split('d');
                    var count = string.IsNullOrEmpty(diceParts[0]) ? 1 : int.Parse(diceParts[0]);
                    var sides = int.Parse(diceParts[1]);

                    for (int i = 0; i < count; i++)
                    {
                        total += _random.Next(1, sides + 1);
                    }
                }
                else
                {
                    // Just a number
                    total = int.Parse(parts);
                }

                return Math.Max(0, total + modifier);
            }
            catch
            {
                return 1; // Fallback
            }
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }
    }

    /// <summary>
    /// Example healing command with type-specific bonuses
    /// </summary>
    public class HealingCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public HealingCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Command("healing", Description = "Calculate healing with various modifiers")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                if (!parameters.TryGetValue("dice_expression", out var diceObj))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, 
                        "Healing dice expression is required (e.g., 1d8+2)");
                }

                var diceExpression = diceObj.ToString() ?? "";
                var healingType = ExtractStringParameter(parameters, "healing_type", "magical");
                var enhancement = ExtractIntParameter(parameters, "enhancement", 0);

                // Parse healing amount (reuse dice parsing logic)
                var baseHealing = ParseDiceExpression(diceExpression);
                var typeBonus = GetHealingTypeBonus(healingType);
                var finalHealing = Math.Max(1, baseHealing + typeBonus + enhancement);

                var result = new HealingResult
                {
                    DiceExpression = diceExpression,
                    BaseHealing = baseHealing,
                    HealingType = healingType,
                    TypeBonus = typeBonus,
                    Enhancement = enhancement,
                    FinalHealing = finalHealing
                };

                var description = FormatHealingResult(result);

                _logger.LogDebug("Healing calculated: Expression={Expression}, Type={Type}, Final={Final}", 
                    diceExpression, healingType, finalHealing);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing healing command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string FormatHealingResult(HealingResult result)
        {
            var typeEmoji = result.HealingType.ToLowerInvariant() switch
            {
                "magical" => "✨",
                "natural" => "🌿",
                "potion" => "🧪",
                "divine" => "🙏",
                _ => "💚"
            };

            var description = $"{typeEmoji} **{result.HealingType.ToTitleCase()} Healing:** {result.FinalHealing} HP\n";
            description += $"🎲 **Roll:** {result.DiceExpression} = {result.BaseHealing}";
            
            if (result.TypeBonus != 0 || result.Enhancement != 0)
            {
                description += " + ";
                var bonuses = new List<string>();
                if (result.TypeBonus != 0)
                    bonuses.Add($"{result.TypeBonus} ({result.HealingType})");
                if (result.Enhancement != 0)
                    bonuses.Add($"{result.Enhancement} (enhancement)");
                
                description += string.Join(" + ", bonuses);
                description += $" = {result.FinalHealing}";
            }

            return description;
        }

        private int GetHealingTypeBonus(string healingType)
        {
            return healingType.ToLowerInvariant() switch
            {
                "magical" => 2,
                "divine" => 3,
                "potion" => 1,
                "natural" => 0,
                _ => 0
            };
        }

        private int ParseDiceExpression(string expression)
        {
            // Reuse the same logic as DamageCommand - in a real plugin, extract to shared utility
            try
            {
                var parts = expression.ToLowerInvariant().Replace(" ", "");
                var total = 0;
                var modifier = 0;

                if (parts.Contains('+'))
                {
                    var splitPlus = parts.Split('+');
                    parts = splitPlus[0];
                    if (splitPlus.Length > 1 && int.TryParse(splitPlus[1], out var plusMod))
                        modifier = plusMod;
                }
                else if (parts.Contains('-'))
                {
                    var splitMinus = parts.Split('-');
                    parts = splitMinus[0];
                    if (splitMinus.Length > 1 && int.TryParse(splitMinus[1], out var minusMod))
                        modifier = -minusMod;
                }

                if (parts.Contains('d'))
                {
                    var diceParts = parts.Split('d');
                    var count = string.IsNullOrEmpty(diceParts[0]) ? 1 : int.Parse(diceParts[0]);
                    var sides = int.Parse(diceParts[1]);

                    for (int i = 0; i < count; i++)
                    {
                        total += _random.Next(1, sides + 1);
                    }
                }
                else
                {
                    total = int.Parse(parts);
                }

                return Math.Max(0, total + modifier);
            }
            catch
            {
                return 1;
            }
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        private int ExtractIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var obj))
                return defaultValue;

            if (int.TryParse(obj.ToString(), out var value))
                return value;

            return defaultValue;
        }
    }

    #region Result Classes

    /// <summary>
    /// Result of a skill check command
    /// </summary>
    public class SkillCheckResult
    {
        public int Roll { get; set; }
        public int OriginalSkill { get; set; }
        public int AdjustedTarget { get; set; }
        public string Difficulty { get; set; } = "normal";
        public int Modifier { get; set; }
        public int DifficultyModifier { get; set; }
        public bool Success { get; set; }
        public string SuccessLevel { get; set; } = "Failure";
    }

    /// <summary>
    /// Result of an initiative roll
    /// </summary>
    public class InitiativeResult
    {
        public int BaseRoll { get; set; }
        public int Modifier { get; set; }
        public bool Advantage { get; set; }
        public int FinalInitiative { get; set; }
    }

    /// <summary>
    /// Result of a damage calculation
    /// </summary>
    public class DamageResult
    {
        public string DiceExpression { get; set; } = "";
        public int BaseDamage { get; set; }
        public string DamageType { get; set; } = "physical";
        public string Resistance { get; set; } = "none";
        public int FinalDamage { get; set; }
    }

    /// <summary>
    /// Result of a healing calculation
    /// </summary>
    public class HealingResult
    {
        public string DiceExpression { get; set; } = "";
        public int BaseHealing { get; set; }
        public string HealingType { get; set; } = "magical";
        public int TypeBonus { get; set; }
        public int Enhancement { get; set; }
        public int FinalHealing { get; set; }
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for string formatting
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert string to title case
    /// </summary>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + (input.Length > 1 ? input[1..].ToLower() : "");
    }
}

#endregion