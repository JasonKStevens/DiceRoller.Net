using DiceRoller.Core.Models.Common;
using System.Text.RegularExpressions;

namespace DiceRoller.Core.Models.ValueObjects;

/// <summary>
/// Value object representing a dice expression in dice notation (e.g., "3d6+2", "d20", "2d10!").
/// Ensures that dice expressions are valid and immutable.
/// </summary>
public sealed class DiceExpression : ValueObject
{
    private static readonly Regex DiceExpressionPattern = new(
        @"^[\w\d\s\+\-\*\/\(\)\!\>\<\=\,\#\.]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private DiceExpression(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the string representation of this dice expression.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new DiceExpression from a string value.
    /// </summary>
    /// <param name="expression">The dice expression string</param>
    /// <returns>A new DiceExpression instance</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is invalid</exception>
    public static DiceExpression From(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Dice expression cannot be null or empty.", nameof(expression));

        var trimmedExpression = expression.Trim();

        if (trimmedExpression.Length > 500)
            throw new ArgumentException("Dice expression cannot exceed 500 characters.", nameof(expression));

        if (!DiceExpressionPattern.IsMatch(trimmedExpression))
            throw new ArgumentException("Dice expression contains invalid characters.", nameof(expression));

        return new DiceExpression(trimmedExpression);
    }

    /// <summary>
    /// Determines whether the specified string is a valid dice expression.
    /// </summary>
    /// <param name="expression">The expression to validate</param>
    /// <returns>True if the expression is valid; otherwise, false</returns>
    public static bool IsValid(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var trimmedExpression = expression.Trim();

        if (trimmedExpression.Length > 500)
            return false;

        return DiceExpressionPattern.IsMatch(trimmedExpression);
    }

    /// <summary>
    /// Gets whether this dice expression contains a comment (indicated by '#').
    /// </summary>
    public bool HasComment => Value.Contains('#');

    /// <summary>
    /// Gets the comment portion of the dice expression, if any.
    /// </summary>
    public string? Comment
    {
        get
        {
            var commentIndex = Value.IndexOf('#');
            return commentIndex >= 0 ? Value[(commentIndex + 1)..].Trim() : null;
        }
    }

    /// <summary>
    /// Gets the dice expression without the comment portion.
    /// </summary>
    public string ExpressionWithoutComment
    {
        get
        {
            var commentIndex = Value.IndexOf('#');
            return commentIndex >= 0 ? Value[..commentIndex].Trim() : Value;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a string to a DiceExpression.
    /// </summary>
    /// <param name="expression">The string to convert</param>
    public static implicit operator DiceExpression(string expression) => From(expression);

    /// <summary>
    /// Implicitly converts a DiceExpression to a string.
    /// </summary>
    /// <param name="diceExpression">The DiceExpression to convert</param>
    public static implicit operator string(DiceExpression diceExpression) => diceExpression.Value;
}