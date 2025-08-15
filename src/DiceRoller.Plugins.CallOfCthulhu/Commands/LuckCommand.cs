using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

namespace DiceRoller.Plugins.CallOfCthulhu.Commands
{
    /// <summary>
    /// Handles luck checks for Call of Cthulhu
    /// </summary>
    public class LuckCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public LuckCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Execute a luck check command
        /// </summary>
        [Command("luck", Description = "Perform a luck check")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                // Extract parameters
                if (!parameters.TryGetValue("current_luck", out var currentLuckObj) ||
                    !int.TryParse(currentLuckObj.ToString(), out var currentLuck))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, "Current luck value is required");
                }

                var difficulty = ExtractStringParameter(parameters, "difficulty", "normal");
                var spendLuck = ExtractBoolParameter(parameters, "spend_luck", false);
                var luckSpent = ExtractIntParameter(parameters, "luck_spent", 0);

                // Perform luck check
                var luckRoll = _random.Next(1, 101); // d100
                var success = DetermineLuckSuccess(luckRoll, currentLuck, difficulty);

                var result = new LuckCheckResult
                {
                    Roll = luckRoll,
                    CurrentLuck = currentLuck,
                    Success = success,
                    Difficulty = difficulty,
                    LuckSpent = spendLuck ? luckSpent : 0,
                    NewLuck = spendLuck ? Math.Max(0, currentLuck - luckSpent) : currentLuck
                };

                var description = FormatLuckResult(result);

                _logger.LogDebug("Luck check executed: Roll={Roll}, Success={Success}, Spent={LuckSpent}", 
                    result.Roll, result.Success, result.LuckSpent);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing luck command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
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

        private int ExtractIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            if (int.TryParse(value.ToString(), out var intValue))
                return intValue;

            return defaultValue;
        }

        private bool DetermineLuckSuccess(int roll, int currentLuck, string difficulty)
        {
            var target = difficulty.ToLowerInvariant() switch
            {
                "hard" => currentLuck / 2,
                "extreme" => currentLuck / 5,
                _ => currentLuck
            };

            return roll <= target;
        }

        private string FormatLuckResult(LuckCheckResult result)
        {
            var successText = result.Success ? "**SUCCESS**" : "**FAILURE**";
            var difficultyText = result.Difficulty != "normal" ? $" ({result.Difficulty})" : "";
            
            var description = $"**Luck Check{difficultyText}:** {successText}\n" +
                             $"Rolled: {result.Roll} vs {result.CurrentLuck} luck";

            if (result.LuckSpent > 0)
            {
                description += $"\n**Luck Spent:** {result.LuckSpent} points";
                description += $"\n**New Luck:** {result.NewLuck}";

                if (result.NewLuck <= 0)
                {
                    description += "\n⚠️ **WARNING:** Investigator has no luck remaining!";
                }
            }

            // Add luck spending advice
            if (!result.Success)
            {
                var luckNeeded = result.Roll - GetLuckTarget(result.CurrentLuck, result.Difficulty) + 1;
                if (luckNeeded <= result.CurrentLuck)
                {
                    description += $"\n💡 **Tip:** You could spend {luckNeeded} luck to make this check succeed.";
                }
            }

            return description;
        }

        private int GetLuckTarget(int currentLuck, string difficulty)
        {
            return difficulty.ToLowerInvariant() switch
            {
                "hard" => currentLuck / 2,
                "extreme" => currentLuck / 5,
                _ => currentLuck
            };
        }
    }

    /// <summary>
    /// Result of a luck check
    /// </summary>
    public class LuckCheckResult
    {
        public int Roll { get; set; }
        public int CurrentLuck { get; set; }
        public bool Success { get; set; }
        public string Difficulty { get; set; } = "normal";
        public int LuckSpent { get; set; }
        public int NewLuck { get; set; }
    }
}