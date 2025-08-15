using DiceRoller.Core.Models.ValueObjects.Identity;

namespace DiceRoller.Core.Models.DTOs;

/// <summary>
/// Data Transfer Object for dice roll results.
/// </summary>
public record DiceRollDto(
    DiceRollId Id,
    string Expression,
    UserId RequestedBy,
    string? Context,
    string Status,
    int? Value,
    string? Breakdown,
    string? ErrorMessage,
    DateTime? ExecutedAt,
    DateTime CreatedAt);