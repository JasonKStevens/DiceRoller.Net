using DiceRoller.Core.Models.Common;

namespace DiceRoller.Core.Models.ValueObjects.Identity;

/// <summary>
/// Strongly-typed identifier for User aggregates.
/// Provides type safety and prevents confusion between different types of identifiers.
/// </summary>
public sealed class UserId : StronglyTypedId<Guid>
{
    private UserId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new UserId with the specified Guid value.
    /// </summary>
    /// <param name="value">The Guid value for this identifier</param>
    /// <returns>A new UserId instance</returns>
    /// <exception cref="ArgumentException">Thrown when the value is empty</exception>
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(value));

        return new UserId(value);
    }

    /// <summary>
    /// Creates a new UserId with a randomly generated Guid value.
    /// </summary>
    /// <returns>A new UserId instance with a unique identifier</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Implicitly converts a Guid to a UserId.
    /// </summary>
    /// <param name="value">The Guid value to convert</param>
    public static implicit operator UserId(Guid value) => From(value);

    /// <summary>
    /// Implicitly converts a UserId to a Guid.
    /// </summary>
    /// <param name="id">The UserId to convert</param>
    public static implicit operator Guid(UserId id) => id.Value;
}