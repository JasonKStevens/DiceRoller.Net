using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;

namespace DiceRoller.Plugins.Template.Tables
{
    /// <summary>
    /// Example critical hits table demonstrating range-based lookup tables
    /// 
    /// This table shows:
    /// - Range-based data organization (01-05, 06-10, etc.)
    /// - Table lookup helpers usage
    /// - Error handling for invalid inputs
    /// - Formatted result descriptions
    /// - Logging integration
    /// </summary>
    public class CriticalHitsTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _criticalHitsTable;

        public CriticalHitsTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _criticalHitsTable = InitializeCriticalHitsTable();
        }

        /// <summary>
        /// Look up a critical hit effect based on a dice roll
        /// </summary>
        /// <param name="key">Dice roll result (1-20)</param>
        /// <param name="context">Game system context</param>
        /// <returns>Critical hit effect description</returns>
        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Invalid roll value for critical hits table - must be a number");
                }

                if (roll < 1 || roll > 20)
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Critical hit roll must be between 1 and 20");
                }

                // Use the helper to look up the result
                var result = TableLookupHelper.LookupByRange(_criticalHitsTable, roll, _logger);
                
                // Add some flavor text
                if (result.Success && result.Value != null)
                {
                    var enhancedDescription = $"⚔️ **Critical Hit!** (rolled {roll})\n{result.Description}";
                    await Task.CompletedTask;
                    return new GameSystemTableResult(true, result.Value, enhancedDescription);
                }

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up critical hit for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Initialize the critical hits table with range-based data
        /// </summary>
        private List<TableRangeEntry> InitializeCriticalHitsTable()
        {
            // Define the critical hits table data using range notation
            var tableData = new Dictionary<string, string>
            {
                ["01-05"] = "**Stunning Blow** - Target is stunned for 1 round and takes maximum damage",
                ["06-10"] = "**Precise Strike** - Double damage and ignore armor",
                ["11-12"] = "**Wounding Hit** - Normal damage plus bleeding (1d4 per round for 3 rounds)",
                ["13-14"] = "**Disarming Strike** - Normal damage and target drops held item",
                ["15-16"] = "**Knockdown** - Normal damage and target is knocked prone", 
                ["17-18"] = "**Vital Strike** - 1.5x damage and target has disadvantage on next attack",
                ["19-19"] = "**Devastating Blow** - Triple damage but weapon takes damage",
                ["20-20"] = "**Perfect Strike** - Maximum damage, ignore all defenses, and weapon glows for 1 minute"
            };

            // Convert to range entries using the helper
            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Example fumbles table for critical failures
    /// 
    /// This demonstrates:
    /// - Negative consequences for bad rolls
    /// - Varied severity of effects
    /// - Descriptive flavor text
    /// - Integration with combat mechanics
    /// </summary>
    public class FumblesTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _fumblesTable;

        public FumblesTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fumblesTable = InitializeFumblesTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Invalid roll value for fumbles table - must be a number");
                }

                if (roll < 1 || roll > 20)
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Fumble roll must be between 1 and 20");
                }

                var result = TableLookupHelper.LookupByRange(_fumblesTable, roll, _logger);
                
                if (result.Success && result.Value != null)
                {
                    var enhancedDescription = $"💥 **Critical Failure!** (rolled {roll})\n{result.Description}";
                    await Task.CompletedTask;
                    return new GameSystemTableResult(true, result.Value, enhancedDescription);
                }

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up fumble for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializeFumblesTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-02"] = "**Equipment Destroyed** - Weapon/tool breaks completely and is unusable",
                ["03-04"] = "**Major Mishap** - Hit nearest ally for full damage if in range",
                ["05-06"] = "**Spectacular Fall** - Fall prone and take 1d4 damage, lose next action",
                ["07-08"] = "**Weapon Stuck** - Weapon lodged in surface, takes full action to retrieve",
                ["09-10"] = "**Off Balance** - Opponent gets immediate free attack with advantage",
                ["11-12"] = "**Equipment Damage** - Weapon/armor takes damage, -1 to rolls until repaired",
                ["13-14"] = "**Trip and Stumble** - Fall prone but take no damage",
                ["15-16"] = "**Poor Position** - Disadvantage on next attack or defense roll",
                ["17-18"] = "**Distracted** - Lose next action due to embarrassment or confusion",
                ["19-20"] = "**Minor Slip** - No additional effect beyond the failed roll"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Example weather table for environmental effects
    /// 
    /// This shows:
    /// - Environmental/atmospheric generation
    /// - Seasonal variations
    /// - Gameplay impact considerations
    /// - Descriptive atmospheric text
    /// </summary>
    public class WeatherTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _weatherTable;

        public WeatherTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _weatherTable = InitializeWeatherTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Invalid roll value for weather table - must be a number (1-100)");
                }

                if (roll < 1 || roll > 100)
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Weather roll must be between 1 and 100");
                }

                var result = TableLookupHelper.LookupByRange(_weatherTable, roll, _logger);
                
                if (result.Success && result.Value != null)
                {
                    var enhancedDescription = $"🌤️ **Weather Conditions** (rolled {roll})\n{result.Description}";
                    await Task.CompletedTask;
                    return new GameSystemTableResult(true, result.Value, enhancedDescription);
                }

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up weather for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializeWeatherTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-10"] = "**Severe Storm** ⛈️ - Heavy rain/snow, strong winds, poor visibility (-4 to ranged attacks, movement halved)",
                ["11-20"] = "**Heavy Precipitation** 🌧️ - Steady rain/snow, reduced visibility (-2 to perception, fire attacks weakened)",
                ["21-30"] = "**Light Precipitation** 🌦️ - Drizzle or light snow, minimal impact (-1 to tracking)",
                ["31-40"] = "**Overcast** ☁️ - Cloudy skies, no precipitation, normal conditions",
                ["41-60"] = "**Clear** ☀️ - Pleasant weather, optimal conditions (+1 to morale)",
                ["61-70"] = "**Partly Cloudy** ⛅ - Mixed sun and clouds, comfortable conditions",
                ["71-80"] = "**Windy** 💨 - Strong winds (-2 to ranged attacks, +1 to sailing)",
                ["81-85"] = "**Foggy** 🌫️ - Dense fog, severely limited visibility (-4 to perception beyond 30 feet)",
                ["86-90"] = "**Hot/Cold** 🌡️ - Extreme temperature, potential exhaustion after prolonged exposure",
                ["91-95"] = "**Unusual** 🌈 - Strange atmospheric phenomena (aurora, double rainbow, etc.)",
                ["96-00"] = "**Magical Weather** ✨ - Supernatural weather effects (roll on magical weather subtable)"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Example random encounters table for wilderness travel
    /// 
    /// This demonstrates:
    /// - Adventure content generation
    /// - Scalable encounter difficulty
    /// - Multiple encounter types
    /// - GM guidance and suggestions
    /// </summary>
    public class EncountersTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _encountersTable;

        public EncountersTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _encountersTable = InitializeEncountersTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Invalid roll value for encounters table - must be a number (1-20)");
                }

                if (roll < 1 || roll > 20)
                {
                    return new GameSystemTableResult(false, null, string.Empty, 
                        "Encounter roll must be between 1 and 20");
                }

                var result = TableLookupHelper.LookupByRange(_encountersTable, roll, _logger);
                
                if (result.Success && result.Value != null)
                {
                    var enhancedDescription = $"🗺️ **Random Encounter** (rolled {roll})\n{result.Description}";
                    await Task.CompletedTask;
                    return new GameSystemTableResult(true, result.Value, enhancedDescription);
                }

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up encounter for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializeEncountersTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-01"] = "**Hostile Monsters** 👹 - 2d4 aggressive creatures appropriate to terrain (goblins, wolves, bandits)",
                ["02-03"] = "**Predatory Beast** 🐺 - Single dangerous animal hunting in the area (bear, big cat, dire wolf)",
                ["04-05"] = "**Bandits/Brigands** ⚔️ - 1d6+2 armed humanoids demanding tolls or goods",
                ["06-07"] = "**Territorial Creature** 🦅 - Large creature defending its territory (giant eagle, owlbear, troll)",
                ["08-09"] = "**Friendly Travelers** 🚶 - 1d4+1 merchants, pilgrims, or adventurers willing to trade/talk",
                ["10-11"] = "**Neutral Wildlife** 🦌 - Harmless animals that may provide food or warning of danger",
                ["12-13"] = "**Patrol/Guards** 🛡️ - 2d4 soldiers or militia on routine patrol, may be helpful or suspicious",
                ["14-15"] = "**Lost Traveler** 🗺️ - Single NPC who is lost, injured, or needs assistance",
                ["16-16"] = "**Interesting Location** 🏛️ - Ruins, cave, shrine, or other landmark worth investigating",
                ["17-17"] = "**Natural Hazard** ⚠️ - Dangerous terrain, unstable ground, or environmental threat",
                ["18-18"] = "**Signs of Passage** 👣 - Tracks, camps, or evidence of others having passed this way",
                ["19-19"] = "**Unusual Discovery** 💎 - Strange object, magical phenomenon, or mysterious occurrence",
                ["20-20"] = "**No Encounter** ✅ - Peaceful travel with no significant events"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Example of a value-based lookup table (not range-based)
    /// 
    /// This shows an alternative table structure for:
    /// - Exact key matching instead of ranges
    /// - Named lookups (skill names, spell names, etc.)
    /// - Reference data organization
    /// - Multiple data types in results
    /// </summary>
    public class SkillDifficultyTable
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, SkillDifficultyInfo> _difficultyTable;

        public SkillDifficultyTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _difficultyTable = InitializeDifficultyTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                var difficultyName = key.ToString()?.ToLowerInvariant() ?? "";

                if (!_difficultyTable.TryGetValue(difficultyName, out var difficultyInfo))
                {
                    var availableKeys = string.Join(", ", _difficultyTable.Keys);
                    return new GameSystemTableResult(false, null, string.Empty, 
                        $"Unknown difficulty level: {key}. Available levels: {availableKeys}");
                }

                var description = $"🎯 **{difficultyInfo.Name}** (Modifier: {difficultyInfo.Modifier:+0;-#})\n" +
                                $"📝 {difficultyInfo.Description}\n" +
                                $"💡 *Example:* {difficultyInfo.Example}";

                await Task.CompletedTask;
                return new GameSystemTableResult(true, difficultyInfo, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up skill difficulty for: {Key}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private Dictionary<string, SkillDifficultyInfo> InitializeDifficultyTable()
        {
            return new Dictionary<string, SkillDifficultyInfo>
            {
                ["trivial"] = new("Trivial", +30, "Extremely easy tasks that rarely fail", "Tying your shoes, reading common text"),
                ["easy"] = new("Easy", +20, "Simple tasks with good chance of success", "Climbing a ladder, picking a simple lock"),
                ["normal"] = new("Normal", 0, "Standard difficulty for trained individuals", "Swimming across a river, haggling for better prices"),
                ["hard"] = new("Hard", -20, "Challenging tasks requiring skill and luck", "Climbing a cliff face, detecting a hidden door"),
                ["extreme"] = new("Extreme", -30, "Nearly impossible tasks for most people", "Jumping across a wide chasm, convincing a hostile enemy"),
                ["impossible"] = new("Impossible", -50, "Legendary feats that defy normal expectations", "Lifting a building, talking to the dead without magic")
            };
        }
    }

    /// <summary>
    /// Data structure for skill difficulty information
    /// Shows how to create structured data for table results
    /// </summary>
    public record SkillDifficultyInfo(
        string Name,
        int Modifier,
        string Description,
        string Example);
}