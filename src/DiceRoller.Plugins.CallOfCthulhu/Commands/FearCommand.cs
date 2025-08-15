using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

namespace DiceRoller.Plugins.CallOfCthulhu.Commands
{
    /// <summary>
    /// Handles fear checks for Call of Cthulhu
    /// </summary>
    public class FearCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public FearCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Execute a fear check command
        /// </summary>
        [Command("fear", Description = "Perform a fear check")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                // Extract parameters
                if (!parameters.TryGetValue("sanity", out var sanityObj) ||
                    !int.TryParse(sanityObj.ToString(), out var currentSanity))
                {
                    return new GameSystemCommandResult(false, null, string.Empty, "Current sanity value is required");
                }

                var fearSource = ExtractStringParameter(parameters, "source", "unknown horror");
                var difficulty = ExtractStringParameter(parameters, "difficulty", "normal");

                // Perform fear check (same mechanics as sanity check)
                var fearRoll = _random.Next(1, 101); // d100
                var success = DetermineFearSuccess(fearRoll, currentSanity, difficulty);

                var result = new FearCheckResult
                {
                    Roll = fearRoll,
                    CurrentSanity = currentSanity,
                    Success = success,
                    FearSource = fearSource,
                    Difficulty = difficulty
                };

                // Determine fear effects
                if (!success)
                {
                    result.FearEffect = DetermineFearEffect(fearRoll, currentSanity);
                }

                var description = FormatFearResult(result);

                _logger.LogDebug("Fear check executed: Roll={Roll}, Success={Success}, Source={Source}", 
                    result.Roll, result.Success, result.FearSource);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing fear command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        private bool DetermineFearSuccess(int roll, int currentSanity, string difficulty)
        {
            var target = difficulty.ToLowerInvariant() switch
            {
                "hard" => currentSanity / 2,
                "extreme" => currentSanity / 5,
                _ => currentSanity
            };

            return roll <= target;
        }

        private FearEffect DetermineFearEffect(int roll, int sanity)
        {
            // Fear effects based on how badly the check was failed
            var failureMargin = roll - sanity;
            
            return failureMargin switch
            {
                <= 10 => new FearEffect("Startled", "Investigator is momentarily startled but recovers quickly.", 0),
                <= 20 => new FearEffect("Nervous", "Investigator becomes nervous and jumpy for a few minutes.", 1),
                <= 30 => new FearEffect("Frightened", "Investigator is genuinely frightened and may flee.", 2),
                <= 40 => new FearEffect("Terrified", "Investigator is terrified and must make immediate escape.", 3),
                <= 50 => new FearEffect("Panicked", "Investigator panics and may act irrationally.", 4),
                _ => new FearEffect("Overwhelmed", "Investigator is overwhelmed by terror and may faint or become catatonic.", 5)
            };
        }

        private string FormatFearResult(FearCheckResult result)
        {
            var successText = result.Success ? "**SUCCESS**" : "**FAILURE**";
            var difficultyText = result.Difficulty != "normal" ? $" ({result.Difficulty})" : "";
            
            var description = $"**Fear Check{difficultyText}:** {successText}\n" +
                             $"**Source:** {result.FearSource}\n" +
                             $"Rolled: {result.Roll} vs {result.CurrentSanity} sanity";

            if (!result.Success && result.FearEffect != null)
            {
                description += $"\n\n**Fear Effect:** {result.FearEffect.Name}";
                description += $"\n**Description:** {result.FearEffect.Description}";
                
                if (result.FearEffect.Severity >= 3)
                {
                    description += "\n⚠️ **Severe Fear:** Consider additional sanity loss or temporary madness.";
                }
            }

            return description;
        }
    }

    /// <summary>
    /// Fear effect based on failure severity
    /// </summary>
    public class FearEffect
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }

        public FearEffect(string name, string description, int severity)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Severity = severity;
        }
    }

    /// <summary>
    /// Result of a fear check
    /// </summary>
    public class FearCheckResult
    {
        public int Roll { get; set; }
        public int CurrentSanity { get; set; }
        public bool Success { get; set; }
        public string FearSource { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "normal";
        public FearEffect? FearEffect { get; set; }
    }
}