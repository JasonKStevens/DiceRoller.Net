using DiceRoller.Core.Models.Common;

namespace DiceRoller.Core.Models.ValueObjects.Identity;

/// <summary>
/// Strongly-typed identifier for GameSession aggregates.
/// Provides type safety and prevents confusion between different types of identifiers.
/// </summary>
public sealed class GameSessionId : StronglyTypedId<Guid>
{
    private GameSessionId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new GameSessionId with the specified Guid value.
    /// </summary>
    /// <param name="value">The Guid value for this identifier</param>
    /// <returns>A new GameSessionId instance</returns>
    /// <exception cref="ArgumentException">Thrown when the value is empty</exception>
    public static GameSessionId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GameSessionId cannot be empty.", nameof(value));

        return new GameSessionId(value);
    }

    /// <summary>
    /// Creates a new GameSessionId with a randomly generated Guid value.
    /// </summary>
    /// <returns>A new GameSessionId instance with a unique identifier</returns>
    public static GameSessionId New() => new(Guid.NewGuid());

    /// <summary>
    /// Implicitly converts a Guid to a GameSessionId.
    /// </summary>
    /// <param name="value">The Guid value to convert</param>
    public static implicit operator GameSessionId(Guid value) => From(value);

    /// <summary>
    /// Implicitly converts a GameSessionId to a Guid.
    /// </summary>
    /// <param name="id">The GameSessionId to convert</param>
    public static implicit operator Guid(GameSessionId id) => id.Value;
}