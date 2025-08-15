using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Domain;

/// <summary>
/// Aggregate root representing a game session where multiple users can participate in dice rolling activities.
/// Tracks session state, participants, and roll history.
/// </summary>
public sealed class GameSession : AggregateRoot<GameSessionId>
{
    private readonly List<UserId> _participants = new();
    private readonly List<DiceRollId> _rollHistory = new();

    private GameSession(GameSessionId id, string name, UserId gameMaster, string? gameSystem = null)
        : base(id)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        GameMaster = gameMaster ?? throw new ArgumentNullException(nameof(gameMaster));
        GameSystem = gameSystem?.Trim();
        Status = GameSessionStatus.Active;
        
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Session name cannot be empty.", nameof(name));

        // Game Master is automatically a participant
        _participants.Add(GameMaster);

        EnsureInvariants();
    }

    /// <summary>
    /// Gets the name of this game session.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the user ID of the game master for this session.
    /// </summary>
    public UserId GameMaster { get; }

    /// <summary>
    /// Gets the optional game system being used (e.g., "DragonQuest", "Heroes", "D&D").
    /// </summary>
    public string? GameSystem { get; private set; }

    /// <summary>
    /// Gets the current status of this game session.
    /// </summary>
    public GameSessionStatus Status { get; private set; }

    /// <summary>
    /// Gets the timestamp when this session was started.
    /// </summary>
    public DateTime StartedAt => CreatedAt;

    /// <summary>
    /// Gets the timestamp when this session ended, if applicable.
    /// </summary>
    public DateTime? EndedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp of the last activity in this session.
    /// </summary>
    public DateTime? LastActivityAt { get; private set; }

    /// <summary>
    /// Gets a read-only list of participants in this session.
    /// </summary>
    public IReadOnlyList<UserId> Participants => _participants.AsReadOnly();

    /// <summary>
    /// Gets a read-only list of dice roll IDs that occurred in this session.
    /// </summary>
    public IReadOnlyList<DiceRollId> RollHistory => _rollHistory.AsReadOnly();

    /// <summary>
    /// Gets whether this session is currently active.
    /// </summary>
    public bool IsActive => Status == GameSessionStatus.Active;

    /// <summary>
    /// Gets whether this session has ended.
    /// </summary>
    public bool HasEnded => Status == GameSessionStatus.Ended;

    /// <summary>
    /// Gets whether this session is paused.
    /// </summary>
    public bool IsPaused => Status == GameSessionStatus.Paused;

    /// <summary>
    /// Creates a new GameSession aggregate.
    /// </summary>
    /// <param name="name">The name of the game session</param>
    /// <param name="gameMaster">The user who will be the game master</param>
    /// <param name="gameSystem">Optional game system being used</param>
    /// <returns>A new GameSession instance</returns>
    public static GameSession Create(string name, UserId gameMaster, string? gameSystem = null)
    {
        var session = new GameSession(GameSessionId.New(), name, gameMaster, gameSystem);
        
        return session;
    }

    /// <summary>
    /// Updates the session name.
    /// </summary>
    /// <param name="newName">The new name for the session</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session has ended</exception>
    public void UpdateName(string newName)
    {
        if (HasEnded)
            throw new BusinessRuleViolationException("Cannot update name of an ended session.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Session name cannot be null or empty.", nameof(newName));

        var trimmedName = newName.Trim();
        if (trimmedName.Length > 200)
            throw new ArgumentException("Session name cannot exceed 200 characters.", nameof(newName));

        Name = trimmedName;
        RecordActivity();
        EnsureInvariants();
    }

    /// <summary>
    /// Updates the game system for this session.
    /// </summary>
    /// <param name="gameSystem">The new game system, or null to clear</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session has ended</exception>
    public void UpdateGameSystem(string? gameSystem)
    {
        if (HasEnded)
            throw new BusinessRuleViolationException("Cannot update game system of an ended session.");

        GameSystem = gameSystem?.Trim();
        RecordActivity();
    }

    /// <summary>
    /// Adds a participant to this game session.
    /// </summary>
    /// <param name="userId">The user to add as a participant</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session has ended or user is already a participant</exception>
    public void AddParticipant(UserId userId)
    {
        if (HasEnded)
            throw new BusinessRuleViolationException("Cannot add participants to an ended session.");

        if (userId is null)
            throw new ArgumentNullException(nameof(userId));

        if (_participants.Contains(userId))
            throw new BusinessRuleViolationException("User is already a participant in this session.");

        _participants.Add(userId);
        RecordActivity();
        EnsureInvariants();
    }

    /// <summary>
    /// Removes a participant from this game session.
    /// </summary>
    /// <param name="userId">The user to remove</param>
    /// <returns>True if the user was removed; false if they weren't a participant</returns>
    /// <exception cref="BusinessRuleViolationException">Thrown when trying to remove the game master or when session has ended</exception>
    public bool RemoveParticipant(UserId userId)
    {
        if (HasEnded)
            throw new BusinessRuleViolationException("Cannot remove participants from an ended session.");

        if (userId is null)
            return false;

        if (userId.Equals(GameMaster))
            throw new BusinessRuleViolationException("Cannot remove the game master from the session.");

        var removed = _participants.Remove(userId);
        if (removed)
        {
            RecordActivity();
        }

        return removed;
    }

    /// <summary>
    /// Records a dice roll that occurred in this session.
    /// </summary>
    /// <param name="diceRollId">The ID of the dice roll</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session is not active</exception>
    public void RecordDiceRoll(DiceRollId diceRollId)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Cannot record dice rolls in an inactive session.");

        if (diceRollId is null)
            throw new ArgumentNullException(nameof(diceRollId));

        _rollHistory.Add(diceRollId);
        RecordActivity();
        EnsureInvariants();
    }

    /// <summary>
    /// Pauses this game session.
    /// </summary>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session is not active</exception>
    public void Pause()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Can only pause an active session.");

        Status = GameSessionStatus.Paused;
        RecordActivity();
    }

    /// <summary>
    /// Resumes this game session from a paused state.
    /// </summary>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session is not paused</exception>
    public void Resume()
    {
        if (!IsPaused)
            throw new BusinessRuleViolationException("Can only resume a paused session.");

        Status = GameSessionStatus.Active;
        RecordActivity();
    }

    /// <summary>
    /// Ends this game session.
    /// </summary>
    /// <exception cref="BusinessRuleViolationException">Thrown when the session has already ended</exception>
    public void End()
    {
        if (HasEnded)
            throw new BusinessRuleViolationException("Session has already ended.");

        Status = GameSessionStatus.Ended;
        EndedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if a user is a participant in this session.
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>True if the user is a participant; otherwise, false</returns>
    public bool IsParticipant(UserId userId)
    {
        return userId is not null && _participants.Contains(userId);
    }

    /// <summary>
    /// Gets the duration of this session.
    /// </summary>
    /// <returns>The duration from start to end (or current time if still active)</returns>
    public TimeSpan GetDuration()
    {
        var endTime = EndedAt ?? DateTime.UtcNow;
        return endTime - StartedAt;
    }

    private void RecordActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    protected override void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainInvariantViolationException("GameSession must have a valid name.");

        if (Name.Length > 200)
            throw new DomainInvariantViolationException("GameSession name cannot exceed 200 characters.");

        if (!_participants.Contains(GameMaster))
            throw new DomainInvariantViolationException("Game master must be a participant in the session.");

        if (_participants.Count > 100)
            throw new DomainInvariantViolationException("GameSession cannot have more than 100 participants.");

        if (_rollHistory.Count > 10000)
            throw new DomainInvariantViolationException("GameSession cannot have more than 10,000 dice rolls.");

        if (HasEnded && EndedAt is null)
            throw new DomainInvariantViolationException("Ended GameSession must have an end timestamp.");

        if (!HasEnded && EndedAt is not null)
            throw new DomainInvariantViolationException("Active GameSession should not have an end timestamp.");
    }
}

/// <summary>
/// Enumeration of possible game session statuses.
/// </summary>
public enum GameSessionStatus
{
    /// <summary>
    /// The session is currently active and accepting dice rolls.
    /// </summary>
    Active,

    /// <summary>
    /// The session is temporarily paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The session has ended and is no longer accepting dice rolls.
    /// </summary>
    Ended
}