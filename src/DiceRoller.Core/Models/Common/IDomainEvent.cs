namespace DiceRoller.Core.Models.Common;

/// <summary>
/// Marker interface for domain events that capture business state changes.
/// Domain events represent important business occurrences that other parts of the system may need to react to.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when this domain event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}