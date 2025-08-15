namespace DiceRoller.Core.Models.Common;

/// <summary>
/// Base exception class for domain-specific exceptions.
/// Use this for exceptions that represent business rule violations or domain invariant failures.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an aggregate cannot be found.
/// </summary>
public class AggregateNotFoundException : DomainException
{
    public AggregateNotFoundException(string aggregateType, object id) 
        : base($"{aggregateType} with id '{id}' was not found.")
    {
        AggregateType = aggregateType;
        Id = id;
    }

    public string AggregateType { get; }
    public object Id { get; }
}

/// <summary>
/// Exception thrown when an invariant of a domain object is violated.
/// </summary>
public class DomainInvariantViolationException : DomainException
{
    public DomainInvariantViolationException(string message) : base(message)
    {
    }

    public DomainInvariantViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}