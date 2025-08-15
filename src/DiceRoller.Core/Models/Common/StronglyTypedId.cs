namespace DiceRoller.Core.Models.Common;

/// <summary>
/// Base class for strongly-typed identifiers in Domain-Driven Design.
/// Provides type safety and prevents primitive obsession by wrapping primitive ID values.
/// </summary>
/// <typeparam name="TValue">The underlying type of the identifier (typically Guid, int, string, etc.)</typeparam>
public abstract class StronglyTypedId<TValue> : ValueObject, IComparable<StronglyTypedId<TValue>>
    where TValue : IComparable<TValue>
{
    protected StronglyTypedId(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Strongly-typed ID value cannot be null.");
        
        Value = value;
    }

    /// <summary>
    /// Gets the underlying value of this strongly-typed identifier.
    /// </summary>
    public TValue Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString() ?? string.Empty;

    public int CompareTo(StronglyTypedId<TValue>? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right)
    {
        return left?.CompareTo(right) < 0;
    }

    public static bool operator <=(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right)
    {
        return left?.CompareTo(right) <= 0;
    }

    public static bool operator >(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right)
    {
        return left?.CompareTo(right) > 0;
    }

    public static bool operator >=(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right)
    {
        return left?.CompareTo(right) >= 0;
    }
}