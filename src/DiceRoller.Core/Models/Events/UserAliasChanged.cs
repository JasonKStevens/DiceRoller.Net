using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.ValueObjects;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Events;

/// <summary>
/// Domain event raised when a user alias is added, updated, or removed.
/// </summary>
public sealed record UserAliasChanged : IDomainEvent
{
    public UserAliasChanged(
        UserId userId,
        string aliasName,
        DiceExpression? newExpression,
        DiceExpression? oldExpression,
        bool isNewAlias)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        AliasName = aliasName ?? throw new ArgumentNullException(nameof(aliasName));
        NewExpression = newExpression;
        OldExpression = oldExpression;
        IsNewAlias = isNewAlias;
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
    /// Gets the ID of the user whose alias was changed.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the name of the alias that was changed.
    /// </summary>
    public string AliasName { get; }

    /// <summary>
    /// Gets the new expression for the alias, or null if the alias was removed.
    /// </summary>
    public DiceExpression? NewExpression { get; }

    /// <summary>
    /// Gets the old expression for the alias, or null if this was a new alias.
    /// </summary>
    public DiceExpression? OldExpression { get; }

    /// <summary>
    /// Gets whether this represents the creation of a new alias.
    /// </summary>
    public bool IsNewAlias { get; }

    /// <summary>
    /// Gets whether this represents the removal of an alias.
    /// </summary>
    public bool IsAliasRemoval => NewExpression is null && OldExpression is not null;

    /// <summary>
    /// Gets whether this represents an update to an existing alias.
    /// </summary>
    public bool IsAliasUpdate => NewExpression is not null && OldExpression is not null && !IsNewAlias;
}