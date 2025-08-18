using System.Collections.Concurrent;
using DiceRoller.Core.Content;

namespace DiceRoller.Core.Services.Help
{
    /// <summary>
    /// Service for managing command help content and providing help responses
    /// </summary>
    public class CommandHelpService : ICommandHelpService
    {
        private static readonly Dictionary<string, string> _builtInHelp = new()
        {
            { "roll", HelpContent.RollCommandHelp },
            { "rollhelp", HelpContent.RollHelpCommandHelp },
            { "dq", HelpContent.DragonQuestCommandHelp },
            { "heroes", HelpContent.HeroesCommandHelp },
            { "hero", HelpContent.HeroesCommandHelp }, // Alias for heroes
            { "party", HelpContent.PartyCommandHelp },
            { "settings", HelpContent.SettingsCommandHelp },
            { "plugin", HelpContent.PluginCommandHelp },
            { "general", HelpContent.GeneralHelp },
            { "notation", HelpContent.DiceNotationReference },
            { "advanced", HelpContent.AdvancedFeaturesHelp }
        };

        private readonly ConcurrentDictionary<string, PluginHelpEntry> _pluginHelp = new();

        /// <inheritdoc/>
        public string GetCommandHelp(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return GetGeneralHelp();
            }

            var normalizedCommand = commandName.ToLowerInvariant().Trim();

            // Check built-in help first
            if (_builtInHelp.TryGetValue(normalizedCommand, out var builtInContent))
            {
                return builtInContent;
            }

            // Check plugin help
            if (_pluginHelp.TryGetValue(normalizedCommand, out var pluginEntry))
            {
                return pluginEntry.Content;
            }

            return null;
        }

        /// <inheritdoc/>
        public string GetGeneralHelp()
        {
            return HelpContent.GeneralHelp;
        }

        /// <inheritdoc/>
        public void RegisterPluginHelp(string commandName, string helpContent, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException("Command name cannot be null or empty", nameof(commandName));
            }

            if (string.IsNullOrWhiteSpace(helpContent))
            {
                throw new ArgumentException("Help content cannot be null or empty", nameof(helpContent));
            }

            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new ArgumentException("Plugin ID cannot be null or empty", nameof(pluginId));
            }

            var normalizedCommand = commandName.ToLowerInvariant().Trim();
            
            // Don't allow plugins to override built-in help
            if (_builtInHelp.ContainsKey(normalizedCommand))
            {
                throw new InvalidOperationException($"Cannot register plugin help for built-in command: {commandName}");
            }

            var entry = new PluginHelpEntry(helpContent, pluginId);
            _pluginHelp.AddOrUpdate(normalizedCommand, entry, (_, _) => entry);
        }

        /// <inheritdoc/>
        public void UnregisterPluginHelp(string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId))
            {
                return;
            }

            var keysToRemove = _pluginHelp
                .Where(kvp => kvp.Value.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _pluginHelp.TryRemove(key, out _);
            }
        }

        /// <inheritdoc/>
        public bool HasHelpFor(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return true; // General help is always available
            }

            var normalizedCommand = commandName.ToLowerInvariant().Trim();
            return _builtInHelp.ContainsKey(normalizedCommand) || _pluginHelp.ContainsKey(normalizedCommand);
        }

        /// <summary>
        /// Gets a truncated version of help content that fits within Discord's character limits
        /// </summary>
        /// <param name="content">The help content to truncate</param>
        /// <param name="maxLength">Maximum length (default 2000 for Discord)</param>
        /// <returns>Truncated content with ellipsis if needed</returns>
        public static string TruncateForDiscord(string content, int maxLength = 2000)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            {
                return content;
            }

            const string suffix = "\n\n*...content truncated. Use `/rollhelp` for full documentation.*";
            
            // Try to truncate at a reasonable breaking point
            var truncateAt = maxLength - suffix.Length; // Leave room for the exact suffix length
            var truncated = content.Substring(0, truncateAt);
            
            // Try to break at a line ending
            var lastNewline = truncated.LastIndexOf('\n');
            if (lastNewline > truncateAt - 200) // If the break isn't too far back
            {
                truncated = truncated.Substring(0, lastNewline);
            }

            return truncated + suffix;
        }

        private record PluginHelpEntry(string Content, string PluginId);
    }
}