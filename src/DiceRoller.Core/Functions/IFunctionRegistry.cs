using System;
using System.Collections.Generic;
using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Registry for dice functions that can be dynamically registered and called
    /// </summary>
    public interface IFunctionRegistry
    {
        /// <summary>
        /// Gets all registered function names
        /// </summary>
        IEnumerable<string> RegisteredFunctions { get; }

        /// <summary>
        /// Determines if a function is registered
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>True if the function is registered</returns>
        bool IsRegistered(string functionName);

        /// <summary>
        /// Registers a function with the registry
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="function">The function implementation</param>
        void RegisterFunction(string functionName, IDiceFunction function);

        /// <summary>
        /// Unregisters a function from the registry
        /// </summary>
        /// <param name="functionName">The name of the function to unregister</param>
        /// <returns>True if the function was found and unregistered, false if it wasn't registered</returns>
        bool UnregisterFunction(string functionName);

        /// <summary>
        /// Executes a registered function
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="node">The parse tree node containing the function call</param>
        /// <param name="context">The function execution context</param>
        /// <returns>The result of the function</returns>
        DiceResultNode ExecuteFunction(string functionName, ParseTreeNode node, IFunctionContext context);
    }
}