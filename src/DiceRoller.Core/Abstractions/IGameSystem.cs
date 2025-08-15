using System.Collections.Generic;
using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Abstractions
{
    /// <summary>
    /// Interface for game system plugins that provide specialized dice rolling and lookup functionality
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>
        /// Gets the name of the game system
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the supported command types for this game system
        /// </summary>
        IEnumerable<string> SupportedCommands { get; }

        /// <summary>
        /// Determines if this game system can handle the specified command
        /// </summary>
        /// <param name="commandType">The command type to check</param>
        /// <returns>True if the command is supported</returns>
        bool SupportsCommand(string commandType);

        /// <summary>
        /// Processes a game system specific command
        /// </summary>
        /// <param name="commandType">The type of command</param>
        /// <param name="parameters">The command parameters</param>
        /// <param name="userId">The user ID for context</param>
        /// <returns>The result of the command</returns>
        TypedResult ProcessCommand(string commandType, string[] parameters, string userId);
    }
}