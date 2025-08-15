using DiceRoller.Core.Models.Common;
using DiceRoller.Core.Models.Events;
using DiceRoller.Core.Models.ValueObjects;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.Domain;

/// <summary>
/// Aggregate root representing a dice roll operation.
/// Encapsulates the dice expression, execution details, and resulting values.
/// </summary>
public sealed class DiceRoll : AggregateRoot<DiceRollId>
{
    private DiceRoll(DiceRollId id, DiceExpression expression, UserId requestedBy, string? context = null)
        : base(id)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        Context = context?.Trim();
        Status = DiceRollStatus.Created;

        EnsureInvariants();
    }

    /// <summary>
    /// Gets the dice expression that was or will be evaluated.
    /// </summary>
    public DiceExpression Expression { get; }

    /// <summary>
    /// Gets the identifier of the user who requested this dice roll.
    /// </summary>
    public UserId RequestedBy { get; }

    /// <summary>
    /// Gets the optional context or description for this dice roll (e.g., "Attack roll", "Damage").
    /// </summary>
    public string? Context { get; }

    /// <summary>
    /// Gets the current status of this dice roll.
    /// </summary>
    public DiceRollStatus Status { get; private set; }

    /// <summary>
    /// Gets the result of the dice roll evaluation, if executed.
    /// </summary>
    public DiceResult? Result { get; private set; }

    /// <summary>
    /// Gets the timestamp when this dice roll was executed, if applicable.
    /// </summary>
    public DateTime? ExecutedAt { get; private set; }

    /// <summary>
    /// Gets whether this dice roll has been executed.
    /// </summary>
    public bool IsExecuted => Status == DiceRollStatus.Executed;

    /// <summary>
    /// Gets whether this dice roll has failed.
    /// </summary>
    public bool HasFailed => Status == DiceRollStatus.Failed;

    /// <summary>
    /// Creates a new DiceRoll aggregate.
    /// </summary>
    /// <param name="expression">The dice expression to evaluate</param>
    /// <param name="requestedBy">The user requesting the dice roll</param>
    /// <param name="context">Optional context or description for the roll</param>
    /// <returns>A new DiceRoll instance</returns>
    public static DiceRoll Create(DiceExpression expression, UserId requestedBy, string? context = null)
    {
        var diceRoll = new DiceRoll(DiceRollId.New(), expression, requestedBy, context);
        
        // Raise domain event for dice roll creation
        diceRoll.AddDomainEvent(new DiceRollCreated(
            diceRoll.Id,
            diceRoll.Expression,
            diceRoll.RequestedBy,
            diceRoll.Context));

        return diceRoll;
    }

    /// <summary>
    /// Executes the dice roll with the provided result.
    /// </summary>
    /// <param name="result">The result of evaluating the dice expression</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the dice roll has already been executed</exception>
    public void Execute(DiceResult result)
    {
        if (IsExecuted)
            throw new BusinessRuleViolationException("Dice roll has already been executed.");

        if (HasFailed)
            throw new BusinessRuleViolationException("Cannot execute a failed dice roll.");

        if (result is null)
            throw new ArgumentNullException(nameof(result));

        Result = result;
        ExecutedAt = DateTime.UtcNow;
        Status = result.IsError ? DiceRollStatus.Failed : DiceRollStatus.Executed;

        MarkAsUpdated();
        EnsureInvariants();

        // Raise domain event for dice roll execution
        AddDomainEvent(new DiceRollExecuted(
            Id,
            Expression,
            RequestedBy,
            Result,
            ExecutedAt.Value));
    }

    /// <summary>
    /// Marks the dice roll as failed with an error message.
    /// </summary>
    /// <param name="errorMessage">The reason for the failure</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when the dice roll has already been executed</exception>
    public void MarkAsFailed(string errorMessage)
    {
        if (IsExecuted)
            throw new BusinessRuleViolationException("Cannot mark an executed dice roll as failed.");

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(errorMessage));

        Result = DiceResult.CreateFailure(errorMessage, Expression);
        ExecutedAt = DateTime.UtcNow;
        Status = DiceRollStatus.Failed;

        MarkAsUpdated();
        EnsureInvariants();

        // Raise domain event for dice roll failure
        AddDomainEvent(new DiceRollExecuted(
            Id,
            Expression,
            RequestedBy,
            Result,
            ExecutedAt.Value));
    }

    protected override void ValidateInvariants()
    {
        if (Expression is null)
            throw new DomainInvariantViolationException("DiceRoll must have a valid expression.");

        if (RequestedBy is null)
            throw new DomainInvariantViolationException("DiceRoll must have a valid user who requested it.");

        if (Status == DiceRollStatus.Executed && Result is null)
            throw new DomainInvariantViolationException("Executed DiceRoll must have a result.");

        if (Status == DiceRollStatus.Failed && Result is null)
            throw new DomainInvariantViolationException("Failed DiceRoll must have a result with error information.");

        if ((Status == DiceRollStatus.Executed || Status == DiceRollStatus.Failed) && ExecutedAt is null)
            throw new DomainInvariantViolationException("Executed or failed DiceRoll must have an execution timestamp.");

        if (Status == DiceRollStatus.Created && (Result is not null || ExecutedAt is not null))
            throw new DomainInvariantViolationException("Created DiceRoll should not have a result or execution timestamp.");
    }
}

/// <summary>
/// Enumeration of possible dice roll statuses.
/// </summary>
public enum DiceRollStatus
{
    /// <summary>
    /// The dice roll has been created but not yet executed.
    /// </summary>
    Created,

    /// <summary>
    /// The dice roll has been successfully executed and has a result.
    /// </summary>
    Executed,

    /// <summary>
    /// The dice roll execution failed due to an error.
    /// </summary>
    Failed
}