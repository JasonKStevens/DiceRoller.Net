namespace DiceRoller.Core.Models.Common;

/// <summary>
/// Base class for aggregate roots in Domain-Driven Design.
/// An aggregate root is the only entry point to an aggregate and is responsible for maintaining
/// business invariants and coordinating changes within the aggregate boundary.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier</typeparam>
public abstract class AggregateRoot<TId> where TId : StronglyTypedId<Guid>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier for this aggregate.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Gets the timestamp when this aggregate was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp when this aggregate was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the collection of domain events that have been raised by this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to this aggregate's collection of events.
    /// Domain events will be dispatched when the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));

        _domainEvents.Add(domainEvent);
        MarkAsUpdated();
    }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// This is typically called after domain events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks this aggregate as updated by setting the UpdatedAt timestamp.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Template method for validating business invariants.
    /// Override this method to implement aggregate-specific validation rules.
    /// </summary>
    protected virtual void ValidateInvariants()
    {
        // Default implementation - override in derived classes
    }

    /// <summary>
    /// Validates the aggregate's business invariants.
    /// Throws a DomainInvariantViolationException if any invariants are violated.
    /// </summary>
    public void EnsureInvariants()
    {
        ValidateInvariants();
    }
}