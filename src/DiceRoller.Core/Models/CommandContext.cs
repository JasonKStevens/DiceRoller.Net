using System.Collections.Generic;

namespace DiceRoller.Core.Models
{
    /// <summary>
    /// Represents the context for command execution
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// Gets or sets the full command text
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the user ID executing the command
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the channel or context where the command was executed
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the game system context
        /// </summary>
        public string GameSystem { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the command
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the platform where the command was executed (Discord, Web, etc.)
        /// </summary>
        public string Platform { get; set; }
    }
}