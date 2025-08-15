using DiceRoller.Core.Models.ValueObjects;

namespace DiceRoller.Core.Services.Domain;

/// <summary>
/// Domain service interface for game system-specific lookups and operations.
/// Provides access to game system tables, calculations, and special mechanics.
/// </summary>
public interface IGameSystemLookupService
{
    /// <summary>
    /// Gets the name of the game system supported by this service.
    /// </summary>
    string GameSystemName { get; }

    /// <summary>
    /// Performs a lookup in a game system table based on a dice result.
    /// </summary>
    /// <param name="tableName">The name of the lookup table</param>
    /// <param name="rollResult">The dice result to use for the lookup</param>
    /// <returns>The lookup result as a formatted string</returns>
    Task<string> LookupAsync(string tableName, int rollResult);

    /// <summary>
    /// Performs a lookup in a game system table based on a dice result synchronously.
    /// </summary>
    /// <param name="tableName">The name of the lookup table</param>
    /// <param name="rollResult">The dice result to use for the lookup</param>
    /// <returns>The lookup result as a formatted string</returns>
    string Lookup(string tableName, int rollResult);

    /// <summary>
    /// Gets a list of available lookup tables for this game system.
    /// </summary>
    /// <returns>A collection of table names that can be used for lookups</returns>
    IReadOnlyList<string> GetAvailableTables();

    /// <summary>
    /// Gets information about a specific lookup table, including its purpose and valid range.
    /// </summary>
    /// <param name="tableName">The name of the table to get information about</param>
    /// <returns>Table information, or null if the table doesn't exist</returns>
    GameSystemTableInfo? GetTableInfo(string tableName);

    /// <summary>
    /// Executes a game system-specific command with the provided parameters.
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="parameters">Optional parameters for the command</param>
    /// <returns>The result of executing the command</returns>
    Task<DiceResult> ExecuteCommandAsync(string command, params string[] parameters);

    /// <summary>
    /// Executes a game system-specific command with the provided parameters synchronously.
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="parameters">Optional parameters for the command</param>
    /// <returns>The result of executing the command</returns>
    DiceResult ExecuteCommand(string command, params string[] parameters);

    /// <summary>
    /// Gets a list of available commands for this game system.
    /// </summary>
    /// <returns>A collection of command names that can be executed</returns>
    IReadOnlyList<string> GetAvailableCommands();

    /// <summary>
    /// Gets help information for a specific command.
    /// </summary>
    /// <param name="command">The command to get help for</param>
    /// <returns>Help information for the command, or null if the command doesn't exist</returns>
    string? GetCommandHelp(string command);
}

/// <summary>
/// Information about a game system lookup table.
/// </summary>
public sealed record GameSystemTableInfo(
    string Name,
    string Description,
    int MinValue,
    int MaxValue,
    string? Category = null)
{
    /// <summary>
    /// Gets whether the specified value is within the valid range for this table.
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if the value is valid for this table; otherwise, false</returns>
    public bool IsValidValue(int value) => value >= MinValue && value <= MaxValue;
}