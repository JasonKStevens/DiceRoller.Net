using System.Threading.Tasks;
using DiceRoller.Core.Models;

namespace DiceRoller.Core.Abstractions
{
    /// <summary>
    /// Service interface for dice rolling operations
    /// </summary>
    public interface IDiceRollingService
    {
        /// <summary>
        /// Rolls dice based on the provided request
        /// </summary>
        /// <param name="request">The dice roll request</param>
        /// <returns>The result of the dice roll</returns>
        Task<DiceRollResult> RollAsync(DiceRollRequest request);

        /// <summary>
        /// Parses a dice expression without evaluating it
        /// </summary>
        /// <param name="expression">The dice expression to parse</param>
        /// <returns>True if the expression is valid</returns>
        bool IsValidExpression(string expression);
    }
}