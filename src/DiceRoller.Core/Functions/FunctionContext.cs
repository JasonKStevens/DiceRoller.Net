using System;
using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Context for function execution providing access to the visitor and other services
    /// </summary>
    public class FunctionContext : IFunctionContext
    {
        private readonly Func<ParseTreeNode, DiceResultNode> _evaluator;

        public FunctionContext(Func<ParseTreeNode, DiceResultNode> evaluator)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        /// <summary>
        /// Evaluates a parse tree node using the current visitor
        /// </summary>
        /// <param name="node">The node to evaluate</param>
        /// <returns>The result of the evaluation</returns>
        public DiceResultNode EvaluateNode(ParseTreeNode node)
        {
            return _evaluator(node);
        }
    }
}