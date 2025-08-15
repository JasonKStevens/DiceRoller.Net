using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Parser;

namespace DiceRoller.GameSystems.Base
{
    /// <summary>
    /// Base class for game system plugins
    /// </summary>
    public abstract class GameSystemBase : IGameSystem
    {
        /// <summary>
        /// Gets the name of the game system
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the supported command types for this game system
        /// </summary>
        public abstract IEnumerable<string> SupportedCommands { get; }

        /// <summary>
        /// Determines if this game system can handle the specified command
        /// </summary>
        /// <param name="commandType">The command type to check</param>
        /// <returns>True if the command is supported</returns>
        public virtual bool SupportsCommand(string commandType)
        {
            if (string.IsNullOrWhiteSpace(commandType))
                return false;

            return SupportedCommands.Any(cmd => 
                string.Equals(cmd, commandType, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes a game system specific command
        /// </summary>
        /// <param name="commandType">The type of command</param>
        /// <param name="parameters">The command parameters</param>
        /// <param name="userId">The user ID for context</param>
        /// <returns>The result of the command</returns>
        public abstract TypedResult ProcessCommand(string commandType, string[] parameters, string userId);

        /// <summary>
        /// Helper method to create a simple result
        /// </summary>
        /// <param name="text">The text to return</param>
        /// <returns>A simple typed result</returns>
        protected TypedResult CreateSimpleResult(string text)
        {
            return TypedResult.NewSimpleResult(text);
        }

        /// <summary>
        /// Helper method to create a lookup result
        /// </summary>
        /// <param name="roll">The dice roll value</param>
        /// <param name="lookupText">The lookup result text</param>
        /// <returns>A typed result with lookup formatting</returns>
        protected TypedResult CreateLookupResult(int roll, string lookupText)
        {
            var typedResult = new TypedResult { NodeType = NodeType.Lookup, Text = roll.ToString() };
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, roll.ToString()));
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.None, lookupText));
            return typedResult;
        }

        /// <summary>
        /// Helper method to create an error result
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A typed result with error formatting</returns>
        protected TypedResult CreateErrorResult(string errorMessage)
        {
            return TypedResult.NewSimpleResult($"Error: {errorMessage}");
        }
    }
}