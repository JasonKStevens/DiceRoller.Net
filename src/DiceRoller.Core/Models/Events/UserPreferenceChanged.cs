using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Events;

/// <summary>
/// Domain event raised when a user preference is added, updated, or removed.
/// </summary>
public sealed record UserPreferenceChanged : IDomainEvent
{
    public UserPreferenceChanged(
        UserId userId,
        string preferenceKey,
        string? newValue,
        string? oldValue)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        PreferenceKey = preferenceKey ?? throw new ArgumentNullException(nameof(preferenceKey));
        NewValue = newValue;
        OldValue = oldValue;
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
    /// Gets the ID of the user whose preference was changed.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the key of the preference that was changed.
    /// </summary>
    public string PreferenceKey { get; }

    /// <summary>
    /// Gets the new value for the preference, or null if the preference was removed.
    /// </summary>
    public string? NewValue { get; }

    /// <summary>
    /// Gets the old value for the preference, or null if this was a new preference.
    /// </summary>
    public string? OldValue { get; }

    /// <summary>
    /// Gets whether this represents the creation of a new preference.
    /// </summary>
    public bool IsNewPreference => OldValue is null && NewValue is not null;

    /// <summary>
    /// Gets whether this represents the removal of a preference.
    /// </summary>
    public bool IsPreferenceRemoval => NewValue is null && OldValue is not null;

    /// <summary>
    /// Gets whether this represents an update to an existing preference.
    /// </summary>
    public bool IsPreferenceUpdate => NewValue is not null && OldValue is not null;
}