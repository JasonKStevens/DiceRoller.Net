using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.ValueObjects;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Events;

/// <summary>
/// Domain event raised when a new dice roll is created.
/// </summary>
public sealed record DiceRollCreated : IDomainEvent
{
    public DiceRollCreated(
        DiceRollId diceRollId,
        DiceExpression expression,
        UserId requestedBy,
        string? context)
    {
        DiceRollId = diceRollId ?? throw new ArgumentNullException(nameof(diceRollId));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        Context = context;
    }

    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this domain event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the ID of the dice roll that was created.
    /// </summary>
    public DiceRollId DiceRollId { get; }

    /// <summary>
    /// Gets the dice expression that was created.
    /// </summary>
    public DiceExpression Expression { get; }

    /// <summary>
    /// Gets the ID of the user who requested the dice roll.
    /// </summary>
    public UserId RequestedBy { get; }

    /// <summary>
    /// Gets the optional context or description for the dice roll.
    /// </summary>
    public string? Context { get; }
}