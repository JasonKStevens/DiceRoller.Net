using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Routing
{
    /// <summary>
    /// Routes game system commands to appropriate handlers and converts results
    /// </summary>
    public class GameSystemRouter : IGameSystemRouter
    {
        private readonly List<IGameSystem> _registeredSystems = new List<IGameSystem>();

        /// <summary>
        /// Gets all registered game systems
        /// </summary>
        public IEnumerable<IGameSystem> RegisteredSystems => _registeredSystems.AsReadOnly();

        /// <summary>
        /// Determines if a command can be handled by any registered game system
        /// </summary>
        /// <param name="commandType">The command type to check</param>
        /// <returns>True if the command can be handled</returns>
        public bool CanHandle(string commandType)
        {
            if (string.IsNullOrWhiteSpace(commandType))
                return false;

            return _registeredSystems.Any(system => system.SupportsCommand(commandType));
        }

        /// <summary>
        /// Routes a command to the appropriate game system and converts the result to DiceResultNode
        /// </summary>
        /// <param name="commandType">The type of command</param>
        /// <param name="parameters">The command parameters (optional)</param>
        /// <param name="userId">The user ID for context (optional)</param>
        /// <returns>A DiceResultNode containing the converted result, or null if no system can handle the command</returns>
        public DiceResultNode RouteCommand(string commandType, string[] parameters = null, string userId = null)
        {
            if (string.IsNullOrWhiteSpace(commandType))
                return null;

            // Find the first system that can handle this command
            var gameSystem = _registeredSystems.FirstOrDefault(system => system.SupportsCommand(commandType));
            
            if (gameSystem == null)
                return null;

            try
            {
                // Process the command through the game system
                var typedResult = gameSystem.ProcessCommand(commandType, parameters ?? new string[0], userId ?? string.Empty);
                
                // Convert TypedResult to DiceResultNode
                return ConvertTypedResultToDiceResultNode(typedResult);
            }
            catch (Exception ex)
            {
                // Return an error result if processing fails
                var errorResult = TypedResult.NewSimpleResult($"Error processing {commandType}: {ex.Message}");
                return ConvertTypedResultToDiceResultNode(errorResult);
            }
        }

        /// <summary>
        /// Registers a game system with the router
        /// </summary>
        /// <param name="gameSystem">The game system to register</param>
        public void RegisterSystem(IGameSystem gameSystem)
        {
            if (gameSystem == null)
                throw new ArgumentNullException(nameof(gameSystem));

            if (!_registeredSystems.Contains(gameSystem))
            {
                _registeredSystems.Add(gameSystem);
            }
        }

        /// <summary>
        /// Unregisters a game system from the router
        /// </summary>
        /// <param name="gameSystem">The game system to unregister</param>
        public void UnregisterSystem(IGameSystem gameSystem)
        {
            if (gameSystem != null)
            {
                _registeredSystems.Remove(gameSystem);
            }
        }

        /// <summary>
        /// Converts a TypedResult to a DiceResultNode
        /// </summary>
        /// <param name="typedResult">The TypedResult to convert</param>
        /// <returns>A DiceResultNode containing the converted result</returns>
        private DiceResultNode ConvertTypedResultToDiceResultNode(TypedResult typedResult)
        {
            if (typedResult == null)
                return new DiceResultNode(0, "No result", TypedResult.NewSimpleResult("No result"));

            // Extract the dice roll value if it's a lookup result
            float value = 0;
            string breakdown = typedResult.Text ?? "Unknown";

            // For lookup results, try to extract the dice roll from SubText
            if (typedResult.NodeType == NodeType.Lookup && typedResult.SubText?.Count > 0)
            {
                var diceRollResult = typedResult.SubText.FirstOrDefault(st => st.NodeType == NodeType.DiceRoll);
                if (diceRollResult != null && float.TryParse(diceRollResult.Text, out var rollValue))
                {
                    value = rollValue;
                }
                
                // Build breakdown from lookup result
                var lookupText = typedResult.SubText.FirstOrDefault(st => st.NodeType == NodeType.None)?.Text;
                if (!string.IsNullOrEmpty(lookupText))
                {
                    breakdown = lookupText;
                }
            }
            else
            {
                // For simple results, try to parse the text as a number
                if (float.TryParse(typedResult.Text, out var parsedValue))
                {
                    value = parsedValue;
                }
            }

            return new DiceResultNode(value, breakdown, typedResult);
        }
    }
}