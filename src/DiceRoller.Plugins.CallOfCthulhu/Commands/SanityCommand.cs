using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

namespace DiceRoller.Plugins.CallOfCthulhu.Commands
{
    /// <summary>
    /// Handles sanity checks and sanity loss calculations for Call of Cthulhu
    /// </summary>
    public class SanityCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public SanityCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Execute a sanity check command
        /// </summary>
        [Command("sanity", Description = "Perform a sanity check with optional loss calculation")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            [CommandParameter(Description = "Current sanity value")] Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                // Extract parameters
                if (!parameters.TryGetValue("current_sanity", out var currentSanityObj) ||
                    !int.TryParse(currentSanityObj.ToString(), out var currentSanity))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, "Current sanity value is required");
                }

                var sanityLoss = ExtractSanityLoss(parameters);
                var rollForLoss = ExtractBoolParameter(parameters, "roll_for_loss", false);
                var difficulty = ExtractStringParameter(parameters, "difficulty", "normal");

                // Perform sanity check
                var sanityRoll = _random.Next(1, 101); // d100
                var success = DetermineSanitySuccess(sanityRoll, currentSanity, difficulty);

                var result = new SanityCheckResult
                {
                    Roll = sanityRoll,
                    CurrentSanity = currentSanity,
                    Success = success,
                    SanityLoss = 0,
                    NewSanity = currentSanity,
                    Difficulty = difficulty
                };

                // Apply sanity loss if specified
                if (sanityLoss > 0 || rollForLoss)
                {
                    var actualLoss = rollForLoss ? RollSanityLoss(sanityLoss) : sanityLoss;
                    
                    // In Call of Cthulhu, you lose less sanity on successful checks
                    if (success && !rollForLoss)
                    {
                        actualLoss = Math.Max(1, actualLoss / 2); // Half loss on success, minimum 1
                    }

                    result.SanityLoss = actualLoss;
                    result.NewSanity = Math.Max(0, currentSanity - actualLoss);
                }

                var description = FormatSanityResult(result);

                _logger.LogDebug("Sanity check executed: Roll={Roll}, Success={Success}, Loss={Loss}", 
                    result.Roll, result.Success, result.SanityLoss);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing sanity command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private int ExtractSanityLoss(Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("sanity_loss", out var sanityLossObj))
                return 0;

            var sanityLossStr = sanityLossObj.ToString();
            if (string.IsNullOrEmpty(sanityLossStr))
                return 0;

            // Try to parse as integer first
            if (int.TryParse(sanityLossStr, out var intValue))
                return intValue;

            // If it contains 'd', treat as dice expression (simplified)
            if (sanityLossStr.Contains('d', StringComparison.OrdinalIgnoreCase))
            {
                return ParseDiceExpression(sanityLossStr);
            }

            return 0;
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

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        private bool DetermineSanitySuccess(int roll, int currentSanity, string difficulty)
        {
            var target = difficulty.ToLowerInvariant() switch
            {
                "hard" => currentSanity / 2,
                "extreme" => currentSanity / 5,
                _ => currentSanity
            };

            return roll <= target;
        }

        private int RollSanityLoss(int baseLoss)
        {
            // Simple implementation - in a real plugin, you'd use proper dice rolling
            return Math.Max(1, baseLoss + _random.Next(-2, 3)); // ±2 variation
        }

        private int ParseDiceExpression(string diceExpression)
        {
            // Simplified dice parsing - in a real implementation, use proper dice parser
            try
            {
                var parts = diceExpression.ToLowerInvariant().Split('d');
                if (parts.Length == 2)
                {
                    var count = string.IsNullOrEmpty(parts[0]) ? 1 : int.Parse(parts[0]);
                    var sides = int.Parse(parts[1]);

                    var total = 0;
                    for (int i = 0; i < count; i++)
                    {
                        total += _random.Next(1, sides + 1);
                    }
                    return total;
                }
            }
            catch
            {
                // Fallback to 1 if parsing fails
            }

            return 1;
        }

        private string FormatSanityResult(SanityCheckResult result)
        {
            var successText = result.Success ? "**SUCCESS**" : "**FAILURE**";
            var difficultyText = result.Difficulty != "normal" ? $" ({result.Difficulty})" : "";
            
            var description = $"**Sanity Check{difficultyText}:** {successText}\n" +
                             $"Rolled: {result.Roll} vs {result.CurrentSanity} sanity";

            if (result.SanityLoss > 0)
            {
                description += $"\n**Sanity Loss:** {result.SanityLoss} points";
                description += $"\n**New Sanity:** {result.NewSanity}";

                if (result.NewSanity <= 0)
                {
                    description += "\n⚠️ **WARNING:** Investigator has reached 0 sanity and may become an NPC!";
                }
                else if (result.SanityLoss >= 5)
                {
                    description += "\n⚠️ **Severe Sanity Loss:** Consider rolling for temporary madness.";
                }
            }

            return description;
        }
    }

    /// <summary>
    /// Result of a sanity check
    /// </summary>
    public class SanityCheckResult
    {
        public int Roll { get; set; }
        public int CurrentSanity { get; set; }
        public bool Success { get; set; }
        public int SanityLoss { get; set; }
        public int NewSanity { get; set; }
        public string Difficulty { get; set; } = "normal";
    }
}