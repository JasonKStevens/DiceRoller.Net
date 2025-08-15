using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Abstractions
{
    /// <summary>
    /// Interface for evaluating parsed dice expressions
    /// </summary>
    public interface IDiceExpressionEvaluator
    {
        /// <summary>
        /// Evaluates a parsed syntax tree
        /// </summary>
        /// <param name="parseTree">The parsed syntax tree</param>
        /// <returns>The result of the dice roll</returns>
        DiceResultNode Evaluate(ParseTree parseTree);

        /// <summary>
        /// Evaluates a parsed syntax tree root node
        /// </summary>
        /// <param name="rootNode">The root node of the syntax tree</param>
        /// <returns>The result of the dice roll</returns>
        DiceResultNode Evaluate(ParseTreeNode rootNode);
    }
}