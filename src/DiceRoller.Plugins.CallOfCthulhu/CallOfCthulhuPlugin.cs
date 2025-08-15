using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;
using DiceRoller.Plugins.CallOfCthulhu.GameSystem;
using DiceRoller.Plugins.CallOfCthulhu.Commands;
using DiceRoller.Plugins.CallOfCthulhu.Tables;

namespace DiceRoller.Plugins.CallOfCthulhu
{
    /// <summary>
    /// Call of Cthulhu game system plugin providing sanity, luck, and madness mechanics
    /// </summary>
    [Plugin("callofcthulhu", "Call of Cthulhu Game System", "1.0.0",
        Description = "Provides Call of Cthulhu RPG game system support including sanity checks, luck rolls, and madness tables.",
        Author = "DiceRoller.Net Team",
        Tags = new[] { "rpg", "cthulhu", "horror", "chaosium" })]
    [GameSystemPlugin("Call of Cthulhu", GameSystemVersion = "7th Edition", Publisher = "Chaosium Inc.")]
    public class CallOfCthulhuPlugin : PluginBase, IGameSystemPlugin
    {
        private CallOfCthulhuGameSystem? _gameSystem;
        private readonly List<GameSystemCommand> _commands = new();
        private readonly List<GameSystemTable> _tables = new();

        public override string Id => "callofcthulhu";
        public override string Name => "Call of Cthulhu Game System";
        public override Version Version => new(1, 0, 0);
        public override string Description => "Provides Call of Cthulhu RPG game system support including sanity checks, luck rolls, and madness tables.";
        public override string Author => "DiceRoller.Net Team";

        public string GameSystemName => "Call of Cthulhu";
        public string GameSystemVersion => "7th Edition";
        public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();
        public IReadOnlyList<GameSystemTable> Tables => _tables.AsReadOnly();

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Initializing Call of Cthulhu game system plugin");

            try
            {
                // Initialize game system
                _gameSystem = new CallOfCthulhuGameSystem(Logger);

                // Register commands
                RegisterCommands();

                // Register tables
                RegisterTables();

                Logger.LogInformation("Call of Cthulhu plugin initialized with {CommandCount} commands and {TableCount} tables", 
                    _commands.Count, _tables.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize Call of Cthulhu plugin");
                throw;
            }
        }

