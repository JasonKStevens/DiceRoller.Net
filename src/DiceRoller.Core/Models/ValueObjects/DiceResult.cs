using DiceRoller.Core.Models.Common;

namespace DiceRoller.Core.Models.ValueObjects;

/// <summary>
/// Value object representing the result of evaluating a dice expression.
/// Contains both the numeric result and a breakdown of how the result was calculated.
/// </summary>
public sealed class DiceResult : ValueObject
{
    private DiceResult(int value, string breakdown, DiceExpression originalExpression)
    {
        Value = value;
        Breakdown = breakdown;
        OriginalExpression = originalExpression;
        CalculatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the numeric result of the dice roll evaluation.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets the detailed breakdown showing how the result was calculated.
    /// For example: "[4, 2, 6] + 3" for the expression "3d6+3"
    /// </summary>
    public string Breakdown { get; }

    /// <summary>
    /// Gets the original dice expression that was evaluated to produce this result.
    /// </summary>
    public DiceExpression OriginalExpression { get; }

    /// <summary>
    /// Gets the timestamp when this result was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; }

    /// <summary>
    /// Gets whether this result represents a success (typically used for binary outcomes).
    /// </summary>
    public bool IsSuccess => Value > 0;

    /// <summary>
    /// Creates a new DiceResult with the specified values.
    /// </summary>
    /// <param name="value">The numeric result</param>
    /// <param name="breakdown">The calculation breakdown</param>
    /// <param name="originalExpression">The original dice expression</param>
    /// <returns>A new DiceResult instance</returns>
    /// <exception cref="ArgumentException">Thrown when breakdown is null or empty</exception>
    public static DiceResult Create(int value, string breakdown, DiceExpression originalExpression)
    {
        if (string.IsNullOrWhiteSpace(breakdown))
            throw new ArgumentException("Breakdown cannot be null or empty.", nameof(breakdown));

        if (originalExpression is null)
            throw new ArgumentNullException(nameof(originalExpression));

        return new DiceResult(value, breakdown.Trim(), originalExpression);
    }

    /// <summary>
    /// Creates a new DiceResult representing a failure or error condition.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the breakdown</param>
    /// <param name="originalExpression">The original dice expression that failed</param>
    /// <returns>A new DiceResult instance representing a failure</returns>
    public static DiceResult CreateFailure(string errorMessage, DiceExpression originalExpression)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));

        if (originalExpression is null)
            throw new ArgumentNullException(nameof(originalExpression));

        return new DiceResult(0, $"Error: {errorMessage.Trim()}", originalExpression);
    }

    /// <summary>
    /// Gets whether this result represents an error condition.
    /// </summary>
    public bool IsError => Breakdown.StartsWith("Error:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the error message if this result represents an error, otherwise null.
    /// </summary>
    public string? ErrorMessage => IsError ? Breakdown[6..].Trim() : null;

    /// <summary>
    /// Creates a new DiceResult with the same values but a different timestamp.
    /// </summary>
    /// <returns>A new DiceResult instance with an updated timestamp</returns>
    public DiceResult WithUpdatedTimestamp()
    {
        return new DiceResult(Value, Breakdown, OriginalExpression);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Breakdown;
        yield return OriginalExpression;
        // Note: CalculatedAt is intentionally excluded from equality comparison
        // as two results with the same value/breakdown should be considered equal
        // regardless of when they were calculated
    }

    public override string ToString() => $"{Value} ({Breakdown})";
}