using System.Threading.Tasks;

namespace DiceRoller.Core.Services.Help
{
    /// <summary>
    /// Service for managing command help content and providing help responses
    /// </summary>
    public interface ICommandHelpService
    {
        /// <summary>
        /// Gets help content for a specific command
        /// </summary>
        /// <param name="commandName">The name of the command (e.g., "roll", "dq", "plugin")</param>
        /// <returns>Help content formatted for Discord, or null if command not found</returns>
        string GetCommandHelp(string commandName);

        /// <summary>
        /// Gets general help content for all available commands
        /// </summary>
        /// <returns>General help content formatted for Discord</returns>
        string GetGeneralHelp();

        /// <summary>
        /// Registers custom help content from a plugin
        /// </summary>
        /// <param name="commandName">The command name to register help for</param>
        /// <param name="helpContent">The help content formatted for Discord</param>
        /// <param name="pluginId">The ID of the plugin registering the help</param>
        void RegisterPluginHelp(string commandName, string helpContent, string pluginId);

        /// <summary>
        /// Unregisters help content for a plugin
        /// </summary>
        /// <param name="pluginId">The ID of the plugin to unregister help for</param>
        void UnregisterPluginHelp(string pluginId);

        /// <summary>
        /// Checks if help is available for a specific command
        /// </summary>
        /// <param name="commandName">The command name to check</param>
        /// <returns>True if help is available, false otherwise</returns>
        bool HasHelpFor(string commandName);
    }
}