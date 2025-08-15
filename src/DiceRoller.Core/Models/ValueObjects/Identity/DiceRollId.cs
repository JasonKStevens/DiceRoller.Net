using DiceRoller.Core.Models.Common;

namespace DiceRoller.Core.Models.ValueObjects.Identity;

/// <summary>
/// Strongly-typed identifier for DiceRoll aggregates.
/// Provides type safety and prevents confusion between different types of identifiers.
/// </summary>
public sealed class DiceRollId : StronglyTypedId<Guid>
{
    private DiceRollId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new DiceRollId with the specified Guid value.
    /// </summary>
    /// <param name="value">The Guid value for this identifier</param>
    /// <returns>A new DiceRollId instance</returns>
    /// <exception cref="ArgumentException">Thrown when the value is empty</exception>
    public static DiceRollId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DiceRollId cannot be empty.", nameof(value));

        return new DiceRollId(value);
    }

    /// <summary>
    /// Creates a new DiceRollId with a randomly generated Guid value.
    /// </summary>
    /// <returns>A new DiceRollId instance with a unique identifier</returns>
    public static DiceRollId New() => new(Guid.NewGuid());

    /// <summary>
    /// Implicitly converts a Guid to a DiceRollId.
    /// </summary>
    /// <param name="value">The Guid value to convert</param>
    public static implicit operator DiceRollId(Guid value) => From(value);

    /// <summary>
    /// Implicitly converts a DiceRollId to a Guid.
    /// </summary>
    /// <param name="id">The DiceRollId to convert</param>
    public static implicit operator Guid(DiceRollId id) => id.Value;
}