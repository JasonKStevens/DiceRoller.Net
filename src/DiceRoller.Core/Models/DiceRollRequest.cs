namespace DiceRoller.Core.Models
{
    /// <summary>
    /// Represents a request to roll dice
    /// </summary>
    public class DiceRollRequest
    {
        /// <summary>
        /// Gets or sets the dice expression to evaluate
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the user ID making the request
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the game system context for the roll
        /// </summary>
        public string GameSystem { get; set; }

        /// <summary>
        /// Gets or sets additional context information
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets whether to include detailed breakdown in the result
        /// </summary>
        public bool IncludeBreakdown { get; set; } = true;
    }
}