using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Interface for plugins that provide game system functionality
    /// </summary>
    public interface IGameSystemPlugin : IPlugin
    {
        /// <summary>
        /// The game system this plugin supports
        /// </summary>
        string GameSystemName { get; }

        /// <summary>
        /// Game system version supported by this plugin
        /// </summary>
        string GameSystemVersion { get; }

        /// <summary>
        /// Available commands provided by this game system
        /// </summary>
        IReadOnlyList<GameSystemCommand> Commands { get; }

        /// <summary>
        /// Available lookup tables provided by this game system
        /// </summary>
        IReadOnlyList<GameSystemTable> Tables { get; }

        /// <summary>
        /// Execute a game system command
        /// </summary>
        /// <param name="commandName">Name of the command to execute</param>
        /// <param name="parameters">Command parameters</param>
        /// <param name="context">Execution context</param>
        /// <returns>Command result</returns>
        Task<GameSystemCommandResult> ExecuteCommandAsync(string commandName, 
            Dictionary<string, object> parameters, 
            GameSystemContext context);

        /// <summary>
        /// Lookup a value in a game system table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="key">Lookup key (usually a dice roll result)</param>
        /// <param name="context">Execution context</param>
        /// <returns>Table lookup result</returns>
        Task<GameSystemTableResult> LookupTableAsync(string tableName, 
            object key, 
            GameSystemContext context);

        /// <summary>
        /// Get help information for a command
        /// </summary>
        /// <param name="commandName">Command name, or null for all commands</param>
        /// <returns>Help information</returns>
        GameSystemHelpInfo GetHelp(string? commandName = null);
    }

    /// <summary>
    /// Game system command definition
    /// </summary>
    public record GameSystemCommand(
        string Name,
        string Description,
        IReadOnlyList<GameSystemParameter> Parameters,
        string HelpText,
        bool RequiresRandomization = true);

    /// <summary>
    /// Game system table definition
    /// </summary>
    public record GameSystemTable(
        string Name,
        string Description,
        Type KeyType,
        Type ValueType,
        string HelpText);

    /// <summary>
    /// Game system command parameter
    /// </summary>
    public record GameSystemParameter(
        string Name,
        Type Type,
        bool IsRequired,
        object? DefaultValue = null,
        string? Description = null);

    /// <summary>
    /// Game system command execution result
    /// </summary>
    public record GameSystemCommandResult(
        bool Success,
        object? Value,
        string Description,
        string? ErrorMessage = null,
        Dictionary<string, object>? AdditionalData = null);

    /// <summary>
    /// Game system table lookup result
    /// </summary>
    public record GameSystemTableResult(
        bool Success,
        object? Value,
        string Description,
        string? ErrorMessage = null);

    /// <summary>
    /// Game system execution context
    /// </summary>
    public record GameSystemContext(
        string UserId,
        string? ChannelId = null,
        string? GuildId = null,
        Dictionary<string, object>? UserData = null,
        Dictionary<string, object>? SessionData = null);

    /// <summary>
    /// Game system help information
    /// </summary>
    public record GameSystemHelpInfo(
        string GameSystemName,
        string Description,
        IReadOnlyList<CommandHelpInfo> Commands,
        IReadOnlyList<TableHelpInfo> Tables);

    /// <summary>
    /// Command help information
    /// </summary>
    public record CommandHelpInfo(
        string Name,
        string Description,
        string Usage,
        string Examples,
        IReadOnlyList<ParameterHelpInfo> Parameters);

    /// <summary>
    /// Table help information
    /// </summary>
    public record TableHelpInfo(
        string Name,
        string Description,
        string Usage,
        string Examples);

    /// <summary>
    /// Parameter help information
    /// </summary>
    public record ParameterHelpInfo(
        string Name,
        string Type,
        bool IsRequired,
        string? Description = null,
        string? DefaultValue = null);
}