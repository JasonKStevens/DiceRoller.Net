using System;
using System.Collections.Generic;
using DiceRoller.Core.Abstractions;
using DiceRoller.DragonQuest;
using DiceRoller.GameSystems.Base;
using DiceRoller.Heroes;
using DiceRoller.Core.Parser;

namespace DiceRoller.GameSystems.Heroes
{
    /// <summary>
    /// Game system plugin for Heroes RPG
    /// </summary>
    public class HeroesGameSystem : GameSystemBase
    {
        private readonly IDiceExpressionEvaluator _evaluator;
        private readonly SpeedTable _speeds;

        /// <summary>
        /// Gets the name of the game system
        /// </summary>
        public override string Name => "Heroes";

        /// <summary>
        /// Gets the supported command types for this game system
        /// </summary>
        public override IEnumerable<string> SupportedCommands => new[]
        {
            "speed"
        };

        /// <summary>
        /// Initializes a new instance of the HeroesGameSystem
        /// </summary>
        /// <param name="evaluator">The dice expression evaluator</param>
        /// <param name="heroesLookupTables">The Heroes lookup tables</param>
        public HeroesGameSystem(IDiceExpressionEvaluator evaluator, HerosLookupTables heroesLookupTables)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            if (heroesLookupTables == null) throw new ArgumentNullException(nameof(heroesLookupTables));

            _speeds = heroesLookupTables.Speeds;
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
                case "speed":
                    return GenerateTypedResult(_speeds, parameters);

                default:
                    return CreateErrorResult($"Unknown command type: {commandType}");
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
    }
}