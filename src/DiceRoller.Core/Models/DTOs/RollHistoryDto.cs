using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.DTOs;

/// <summary>
/// Data Transfer Object for roll history.
/// </summary>
public record RollHistoryDto(
    UserId UserId,
    IReadOnlyList<DiceRollDto> Rolls,
    int TotalRolls,
    DateTime? LastRollAt);