        public async Task<GameSystemCommandResult> ExecuteCommandAsync(string commandName, 
            Dictionary<string, object> parameters, 
            GameSystemContext context)
        {
            if (_gameSystem == null)
                throw new InvalidOperationException("Game system not initialized");

            try
            {
                Logger.LogDebug("Executing Call of Cthulhu command: {CommandName}", commandName);

                return commandName.ToLowerInvariant() switch
                {
                    "sanity" => await _gameSystem.SanityCommand.ExecuteAsync(parameters, context),
                    "luck" => await _gameSystem.LuckCommand.ExecuteAsync(parameters, context),
                    "madness" => await _gameSystem.MadnessCommand.ExecuteAsync(parameters, context),
                    "fear" => await _gameSystem.FearCommand.ExecuteAsync(parameters, context),
                    _ => new GameSystemCommandResult(false, null, string.Empty, $"Unknown command: {commandName}")
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing Call of Cthulhu command: {CommandName}", commandName);
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        public async Task<GameSystemTableResult> LookupTableAsync(string tableName, 
            object key, 
            GameSystemContext context)
        {
            if (_gameSystem == null)
                throw new InvalidOperationException("Game system not initialized");

            try
            {
                Logger.LogDebug("Looking up Call of Cthulhu table: {TableName} with key: {Key}", tableName, key);

                return tableName.ToLowerInvariant() switch
                {
                    "sanityloss" or "sanity_loss" => await _gameSystem.SanityLossTable.LookupAsync(key, context),
                    "madness" or "temporary_madness" => await _gameSystem.MadnessTable.LookupAsync(key, context),
                    "permanentmadness" or "permanent_madness" => await _gameSystem.PermanentMadnessTable.LookupAsync(key, context),
                    "phobias" => await _gameSystem.PhobiasTable.LookupAsync(key, context),
                    "manias" => await _gameSystem.ManiasTable.LookupAsync(key, context),
                    _ => new GameSystemTableResult(false, null, string.Empty, $"Unknown table: {tableName}")
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error looking up Call of Cthulhu table: {TableName}", tableName);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        public GameSystemHelpInfo GetHelp(string? commandName = null)
        {
            var commandHelps = new List<CommandHelpInfo>();
            var tableHelps = new List<TableHelpInfo>();

            if (commandName == null)
            {
                // Return help for all commands
                commandHelps.AddRange(new[]
                {
                    new CommandHelpInfo(
                        "sanity",
                        "Perform a sanity check with optional loss calculation",
                        "sanity <current_sanity> [sanity_loss] [--roll_for_loss]",
                        "sanity 65 - Roll sanity check\nsanity 65 5 - Apply 5 sanity loss\nsanity 65 1d10 --roll_for_loss - Roll for sanity loss",
                        new List<ParameterHelpInfo>
                        {
                            new("current_sanity", "int", true, "Current sanity value", null),
                            new("sanity_loss", "string", false, "Sanity loss amount or dice expression", "0"),
                            new("roll_for_loss", "bool", false, "Whether to roll for sanity loss", "false")
                        }),
                    new CommandHelpInfo(
                        "luck",
                        "Perform a luck check",
                        "luck <current_luck> [difficulty]",
                        "luck 50 - Standard luck check\nluck 50 hard - Hard luck check",
                        new List<ParameterHelpInfo>
                        {
                            new("current_luck", "int", true, "Current luck value", null),
                            new("difficulty", "string", false, "Difficulty level (normal, hard, extreme)", "normal")
                        }),
                    new CommandHelpInfo(
                        "madness",
                        "Roll on the madness table",
                        "madness [type]",
                        "madness - Temporary madness\nmadness permanent - Permanent madness",
                        new List<ParameterHelpInfo>
                        {
                            new("type", "string", false, "Madness type (temporary, permanent)", "temporary")
                        })
                });

                tableHelps.AddRange(new[]
                {
                    new TableHelpInfo(
                        "sanityloss",
                        "Sanity loss table for various threats",
                        "sanityloss <threat_level>",
                        "sanityloss minor - Minor threat sanity loss"),
                    new TableHelpInfo(
                        "madness",
                        "Temporary madness effects table",
                        "madness <d100_roll>",
                        "madness 45 - Look up madness effect"),
                    new TableHelpInfo(
                        "phobias",
                        "Random phobias table",
                        "phobias <d100_roll>",
                        "phobias 23 - Generate random phobia")
                });
            }
            else
            {
                // Return help for specific command
                var specificCommand = commandHelps.Find(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
                if (specificCommand != null)
                {
                    commandHelps.Add(specificCommand);
                }
            }

            return new GameSystemHelpInfo(
                GameSystemName,
                "Call of Cthulhu 7th Edition game system providing sanity mechanics, luck checks, and madness effects.",
                commandHelps,
                tableHelps);
        }

        private void RegisterCommands()
        {
            _commands.Add(new GameSystemCommand(
                "sanity",
                "Perform a sanity check with optional loss calculation",
                new List<GameSystemParameter>
                {
                    new("current_sanity", typeof(int), true),
                    new("sanity_loss", typeof(string), false, "0"),
                    new("roll_for_loss", typeof(bool), false, false)
                },
                "Sanity checks determine if an investigator loses sanity when confronting the unknown.",
                true));

            _commands.Add(new GameSystemCommand(
                "luck",
                "Perform a luck check",
                new List<GameSystemParameter>
                {
                    new("current_luck", typeof(int), true),
                    new("difficulty", typeof(string), false, "normal")
                },
                "Luck checks can help investigators in desperate situations.",
                true));

            _commands.Add(new GameSystemCommand(
                "madness",
                "Roll on the madness table",
                new List<GameSystemParameter>
                {
                    new("type", typeof(string), false, "temporary")
                },
                "Madness effects when sanity loss is severe.",
                true));
        }

        private void RegisterTables()
        {
            _tables.Add(new GameSystemTable(
                "sanityloss",
                "Sanity loss amounts for various threats",
                typeof(string),
                typeof(string),
                "Provides sanity loss amounts based on threat severity."));

            _tables.Add(new GameSystemTable(
                "madness",
                "Temporary madness effects",
                typeof(int),
                typeof(string),
                "Random temporary madness effects for severe sanity loss."));

            _tables.Add(new GameSystemTable(
                "permanentmadness",
                "Permanent madness effects",
                typeof(int),
                typeof(string),
                "Random permanent madness effects for extreme sanity loss."));

            _tables.Add(new GameSystemTable(
                "phobias",
                "Random phobias table",
                typeof(int),
                typeof(string),
                "Generate random phobias for investigators."));

            _tables.Add(new GameSystemTable(
                "manias",
                "Random manias table",
                typeof(int),
                typeof(string),
                "Generate random manias for investigators."));
        }

        protected override PluginHealthStatus? OnGetHealthStatus()
        {
            var isHealthy = _gameSystem != null && Status == PluginStatus.Running;
            
            return new PluginHealthStatus(
                isHealthy,
                isHealthy ? "Running" : "Not initialized",
                null,
                new Dictionary<string, object>
                {
                    ["gameSystemLoaded"] = _gameSystem != null,
                    ["commandCount"] = _commands.Count,
                    ["tableCount"] = _tables.Count
                });
        }

        protected override void OnDispose()
        {
            _gameSystem = null;
            _commands.Clear();
            _tables.Clear();
        }
    }
}