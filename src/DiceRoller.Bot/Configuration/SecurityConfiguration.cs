using System.ComponentModel.DataAnnotations;

namespace DiceRoller.Bot.Configuration
{
    /// <summary>
    /// Configuration for security and rate limiting.
    /// </summary>
    public class SecurityConfiguration
    {
        public const string SectionName = "Security";

        /// <summary>
        /// Maximum length of Discord messages before truncation.
        /// </summary>
        [Range(100, 2000)]
        public int MaxMessageLength { get; set; } = 2000;

        /// <summary>
        /// Whether to enable rate limiting.
        /// </summary>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// Maximum number of commands per user per minute.
        /// </summary>
        [Range(1, 1000)]
        public int CommandsPerMinute { get; set; } = 60;

        /// <summary>
        /// Maximum complexity level allowed for dice expressions.
        /// </summary>
        [Range(1, 5)]
        public int MaxExpressionComplexity { get; set; } = 5;

        /// <summary>
        /// Maximum number of dice in a single expression.
        /// </summary>
        [Range(1, 10000)]
        public int MaxDiceCount { get; set; } = 1000;

        /// <summary>
        /// Maximum number of sides on a die.
        /// </summary>
        [Range(2, 1000000)]
        public int MaxDieSides { get; set; } = 100000;

        /// <summary>
        /// Whether to log security violations.
        /// </summary>
        public bool LogSecurityViolations { get; set; } = true;

        /// <summary>
        /// Whether to automatically ban users who repeatedly violate limits.
        /// </summary>
        public bool AutoBanViolators { get; set; } = false;

        /// <summary>
        /// Number of violations before auto-ban (if enabled).
        /// </summary>
        public int ViolationsBeforeBan { get; set; } = 10;
    }
}