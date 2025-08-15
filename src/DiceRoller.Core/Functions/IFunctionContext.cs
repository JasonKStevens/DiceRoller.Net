using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Context for function execution providing access to the visitor and other services
    /// </summary>
    public interface IFunctionContext
    {
        /// <summary>
        /// Evaluates a parse tree node using the current visitor
        /// </summary>
        /// <param name="node">The node to evaluate</param>
        /// <returns>The result of the evaluation</returns>
        DiceResultNode EvaluateNode(ParseTreeNode node);
    }
}