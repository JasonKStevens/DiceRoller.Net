using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.DTOs;

/// <summary>
/// Data Transfer Object for user aliases.
/// </summary>
public record UserAliasDto(
    UserId UserId,
    string AliasName,
    string Expression,
    DateTime CreatedAt);