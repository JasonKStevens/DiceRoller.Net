using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.SDK.Helpers
{
    /// <summary>
    /// Helper class for table lookup operations in game system plugins
    /// </summary>
    public static class TableLookupHelper
    {
        /// <summary>
        /// Perform a range-based table lookup (e.g., d100 tables)
        /// </summary>
        /// <param name="table">Table entries with ranges</param>
        /// <param name="roll">Dice roll result</param>
        /// <param name="logger">Optional logger</param>
        /// <returns>Lookup result</returns>
        public static GameSystemTableResult LookupByRange(
            IEnumerable<TableRangeEntry> table,
            int roll,
            ILogger? logger = null)
        {
            try
            {
                var entry = table.FirstOrDefault(e => roll >= e.MinValue && roll <= e.MaxValue);
                
                if (entry != null)
                {
                    logger?.LogDebug("Table lookup successful: {Roll} -> {Result}", roll, entry.Result);
                    return new GameSystemTableResult(true, entry.Result, entry.Description);
                }

                logger?.LogWarning("No table entry found for roll: {Roll}", roll);
                return new GameSystemTableResult(false, null, string.Empty, $"No entry found for roll {roll}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during range table lookup for roll {Roll}", roll);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Perform an exact value table lookup
        /// </summary>
        /// <param name="table">Table entries with exact values</param>
        /// <param name="key">Lookup key</param>
        /// <param name="logger">Optional logger</param>
        /// <returns>Lookup result</returns>
        public static GameSystemTableResult LookupByExactValue<TKey>(
            IEnumerable<TableExactEntry<TKey>> table,
            TKey key,
            ILogger? logger = null) where TKey : IEquatable<TKey>
        {
            try
            {
                var entry = table.FirstOrDefault(e => e.Key.Equals(key));
                
                if (entry != null)
                {
                    logger?.LogDebug("Table lookup successful: {Key} -> {Result}", key, entry.Result);
                    return new GameSystemTableResult(true, entry.Result, entry.Description);
                }

                logger?.LogWarning("No table entry found for key: {Key}", key);
                return new GameSystemTableResult(false, null, string.Empty, $"No entry found for key {key}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during exact value table lookup for key {Key}", key);
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Perform a weighted random table lookup
        /// </summary>
        /// <param name="table">Table entries with weights</param>
        /// <param name="random">Random number generator</param>
        /// <param name="logger">Optional logger</param>
        /// <returns>Lookup result</returns>
        public static GameSystemTableResult LookupByWeight(
            IEnumerable<TableWeightedEntry> table,
            Random random,
            ILogger? logger = null)
        {
            try
            {
                var entries = table.ToList();
                var totalWeight = entries.Sum(e => e.Weight);
                
                if (totalWeight <= 0)
                {
                    return new GameSystemTableResult(false, null, string.Empty, "Total weight must be greater than 0");
                }

                var randomValue = random.NextDouble() * totalWeight;
                var currentWeight = 0.0;

                foreach (var entry in entries)
                {
                    currentWeight += entry.Weight;
                    if (randomValue <= currentWeight)
                    {
                        logger?.LogDebug("Weighted table lookup successful: {RandomValue}/{TotalWeight} -> {Result}", 
                            randomValue, totalWeight, entry.Result);
                        return new GameSystemTableResult(true, entry.Result, entry.Description);
                    }
                }

                // Fallback to last entry
                var lastEntry = entries.Last();
                logger?.LogDebug("Weighted table lookup fallback: -> {Result}", lastEntry.Result);
                return new GameSystemTableResult(true, lastEntry.Result, lastEntry.Description);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during weighted table lookup");
                return new GameSystemTableResult(false, null, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Create a range-based table from a dictionary with string keys like "01-05", "06-10"
        /// </summary>
        /// <param name="tableData">Dictionary with range keys and result values</param>
        /// <returns>List of table range entries</returns>
        public static List<TableRangeEntry> CreateRangeTable(Dictionary<string, string> tableData)
        {
            var entries = new List<TableRangeEntry>();

            foreach (var (rangeKey, result) in tableData)
            {
                try
                {
                    var (minValue, maxValue) = ParseRange(rangeKey);
                    entries.Add(new TableRangeEntry(minValue, maxValue, result, result));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid range format: {rangeKey}", nameof(tableData), ex);
                }
            }

            return entries.OrderBy(e => e.MinValue).ToList();
        }

        /// <summary>
        /// Create a weighted table from a dictionary with result keys and weight values
        /// </summary>
        /// <param name="tableData">Dictionary with results and their weights</param>
        /// <returns>List of table weighted entries</returns>
        public static List<TableWeightedEntry> CreateWeightedTable(Dictionary<string, double> tableData)
        {
            return tableData.Select(kvp => new TableWeightedEntry(kvp.Value, kvp.Key, kvp.Key)).ToList();
        }

        /// <summary>
        /// Validate a range-based table for completeness and overlaps
        /// </summary>
        /// <param name="table">Table to validate</param>
        /// <param name="expectedMin">Expected minimum value</param>
        /// <param name="expectedMax">Expected maximum value</param>
        /// <returns>Validation result</returns>
        public static TableValidationResult ValidateRangeTable(
            IEnumerable<TableRangeEntry> table,
            int expectedMin,
            int expectedMax)
        {
            var entries = table.OrderBy(e => e.MinValue).ToList();
            var errors = new List<string>();

            // Check for gaps and overlaps
            for (int i = 0; i < entries.Count; i++)
            {
                var current = entries[i];

                // Check if range is valid
                if (current.MinValue > current.MaxValue)
                {
                    errors.Add($"Invalid range: {current.MinValue}-{current.MaxValue}");
                }

                // Check for overlaps with next entry
                if (i < entries.Count - 1)
                {
                    var next = entries[i + 1];
                    if (current.MaxValue >= next.MinValue)
                    {
                        errors.Add($"Overlap between ranges: {current.MinValue}-{current.MaxValue} and {next.MinValue}-{next.MaxValue}");
                    }
                    
                    // Check for gaps
                    if (current.MaxValue + 1 < next.MinValue)
                    {
                        errors.Add($"Gap between ranges: {current.MaxValue + 1}-{next.MinValue - 1}");
                    }
                }
            }

            // Check coverage
            if (entries.Any())
            {
                var firstEntry = entries.First();
                var lastEntry = entries.Last();

                if (firstEntry.MinValue > expectedMin)
                {
                    errors.Add($"Table does not cover minimum expected value: {expectedMin} (starts at {firstEntry.MinValue})");
                }

                if (lastEntry.MaxValue < expectedMax)
                {
                    errors.Add($"Table does not cover maximum expected value: {expectedMax} (ends at {lastEntry.MaxValue})");
                }
            }

            return new TableValidationResult(errors.Count == 0, errors);
        }

        /// <summary>
        /// Parse a range string like "01-05" or "15" into min and max values
        /// </summary>
        /// <param name="rangeString">Range string to parse</param>
        /// <returns>Tuple of min and max values</returns>
        public static (int min, int max) ParseRange(string rangeString)
        {
            if (string.IsNullOrWhiteSpace(rangeString))
                throw new ArgumentException("Range string cannot be null or empty", nameof(rangeString));

            var trimmed = rangeString.Trim();

            if (trimmed.Contains('-'))
            {
                var parts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ArgumentException($"Invalid range format: {rangeString}");

                if (!int.TryParse(parts[0], out var min) || !int.TryParse(parts[1], out var max))
                    throw new ArgumentException($"Invalid range format: {rangeString}");

                return (min, max);
            }
            else
            {
                if (!int.TryParse(trimmed, out var value))
                    throw new ArgumentException($"Invalid range format: {rangeString}");

                return (value, value);
            }
        }

        /// <summary>
        /// Format a lookup result for display
        /// </summary>
        /// <param name="result">Lookup result</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="key">Lookup key used</param>
        /// <returns>Formatted result string</returns>
        public static string FormatResult(GameSystemTableResult result, string tableName, object key)
        {
            if (!result.Success)
            {
                return $"**{tableName}** lookup failed for {key}: {result.ErrorMessage}";
            }

            return string.IsNullOrEmpty(result.Description) 
                ? $"**{tableName}** ({key}): {result.Value}"
                : $"**{tableName}** ({key}): {result.Description}";
        }
    }

    /// <summary>
    /// Table entry with a range of values
    /// </summary>
    public record TableRangeEntry(int MinValue, int MaxValue, object Result, string Description);

    /// <summary>
    /// Table entry with an exact key value
    /// </summary>
    public record TableExactEntry<TKey>(TKey Key, object Result, string Description) where TKey : IEquatable<TKey>;

    /// <summary>
    /// Table entry with a weight for random selection
    /// </summary>
    public record TableWeightedEntry(double Weight, object Result, string Description);

    /// <summary>
    /// Table validation result
    /// </summary>
    public record TableValidationResult(bool IsValid, List<string> Errors);
}