using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Registry for dice functions that can be dynamically registered and called
    /// </summary>
    public class FunctionRegistry : IFunctionRegistry
    {
        private readonly Dictionary<string, IDiceFunction> _functions = new Dictionary<string, IDiceFunction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets all registered function names
        /// </summary>
        public IEnumerable<string> RegisteredFunctions => _functions.Keys;

        /// <summary>
        /// Determines if a function is registered
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>True if the function is registered</returns>
        public bool IsRegistered(string functionName)
        {
            return _functions.ContainsKey(functionName);
        }

        /// <summary>
        /// Registers a function with the registry
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="function">The function implementation</param>
        public void RegisterFunction(string functionName, IDiceFunction function)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException("Function name cannot be null or whitespace", nameof(functionName));
            
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _functions[functionName] = function;
        }

        /// <summary>
        /// Unregisters a function from the registry
        /// </summary>
        /// <param name="functionName">The name of the function to unregister</param>
        /// <returns>True if the function was found and unregistered, false if it wasn't registered</returns>
        public bool UnregisterFunction(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                return false;

            return _functions.Remove(functionName);
        }

        /// <summary>
        /// Executes a registered function
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <param name="node">The parse tree node containing the function call</param>
        /// <param name="context">The function execution context</param>
        /// <returns>The result of the function</returns>
        public DiceResultNode ExecuteFunction(string functionName, ParseTreeNode node, IFunctionContext context)
        {
            if (!IsRegistered(functionName))
                throw new InvalidOperationException($"Function '{functionName}' is not registered");

            return _functions[functionName].Execute(node, context);
        }
    }
}