using DiceRoller.Core.Models.Domain;
using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Services.Repositories;

/// <summary>
/// Repository for User aggregates.
/// Note: Simplified version without event sourcing dependency.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserId id, CancellationToken cancellationToken = default);
}