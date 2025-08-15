using DiceRoller.Core.Models.Domain;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Services.Repositories;

/// <summary>
/// Repository for DiceRoll aggregates.
/// Note: Simplified version without event sourcing dependency.
/// </summary>
public interface IDiceRollRepository
{
    Task<DiceRoll?> GetByIdAsync(DiceRollId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiceRoll>> GetByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task SaveAsync(DiceRoll diceRoll, CancellationToken cancellationToken = default);
    Task DeleteAsync(DiceRollId id, CancellationToken cancellationToken = default);
}