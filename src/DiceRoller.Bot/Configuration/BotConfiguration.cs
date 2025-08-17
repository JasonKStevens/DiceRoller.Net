using System.ComponentModel.DataAnnotations;

namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Configuration for the Discord bot connection and basic settings.
    /// </summary>
    public class BotConfiguration
    {
        public const string SectionName = "Bot";

        /// <summary>
        /// Discord bot token. Required for bot authentication.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Command prefix for legacy text commands (if enabled).
        /// </summary>
        public string CommandPrefix { get; set; } = "!roll";

        /// <summary>
        /// Whether to enable slash commands (recommended).
        /// </summary>
        public bool EnableSlashCommands { get; set; } = true;

        /// <summary>
        /// Whether to enable prefix commands (deprecated).
        /// </summary>
        public bool EnablePrefixCommands { get; set; } = false;

        /// <summary>
        /// Whether to enable button interactions.
        /// </summary>
        public bool EnableButtonInteractions { get; set; } = true;

        /// <summary>
        /// Maximum time to wait for Discord API operations (in seconds).
        /// </summary>
        public int DiscordTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to register commands globally or per-guild.
        /// </summary>
        public bool UseGlobalCommands { get; set; } = true;

        /// <summary>
        /// Specific guild ID for testing commands (if UseGlobalCommands is false).
        /// </summary>
        public ulong? TestGuildId { get; set; }
    }
}