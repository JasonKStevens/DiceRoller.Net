using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;

namespace DiceRoller.Plugins.CallOfCthulhu.Tables
{
    /// <summary>
    /// Temporary madness effects table
    /// </summary>
    public class MadnessTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _madnessTable;

        public MadnessTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _madnessTable = InitializeMadnessTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, "Invalid roll value for madness table");
                }

                var result = TableLookupHelper.LookupByRange(_madnessTable, roll, _logger);
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up madness for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializeMadnessTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-05"] = "Fainting - 1d4 hours unconscious",
                ["06-10"] = "Fleeing in panic - 1d4+1 rounds",
                ["11-15"] = "Physical hysterics - 1d4+1 rounds",
                ["16-20"] = "Emotional outburst - 1d4+1 rounds", 
                ["21-25"] = "Memory loss - 1d4 hours amnesia",
                ["26-30"] = "Phobia develops - 1d4+1 hours",
                ["31-35"] = "Mania develops - 1d4+1 hours",
                ["36-40"] = "Hallucinations - 1d4 hours",
                ["41-45"] = "Paranoia - 1d4+1 hours",
                ["46-50"] = "Catatonia - 1d4 hours unresponsive",
                ["51-55"] = "Violence - 1d4+1 rounds attacking",
                ["56-60"] = "Specific phobia - 1d4+1 hours intense fear",
                ["61-65"] = "Echopraxia - 1d4 hours mimicking",
                ["66-70"] = "Echolalia - 1d4 hours repeating",
                ["71-75"] = "Coprophasia - 1d4+1 hours obscenity",
                ["76-80"] = "Megalomania - 1d4+1 hours grandiosity",
                ["81-85"] = "Melancholia - 1d4+1 hours depression",
                ["86-90"] = "Manic episode - 1d4+1 hours hyperactivity",
                ["91-95"] = "Ritual behavior - 1d4 hours compulsions",
                ["96-00"] = "Complete break - 1d4+1 hours insanity"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Permanent madness effects table
    /// </summary>
    public class PermanentMadnessTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _permanentMadnessTable;

        public PermanentMadnessTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _permanentMadnessTable = InitializePermanentMadnessTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, "Invalid roll value for permanent madness table");
                }

                var result = TableLookupHelper.LookupByRange(_permanentMadnessTable, roll, _logger);
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up permanent madness for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializePermanentMadnessTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-10"] = "Amnesia - Memory loss of traumatic period",
                ["11-20"] = "Agoraphobia - Fear of open/crowded spaces",
                ["21-30"] = "Claustrophobia - Fear of enclosed spaces",
                ["31-40"] = "Paranoid schizophrenia - Elaborate conspiracies",
                ["41-50"] = "Multiple personality - Alternate identities",
                ["51-60"] = "Catatonia - Periodic unresponsive states",
                ["61-70"] = "Manic-depression - Mood swings",
                ["71-80"] = "Anxiety disorder - Chronic overwhelming anxiety",
                ["81-90"] = "Conversion disorder - Physical symptoms",
                ["91-00"] = "Complete insanity - Total break from reality"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Phobias table for generating random fears
    /// </summary>
    public class PhobiasTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _phobiasTable;

        public PhobiasTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _phobiasTable = InitializePhobiasTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, "Invalid roll value for phobias table");
                }

                var result = TableLookupHelper.LookupByRange(_phobiasTable, roll, _logger);
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up phobia for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializePhobiasTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-05"] = "Acrophobia - Fear of heights",
                ["06-10"] = "Agoraphobia - Fear of open spaces",
                ["11-15"] = "Arachnophobia - Fear of spiders",
                ["16-20"] = "Claustrophobia - Fear of enclosed spaces",
                ["21-25"] = "Hemophobia - Fear of blood",
                ["26-30"] = "Necrophobia - Fear of the dead",
                ["31-35"] = "Nyctophobia - Fear of darkness",
                ["36-40"] = "Ophidiophobia - Fear of snakes",
                ["41-45"] = "Pyrophobia - Fear of fire",
                ["46-50"] = "Thanatophobia - Fear of death",
                ["51-55"] = "Trypanophobia - Fear of injections",
                ["56-60"] = "Zoophobia - Fear of animals",
                ["61-65"] = "Aquaphobia - Fear of water",
                ["66-70"] = "Entomophobia - Fear of insects",
                ["71-75"] = "Germophobia - Fear of contamination",
                ["76-80"] = "Phonophobia - Fear of loud sounds",
                ["81-85"] = "Spectrophobia - Fear of mirrors",
                ["86-90"] = "Taphophobia - Fear of being buried alive",
                ["91-95"] = "Technophobia - Fear of technology",
                ["96-00"] = "Xenophobia - Fear of strangers"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }

    /// <summary>
    /// Manias table for generating random obsessions
    /// </summary>
    public class ManiasTable
    {
        private readonly ILogger _logger;
        private readonly List<TableRangeEntry> _maniasTable;

        public ManiasTable(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maniasTable = InitializeManiasTable();
        }

        public async Task<GameSystemTableResult> LookupAsync(object key, GameSystemContext context)
        {
            try
            {
                if (!int.TryParse(key.ToString(), out var roll))
                {
                    return new GameSystemTableResult(false, null, string.Empty, "Invalid roll value for manias table");
                }

                var result = TableLookupHelper.LookupByRange(_maniasTable, roll, _logger);
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up mania for roll: {Roll}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        private List<TableRangeEntry> InitializeManiasTable()
        {
            var tableData = new Dictionary<string, string>
            {
                ["01-05"] = "Bibliomania - Obsession with books",
                ["06-10"] = "Kleptomania - Compulsive stealing", 
                ["11-15"] = "Pyromania - Obsession with fire",
                ["16-20"] = "Trichotillomania - Hair pulling compulsion",
                ["21-25"] = "Arithmomania - Obsession with counting",
                ["26-30"] = "Dermatillomania - Skin picking compulsion",
                ["31-35"] = "Dipsomania - Alcohol obsession",
                ["36-40"] = "Egomania - Obsession with self",
                ["41-45"] = "Erotomania - Obsession with love",
                ["46-50"] = "Graphomania - Compulsive writing",
                ["51-55"] = "Hypergraphia - Excessive writing urge",
                ["56-60"] = "Logomania - Obsession with talking",
                ["61-65"] = "Mythomania - Compulsive lying",
                ["66-70"] = "Nymphomania - Sexual obsession",
                ["71-75"] = "Oniomania - Shopping compulsion",
                ["76-80"] = "Phagomania - Obsession with eating",
                ["81-85"] = "Rhinotillexomania - Nose picking compulsion",
                ["86-90"] = "Theomania - Religious obsession",
                ["91-95"] = "Trichomania - Hair obsession",
                ["96-00"] = "Zoomania - Animal obsession"
            };

            return TableLookupHelper.CreateRangeTable(tableData);
        }
    }
}