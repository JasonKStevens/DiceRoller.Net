using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.ValueObjects;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Events;

/// <summary>
/// Domain event raised when a dice roll is executed (successfully or with failure).
/// </summary>
public sealed record DiceRollExecuted : IDomainEvent
{
    public DiceRollExecuted(
        DiceRollId diceRollId,
        DiceExpression expression,
        UserId requestedBy,
        DiceResult result,
        DateTime executedAt)
    {
        DiceRollId = diceRollId ?? throw new ArgumentNullException(nameof(diceRollId));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        ExecutedAt = executedAt;
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
    /// Gets the ID of the dice roll that was executed.
    /// </summary>
    public DiceRollId DiceRollId { get; }

    /// <summary>
    /// Gets the dice expression that was executed.
    /// </summary>
    public DiceExpression Expression { get; }

    /// <summary>
    /// Gets the ID of the user who requested the dice roll.
    /// </summary>
    public UserId RequestedBy { get; }

    /// <summary>
    /// Gets the result of the dice roll execution.
    /// </summary>
    public DiceResult Result { get; }

    /// <summary>
    /// Gets the timestamp when the dice roll was executed.
    /// </summary>
    public DateTime ExecutedAt { get; }

    /// <summary>
    /// Gets whether the dice roll execution was successful.
    /// </summary>
    public bool IsSuccess => !Result.IsError;

    /// <summary>
    /// Gets whether the dice roll execution failed.
    /// </summary>
    public bool IsFailure => Result.IsError;
}