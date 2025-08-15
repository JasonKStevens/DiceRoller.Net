using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Interface for formatting dice expression breakdowns with adaptive complexity handling
/// </summary>
public interface IAdaptiveBreakdownFormatter
{
    /// <summary>
    /// Formats a dice expression breakdown based on its TypedResult structure
    /// </summary>
    /// <param name="typedResult">The TypedResult structure to format</param>
    /// <param name="breakdown">The raw breakdown text</param>
    /// <param name="originalExpression">The original dice expression</param>
    /// <returns>The formatted breakdown text</returns>
    string FormatBreakdown(TypedResult typedResult, string breakdown, string originalExpression);
}