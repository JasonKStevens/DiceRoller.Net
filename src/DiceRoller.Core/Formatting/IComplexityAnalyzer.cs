using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Analyzes the complexity of dice expressions for adaptive formatting
/// </summary>
public interface IComplexityAnalyzer
{
    /// <summary>
    /// Analyzes the complexity of a TypedResult structure
    /// </summary>
    /// <param name="typedResult">The TypedResult to analyze</param>
    /// <returns>The complexity level of the expression</returns>
    ComplexityLevel AnalyzeComplexity(TypedResult typedResult);
}