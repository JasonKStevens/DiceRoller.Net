using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Models
{
    /// <summary>
    /// Represents the result of a dice roll operation
    /// </summary>
    public class DiceRollResult
    {
        /// <summary>
        /// Gets or sets the numeric value of the roll
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the human-readable breakdown of the roll
        /// </summary>
        public string Breakdown { get; set; }

        /// <summary>
        /// Gets or sets the typed result for formatted output
        /// </summary>
        public TypedResult TypedResult { get; set; }

        /// <summary>
        /// Gets or sets the original expression that was rolled
        /// </summary>
        public string OriginalExpression { get; set; }

        /// <summary>
        /// Gets or sets whether the roll was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets any error message if the roll failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Creates a DiceRollResult from a DiceResultNode
        /// </summary>
        /// <param name="node">The dice result node</param>
        /// <param name="originalExpression">The original expression</param>
        /// <returns>A new DiceRollResult</returns>
        public static DiceRollResult FromDiceResultNode(DiceResultNode node, string originalExpression)
        {
            return new DiceRollResult
            {
                Value = (decimal)node.Value,
                Breakdown = node.Breakdown,
                TypedResult = node.TypedResult,
                OriginalExpression = originalExpression,
                Success = true
            };
        }

        /// <summary>
        /// Creates a failed DiceRollResult
        /// </summary>
        /// <param name="expression">The original expression</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A failed DiceRollResult</returns>
        public static DiceRollResult Failed(string expression, string errorMessage)
        {
            return new DiceRollResult
            {
                OriginalExpression = expression,
                ErrorMessage = errorMessage,
                Success = false,
                TypedResult = TypedResult.NewSimpleResult(errorMessage)
            };
        }
    }
}