using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.CallOfCthulhu.Commands;
using DiceRoller.Plugins.CallOfCthulhu.Tables;

namespace DiceRoller.Plugins.CallOfCthulhu.GameSystem
{
    /// <summary>
    /// Call of Cthulhu game system implementation containing all commands and tables
    /// </summary>
    public class CallOfCthulhuGameSystem
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Sanity check command
        /// </summary>
        public SanityCommand SanityCommand { get; }

        /// <summary>
        /// Luck check command
        /// </summary>
        public LuckCommand LuckCommand { get; }

        /// <summary>
        /// Madness effect command
        /// </summary>
        public MadnessCommand MadnessCommand { get; }

        /// <summary>
        /// Fear check command
        /// </summary>
        public FearCommand FearCommand { get; }

        /// <summary>
        /// Sanity loss lookup table
        /// </summary>
        public SanityLossTable SanityLossTable { get; }

        /// <summary>
        /// Temporary madness table
        /// </summary>
        public MadnessTable MadnessTable { get; }

        /// <summary>
        /// Permanent madness table
        /// </summary>
        public PermanentMadnessTable PermanentMadnessTable { get; }

        /// <summary>
        /// Phobias table
        /// </summary>
        public PhobiasTable PhobiasTable { get; }

        /// <summary>
        /// Manias table
        /// </summary>
        public ManiasTable ManiasTable { get; }

        public CallOfCthulhuGameSystem(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize commands
            SanityCommand = new SanityCommand(_logger);
            LuckCommand = new LuckCommand(_logger);
            MadnessCommand = new MadnessCommand(_logger);
            FearCommand = new FearCommand(_logger);

            // Initialize tables
            SanityLossTable = new SanityLossTable(_logger);
            MadnessTable = new MadnessTable(_logger);
            PermanentMadnessTable = new PermanentMadnessTable(_logger);
            PhobiasTable = new PhobiasTable(_logger);
            ManiasTable = new ManiasTable(_logger);

            _logger.LogDebug("Call of Cthulhu game system initialized");
        }
    }
}