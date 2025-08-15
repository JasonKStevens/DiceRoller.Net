using System;
using System.Threading.Tasks;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Models;

namespace DiceRoller.Core.Services
{
    /// <summary>
    /// Main service for dice rolling operations
    /// </summary>
    public class DiceRollingService : IDiceRollingService
    {
        private readonly IDiceExpressionParser _parser;
        private readonly IDiceExpressionEvaluator _evaluator;

        /// <summary>
        /// Initializes a new instance of the DiceRollingService
        /// </summary>
        /// <param name="parser">The dice expression parser</param>
        /// <param name="evaluator">The dice expression evaluator</param>
        public DiceRollingService(IDiceExpressionParser parser, IDiceExpressionEvaluator evaluator)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        /// <summary>
        /// Rolls dice based on the provided request
        /// </summary>
        /// <param name="request">The dice roll request</param>
        /// <returns>The result of the dice roll</returns>
        public async Task<DiceRollResult> RollAsync(DiceRollRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Expression))
                return DiceRollResult.Failed(request.Expression, "Expression cannot be empty");

            try
            {
                // Parse the expression
                var parseTree = _parser.Parse(request.Expression);
                
                // Evaluate the parsed expression
                var result = _evaluator.Evaluate(parseTree);
                
                return await Task.FromResult(DiceRollResult.FromDiceResultNode(result, request.Expression));
            }
            catch (Exception ex)
            {
                return DiceRollResult.Failed(request.Expression, ex.Message);
            }
        }

        /// <summary>
        /// Parses a dice expression without evaluating it
        /// </summary>
        /// <param name="expression">The dice expression to parse</param>
        /// <returns>True if the expression is valid</returns>
        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                var parseTree = _parser.Parse(expression);
                return !parseTree.HasErrors();
            }
            catch
            {
                return false;
            }
        }
    }
}