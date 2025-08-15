using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Interface for individual dice functions that can be executed
    /// </summary>
    public interface IDiceFunction
    {
        /// <summary>
        /// Gets the name of the function
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the function with the given parameters
        /// </summary>
        /// <param name="node">The parse tree node containing the function call</param>
        /// <param name="context">The function execution context</param>
        /// <returns>The result of the function</returns>
        DiceResultNode Execute(ParseTreeNode node, IFunctionContext context);
    }
}