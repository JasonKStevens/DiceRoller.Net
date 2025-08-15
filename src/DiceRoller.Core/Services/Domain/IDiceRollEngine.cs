using DiceRoller.Core.Models.ValueObjects;

namespace DiceRoller.Core.Services.Domain;

/// <summary>
/// Domain service interface for evaluating dice expressions and producing results.
/// This service encapsulates the core business logic for dice rolling and expression evaluation.
/// </summary>
public interface IDiceRollEngine
{
    /// <summary>
    /// Evaluates a dice expression and returns the result.
    /// </summary>
    /// <param name="expression">The dice expression to evaluate</param>
    /// <returns>The result of evaluating the dice expression</returns>
    /// <exception cref="ArgumentNullException">Thrown when expression is null</exception>
    Task<DiceResult> EvaluateAsync(DiceExpression expression);

    /// <summary>
    /// Evaluates a dice expression synchronously and returns the result.
    /// </summary>
    /// <param name="expression">The dice expression to evaluate</param>
    /// <returns>The result of evaluating the dice expression</returns>
    /// <exception cref="ArgumentNullException">Thrown when expression is null</exception>
    DiceResult Evaluate(DiceExpression expression);

    /// <summary>
    /// Validates whether a dice expression is syntactically correct and can be evaluated.
    /// </summary>
    /// <param name="expression">The dice expression to validate</param>
    /// <returns>True if the expression is valid; otherwise, false</returns>
    bool IsValidExpression(DiceExpression expression);

    /// <summary>
    /// Gets a description of the syntax and capabilities supported by this dice roll engine.
    /// </summary>
    /// <returns>A description of supported dice notation syntax</returns>
    string GetSyntaxDescription();

    /// <summary>
    /// Evaluates multiple dice expressions in batch and returns their results.
    /// This method can be more efficient than evaluating expressions individually.
    /// </summary>
    /// <param name="expressions">The collection of dice expressions to evaluate</param>
    /// <returns>A collection of results corresponding to the input expressions</returns>
    Task<IReadOnlyList<DiceResult>> EvaluateBatchAsync(IEnumerable<DiceExpression> expressions);
}