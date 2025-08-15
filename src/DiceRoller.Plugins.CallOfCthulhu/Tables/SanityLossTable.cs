using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;

namespace DiceRoller.Plugins.CallOfCthulhu.Tables
{
    /// <summary>
    /// Sanity loss table for various threats and encounters
    /// </summary>
    public class SanityLossTable
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _sanityLossData;

        public SanityLossTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sanityLossData = InitializeSanityLossData();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                var keyStr = key.ToString()?.ToLowerInvariant() ?? string.Empty;
                
                if (_sanityLossData.TryGetValue(keyStr, out var sanityLoss))
                {
                    var description = $"Sanity Loss: {sanityLoss}";
                    await Task.CompletedTask;
                    return new GameSystemTableResult(true, sanityLoss, description);
                }

                return new GameSystemTableResult(false, null, string.Empty, $"No sanity loss data found for: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up sanity loss for key: {Key}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private Dictionary<string, string> InitializeSanityLossData()
        {
            return new Dictionary<string, string>
            {
                // Common threats
                ["corpse"] = "0/1d2",
                ["mutilated_corpse"] = "1/1d4+1",
                ["violence"] = "0/1d2",
                ["murder"] = "0/1d4+1",
                ["torture"] = "1d2/1d4+1",
                
                // Supernatural
                ["ghost"] = "0/1d6",
                ["zombie"] = "1/1d8",
                ["vampire"] = "0/1d6",
                ["werewolf"] = "1d2/1d8",
                ["mummy"] = "1d4/1d8",
                
                // Mythos creatures (minor)
                ["deep_one"] = "0/1d6",
                ["ghoul"] = "0/1d6",
                ["byakhee"] = "1/1d8",
                ["hunting_horror"] = "1d6/1d10",
                
                // Mythos creatures (major)
                ["shoggoth"] = "1d10/1d20",
                ["star_spawn"] = "1d6/1d20",
                ["great_old_one"] = "1d10/1d100",
                
                // Cosmic horror
                ["alien_technology"] = "0/1d6",
                ["non_euclidean_geometry"] = "1d2/1d8",
                ["time_distortion"] = "1d4/1d10",
                ["reality_break"] = "1d6/1d20",
                
                // Tomes and knowledge
                ["blasphemous_tome"] = "1d4/1d8",
                ["forbidden_ritual"] = "1d6/1d10",
                ["cosmic_truth"] = "1d8/1d20",
                ["ultimate_revelation"] = "1d10/1d100"
            };
        }
    }
}