using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Abstractions;
using DiceRoller.DragonQuest;
using DiceRoller.GameSystems.Base;
using DiceRoller.Core.Parser;

namespace DiceRoller.GameSystems.DragonQuest
{
    /// <summary>
    /// Game system plugin for DragonQuest RPG
    /// </summary>
    public class DragonQuestGameSystem : GameSystemBase
    {
        private readonly IDiceExpressionEvaluator _evaluator;
        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;
        private readonly Dictionary<string, LookupTable> _locations;

        /// <summary>
        /// Gets the name of the game system
        /// </summary>
        public override string Name => "DragonQuest";

        /// <summary>
        /// Gets the supported command types for this game system
        /// </summary>
        public override IEnumerable<string> SupportedCommands => new[]
        {
            "backfire",
            "specgrev",
            "injury", 
            "fear",
            "hitlocation",
            "hitloc"
        };

        /// <summary>
        /// Initializes a new instance of the DragonQuestGameSystem
        /// </summary>
        /// <param name="evaluator">The dice expression evaluator</param>
        /// <param name="dqTables">The DragonQuest lookup tables</param>
        public DragonQuestGameSystem(IDiceExpressionEvaluator evaluator, DQLookupTables dqTables)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            if (dqTables == null) throw new ArgumentNullException(nameof(dqTables));

            _injuries = dqTables.Injuries;
            _backfires = dqTables.Backfires;
            _fears = dqTables.Fear;

            _locations = new Dictionary<string, LookupTable>(StringComparer.OrdinalIgnoreCase);
            AddLookupTable(new HumanoidHighLocationsTable(), "humanoid_high", "hh");
            AddLookupTable(new HumanoidArmsLocationsTable(), "humanoid_arms", "ha");
            AddLookupTable(new HumanoidMidLocationsTable(), "humanoid_mid", "hm");
            AddLookupTable(new HumanoidLegsLocationsTable(), "humanoid_legs", "hl");
            AddLookupTable(new QuadrupedHighLocationsTable(), "quadruped_high", "qh");
            AddLookupTable(new QuadrupedMidLocationsTable(), "quadruped_mid", "qm");
            AddLookupTable(new QuadrupedLowLocationsTable(), "quadruped_low", "ql");
            AddLookupTable(new AvianHighLocationsTable(), "avian_high", "ah");
            AddLookupTable(new AvianMidLocationsTable(), "avian_mid", "am");
            AddLookupTable(new AvianLowLocationsTable(), "avian_low", "al");
            AddLookupTable(new SerpentineHighLocationsTable(), "serpent_high", "sh");
            AddLookupTable(new SerpentineMidLocationsTable(), "serpent_mid", "sm");
            AddLookupTable(new SerpentineLowLocationsTable(), "serpent_low", "sl");
        }

        /// <summary>
        /// Processes a game system specific command
        /// </summary>
        /// <param name="commandType">The type of command</param>
        /// <param name="parameters">The command parameters</param>
        /// <param name="userId">The user ID for context</param>
        /// <returns>The result of the command</returns>
        public override TypedResult ProcessCommand(string commandType, string[] parameters, string userId)
        {
            if (string.IsNullOrWhiteSpace(commandType))
                return CreateErrorResult("Command type cannot be empty");

            switch (commandType.ToLower())
            {
                case "backfire":
                    return GenerateTypedResult(_backfires, parameters);

                case "specgrev":
                case "injury":
                    return GenerateTypedResult(_injuries, parameters);

                case "fear":
                    return GenerateTypedResult(_fears, parameters);

                case "hitlocation":
                case "hitloc":
                    return GenerateTypedLocationResult(parameters);

                default:
                    return CreateErrorResult($"Unknown command type: {commandType}");
            }
        }

        private void AddLookupTable(LookupTable table, params string[] names)
        {
            foreach (var name in names)
            {
                _locations.Add(name, table);
            }
        }

        private TypedResult GenerateTypedResult(LookupTable table, string[] parameters)
        {
            try
            {
                string instructions = parameters?.Length > 0 ? string.Join(' ', parameters) : table.GetRoll();

                if (string.IsNullOrWhiteSpace(instructions))
                    instructions = table.GetRoll();

                // Create a simple parse tree for the dice expression
                var diceTree = new DiceRoller.Core.Parser.DiceExpressionParser().Parse(instructions);
                int roll = (int)_evaluator.Evaluate(diceTree).Value;

                return CreateLookupResult(roll, table.LookupResult(roll));
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"Error generating result: {ex.Message}");
            }
        }

        private TypedResult GenerateTypedLocationResult(string[] parameters)
        {
            try
            {
                var bodyType = "humanoid_mid";
                var rollInstruction = "";

                if (parameters?.Length > 0)
                {
                    foreach (var param in parameters)
                    {
                        if (int.TryParse(param, out var _))
                        {
                            rollInstruction = param;
                        }
                        else if (_locations.ContainsKey(param))
                        {
                            bodyType = param;
                        }
                    }
                }

                if (!_locations.ContainsKey(bodyType))
                    return CreateErrorResult($"Unknown body type: '{bodyType}'");

                var table = _locations[bodyType];

                if (string.IsNullOrWhiteSpace(rollInstruction))
                    rollInstruction = table.GetRoll();

                // Create a simple parse tree for the dice expression
                var diceTree = new DiceRoller.Core.Parser.DiceExpressionParser().Parse(rollInstruction);
                int roll = (int)_evaluator.Evaluate(diceTree).Value;

                return CreateLookupResult(roll, table.LookupResult(roll));
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"Error generating location result: {ex.Message}");
            }
        }
    }
}