using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.Events;
using DiceRoller.Core.Models.ValueObjects;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Domain;

/// <summary>
/// Aggregate root representing a user of the dice rolling system.
/// Manages user aliases, preferences, and other user-specific data.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    private readonly Dictionary<string, DiceExpression> _aliases = new();
    private readonly Dictionary<string, string> _preferences = new();

    private User(UserId id, string displayName, string? externalId = null)
        : base(id)
    {
        DisplayName = displayName?.Trim() ?? throw new ArgumentNullException(nameof(displayName));
        ExternalId = externalId?.Trim();
        IsActive = true;

        if (string.IsNullOrWhiteSpace(DisplayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        EnsureInvariants();
    }

    /// <summary>
    /// Gets the display name for this user.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the external identifier for this user (e.g., Discord user ID).
    /// </summary>
    public string? ExternalId { get; }

    /// <summary>
    /// Gets whether this user account is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the timestamp when this user was last active.
    /// </summary>
    public DateTime? LastActiveAt { get; private set; }

    /// <summary>
    /// Gets a read-only collection of this user's aliases.
    /// </summary>
    public IReadOnlyDictionary<string, DiceExpression> Aliases => _aliases.AsReadOnly();

    /// <summary>
    /// Gets a read-only collection of this user's preferences.
    /// </summary>
    public IReadOnlyDictionary<string, string> Preferences => _preferences.AsReadOnly();

    /// <summary>
    /// Creates a new User aggregate.
    /// </summary>
    /// <param name="displayName">The display name for the user</param>
    /// <param name="externalId">Optional external identifier (e.g., Discord user ID)</param>
    /// <returns>A new User instance</returns>
    public static User Create(string displayName, string? externalId = null)
    {
        var user = new User(UserId.New(), displayName, externalId);
        
        return user;
    }

    /// <summary>
    /// Updates the user's display name.
    /// </summary>
    /// <param name="newDisplayName">The new display name</param>
    /// <exception cref="ArgumentException">Thrown when the display name is invalid</exception>
    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Display name cannot be null or empty.", nameof(newDisplayName));

        var trimmedName = newDisplayName.Trim();
        if (trimmedName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters.", nameof(newDisplayName));

        var oldDisplayName = DisplayName;
        DisplayName = trimmedName;
        
        MarkAsUpdated();
        RecordActivity();
        EnsureInvariants();
    }

    /// <summary>
    /// Adds or updates an alias for this user.
    /// </summary>
    /// <param name="aliasName">The name of the alias</param>
    /// <param name="expression">The dice expression for the alias</param>
    /// <exception cref="ArgumentException">Thrown when the alias name is invalid</exception>
    public void SetAlias(string aliasName, DiceExpression expression)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            throw new ArgumentException("Alias name cannot be null or empty.", nameof(aliasName));

        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var normalizedAliasName = aliasName.Trim().ToLowerInvariant();
        
        if (normalizedAliasName.Length > 50)
            throw new ArgumentException("Alias name cannot exceed 50 characters.", nameof(aliasName));

        if (!IsValidAliasName(normalizedAliasName))
            throw new ArgumentException("Alias name contains invalid characters.", nameof(aliasName));

        var isNewAlias = !_aliases.ContainsKey(normalizedAliasName);
        var oldExpression = isNewAlias ? null : _aliases[normalizedAliasName];

        _aliases[normalizedAliasName] = expression;

        MarkAsUpdated();
        RecordActivity();
        EnsureInvariants();

        // Raise domain event for alias change
        AddDomainEvent(new UserAliasChanged(
            Id,
            normalizedAliasName,
            expression,
            oldExpression,
            isNewAlias));
    }

    /// <summary>
    /// Removes an alias for this user.
    /// </summary>
    /// <param name="aliasName">The name of the alias to remove</param>
    /// <returns>True if the alias was removed; false if it didn't exist</returns>
    public bool RemoveAlias(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            return false;

        var normalizedAliasName = aliasName.Trim().ToLowerInvariant();
        
        if (!_aliases.TryGetValue(normalizedAliasName, out var removedExpression))
            return false;

        _aliases.Remove(normalizedAliasName);

        MarkAsUpdated();
        RecordActivity();

        // Raise domain event for alias removal
        AddDomainEvent(new UserAliasChanged(
            Id,
            normalizedAliasName,
            null,
            removedExpression,
            false));

        return true;
    }

    /// <summary>
    /// Gets an alias by name.
    /// </summary>
    /// <param name="aliasName">The name of the alias</param>
    /// <returns>The dice expression for the alias, or null if not found</returns>
    public DiceExpression? GetAlias(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
            return null;

        var normalizedAliasName = aliasName.Trim().ToLowerInvariant();
        return _aliases.TryGetValue(normalizedAliasName, out var expression) ? expression : null;
    }

    /// <summary>
    /// Sets a preference for this user.
    /// </summary>
    /// <param name="key">The preference key</param>
    /// <param name="value">The preference value</param>
    public void SetPreference(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Preference key cannot be null or empty.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var normalizedKey = key.Trim().ToLowerInvariant();
        
        if (normalizedKey.Length > 50)
            throw new ArgumentException("Preference key cannot exceed 50 characters.", nameof(key));

        if (value.Length > 500)
            throw new ArgumentException("Preference value cannot exceed 500 characters.", nameof(value));

        var oldValue = _preferences.TryGetValue(normalizedKey, out var existing) ? existing : null;
        _preferences[normalizedKey] = value;

        MarkAsUpdated();
        RecordActivity();

        // Raise domain event for preference change
        AddDomainEvent(new UserPreferenceChanged(
            Id,
            normalizedKey,
            value,
            oldValue));
    }

    /// <summary>
    /// Gets a preference value for this user.
    /// </summary>
    /// <param name="key">The preference key</param>
    /// <returns>The preference value, or null if not set</returns>
    public string? GetPreference(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var normalizedKey = key.Trim().ToLowerInvariant();
        return _preferences.TryGetValue(normalizedKey, out var value) ? value : null;
    }

    /// <summary>
    /// Removes a preference for this user.
    /// </summary>
    /// <param name="key">The preference key to remove</param>
    /// <returns>True if the preference was removed; false if it didn't exist</returns>
    public bool RemovePreference(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var normalizedKey = key.Trim().ToLowerInvariant();
        
        if (!_preferences.TryGetValue(normalizedKey, out var removedValue))
            return false;

        _preferences.Remove(normalizedKey);

        MarkAsUpdated();
        RecordActivity();

        // Raise domain event for preference removal
        AddDomainEvent(new UserPreferenceChanged(
            Id,
            normalizedKey,
            null,
            removedValue));

        return true;
    }

    /// <summary>
    /// Deactivates this user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Reactivates this user account.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        RecordActivity();
        MarkAsUpdated();
    }

    /// <summary>
    /// Records user activity by updating the last active timestamp.
    /// </summary>
    public void RecordActivity()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    private static bool IsValidAliasName(string aliasName)
    {
        // Alias names should only contain letters, numbers, and underscores
        return aliasName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    protected override void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            throw new DomainInvariantViolationException("User must have a valid display name.");

        if (DisplayName.Length > 100)
            throw new DomainInvariantViolationException("User display name cannot exceed 100 characters.");

        if (_aliases.Count > 100)
            throw new DomainInvariantViolationException("User cannot have more than 100 aliases.");

        if (_preferences.Count > 50)
            throw new DomainInvariantViolationException("User cannot have more than 50 preferences.");
    }
}