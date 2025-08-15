using System.Collections.Generic;
using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Abstractions
{
    /// <summary>
    /// Interface for routing game system commands to appropriate handlers
    /// </summary>
    public interface IGameSystemRouter
    {
        /// <summary>
        /// Gets all registered game systems
        /// </summary>
        IEnumerable<IGameSystem> RegisteredSystems { get; }

        /// <summary>
        /// Determines if a command can be handled by any registered game system
        /// </summary>
        /// <param name="commandType">The command type to check</param>
        /// <returns>True if the command can be handled</returns>
        bool CanHandle(string commandType);

        /// <summary>
        /// Routes a command to the appropriate game system and converts the result to DiceResultNode
        /// </summary>
        /// <param name="commandType">The type of command</param>
        /// <param name="parameters">The command parameters (optional)</param>
        /// <param name="userId">The user ID for context (optional)</param>
        /// <returns>A DiceResultNode containing the converted result, or null if no system can handle the command</returns>
        DiceResultNode RouteCommand(string commandType, string[] parameters = null, string userId = null);

        /// <summary>
        /// Registers a game system with the router
        /// </summary>
        /// <param name="gameSystem">The game system to register</param>
        void RegisterSystem(IGameSystem gameSystem);

        /// <summary>
        /// Unregisters a game system from the router
        /// </summary>
        /// <param name="gameSystem">The game system to unregister</param>
        void UnregisterSystem(IGameSystem gameSystem);
    }
}