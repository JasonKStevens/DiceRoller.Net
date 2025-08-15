using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;

namespace DiceRoller.Plugins.Infrastructure.SDK.Helpers
{
    /// <summary>
    /// Helper class for dice notation extensions and operations
    /// </summary>
    public static class DiceNotationHelper
    {
        /// <summary>
        /// Create a simple dice notation function
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="description">Function description</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="implementation">Function implementation</param>
        /// <returns>Dice notation function</returns>
        public static DiceNotationFunction CreateFunction(
            string name,
            string description,
            IReadOnlyList<DiceNotationParameter> parameters,
            Func<object[], DiceNotationContext, Task<object>> implementation)
        {
            return new DiceNotationFunction(
                name,
                description,
                parameters,
                typeof(object),
                implementation);
        }

        /// <summary>
        /// Create a typed dice notation function
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="name">Function name</param>
        /// <param name="description">Function description</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="implementation">Function implementation</param>
        /// <returns>Dice notation function</returns>
        public static DiceNotationFunction CreateFunction<T>(
            string name,
            string description,
            IReadOnlyList<DiceNotationParameter> parameters,
            Func<object[], DiceNotationContext, Task<T>> implementation)
        {
            return new DiceNotationFunction(
                name,
                description,
                parameters,
                typeof(T),
                async (args, context) => await implementation(args, context));
        }

        /// <summary>
        /// Create a dice notation parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="isRequired">Whether parameter is required</param>
        /// <param name="defaultValue">Default value if optional</param>
        /// <param name="description">Parameter description</param>
        /// <returns>Dice notation parameter</returns>
        public static DiceNotationParameter CreateParameter(
            string name,
            Type type,
            bool isRequired = true,
            object? defaultValue = null,
            string? description = null)
        {
            return new DiceNotationParameter(name, type, isRequired, defaultValue, description);
        }

        /// <summary>
        /// Create a binary operator
        /// </summary>
        /// <param name="symbol">Operator symbol</param>
        /// <param name="name">Operator name</param>
        /// <param name="description">Operator description</param>
        /// <param name="precedence">Operator precedence</param>
        /// <param name="associativity">Operator associativity</param>
        /// <param name="implementation">Operator implementation</param>
        /// <returns>Dice notation operator</returns>
        public static DiceNotationOperator CreateBinaryOperator(
            string symbol,
            string name,
            string description,
            int precedence,
            DiceNotationAssociativity associativity,
            Func<object, object, DiceNotationContext, Task<object>> implementation)
        {
            return new DiceNotationOperator(
                symbol,
                name,
                description,
                DiceNotationOperatorType.Binary,
                precedence,
                associativity,
                implementation);
        }

        /// <summary>
        /// Parse dice notation and extract dice expressions
        /// </summary>
        /// <param name="notation">Dice notation string</param>
        /// <returns>List of dice expressions found</returns>
        public static List<DiceExpression> ParseDiceExpressions(string notation)
        {
            var expressions = new List<DiceExpression>();
            
            // Basic dice pattern: NdS (e.g., 3d6, d20)
            var dicePattern = @"(?<count>\d+)?d(?<sides>\d+)(?<modifier>[!])?";
            var matches = Regex.Matches(notation, dicePattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var countStr = match.Groups["count"].Value;
                var sidesStr = match.Groups["sides"].Value;
                var modifier = match.Groups["modifier"].Value;

                if (int.TryParse(sidesStr, out var sides))
                {
                    var count = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr);
                    var isExploding = modifier == "!";

                    expressions.Add(new DiceExpression(count, sides, isExploding, match.Value));
                }
            }

            return expressions;
        }

        /// <summary>
        /// Validate dice notation syntax
        /// </summary>
        /// <param name="notation">Notation to validate</param>
        /// <returns>Validation result</returns>
        public static DiceNotationValidationResult ValidateNotation(string notation)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(notation))
            {
                errors.Add("Notation cannot be empty");
                return new DiceNotationValidationResult(false, errors);
            }

            try
            {
                // Check for basic syntax issues
                var parenCount = notation.Count(c => c == '(') - notation.Count(c => c == ')');
                if (parenCount != 0)
                {
                    errors.Add("Mismatched parentheses");
                }

                // Check for invalid characters
                var allowedChars = "0123456789+-*/()d! <><=>=,[]{}";
                var invalidChars = notation.Where(c => !allowedChars.Contains(c) && !char.IsWhiteSpace(c) && !char.IsLetter(c)).ToList();
                if (invalidChars.Any())
                {
                    errors.Add($"Invalid characters: {string.Join(", ", invalidChars.Distinct())}");
                }

                // Check dice expressions
                var diceExpressions = ParseDiceExpressions(notation);
                foreach (var dice in diceExpressions)
                {
                    if (dice.Sides <= 0)
                    {
                        errors.Add($"Invalid dice sides: {dice.Sides} (must be positive)");
                    }
                    if (dice.Count <= 0)
                    {
                        errors.Add($"Invalid dice count: {dice.Count} (must be positive)");
                    }
                    if (dice.Count > 1000)
                    {
                        errors.Add($"Too many dice: {dice.Count} (maximum 1000)");
                    }
                    if (dice.Sides > 10000)
                    {
                        errors.Add($"Too many sides: {dice.Sides} (maximum 10000)");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
            }

            return new DiceNotationValidationResult(errors.Count == 0, errors);
        }

        /// <summary>
        /// Format a dice roll result with breakdown
        /// </summary>
        /// <param name="result">Dice roll result</param>
        /// <param name="includeTotal">Whether to include total</param>
        /// <param name="includeBreakdown">Whether to include breakdown</param>
        /// <returns>Formatted result string</returns>
        public static string FormatResult(DiceNotationResult result, bool includeTotal = true, bool includeBreakdown = true)
        {
            if (!result.Success)
            {
                return $"Error: {result.ErrorMessage}";
            }

            var parts = new List<string>();

            if (includeTotal && result.Value != null)
            {
                parts.Add($"**{result.Value}**");
            }

            if (includeBreakdown && !string.IsNullOrEmpty(result.Breakdown))
            {
                parts.Add($"({result.Breakdown})");
            }

            return string.Join(" ", parts);
        }

        /// <summary>
        /// Create a success/failure result for dice notation
        /// </summary>
        /// <param name="value">Result value</param>
        /// <param name="breakdown">Breakdown text</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Success result</returns>
        public static DiceNotationResult CreateSuccessResult(
            object value,
            string breakdown,
            Dictionary<string, object>? additionalData = null)
        {
            return new DiceNotationResult(true, value, breakdown, null, additionalData);
        }

        /// <summary>
        /// Create an error result for dice notation
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Error result</returns>
        public static DiceNotationResult CreateErrorResult(
            string errorMessage,
            Dictionary<string, object>? additionalData = null)
        {
            return new DiceNotationResult(false, null, string.Empty, errorMessage, additionalData);
        }

        /// <summary>
        /// Extract numerical values from arguments
        /// </summary>
        /// <param name="args">Function arguments</param>
        /// <param name="parameterName">Parameter name for error messages</param>
        /// <returns>List of integers</returns>
        public static List<int> ExtractIntegerValues(object[] args, string parameterName = "parameter")
        {
            var values = new List<int>();

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case int intValue:
                        values.Add(intValue);
                        break;
                    case double doubleValue:
                        values.Add((int)Math.Round(doubleValue));
                        break;
                    case string stringValue when int.TryParse(stringValue, out var parsedValue):
                        values.Add(parsedValue);
                        break;
                    default:
                        throw new ArgumentException($"Cannot convert {parameterName} to integer: {arg}");
                }
            }

            return values;
        }

        /// <summary>
        /// Generate random numbers for dice rolls
        /// </summary>
        /// <param name="count">Number of dice</param>
        /// <param name="sides">Number of sides per die</param>
        /// <param name="random">Random number generator</param>
        /// <param name="exploding">Whether dice explode on maximum roll</param>
        /// <returns>List of individual roll results</returns>
        public static List<int> RollDice(int count, int sides, Random random, bool exploding = false)
        {
            var results = new List<int>();

            for (int i = 0; i < count; i++)
            {
                var roll = RollSingleDie(sides, random, exploding);
                results.Add(roll);
            }

            return results;
        }

        /// <summary>
        /// Roll a single die with optional exploding
        /// </summary>
        /// <param name="sides">Number of sides</param>
        /// <param name="random">Random number generator</param>
        /// <param name="exploding">Whether die explodes on maximum roll</param>
        /// <returns>Roll result</returns>
        public static int RollSingleDie(int sides, Random random, bool exploding = false)
        {
            var total = 0;
            int roll;

            do
            {
                roll = random.Next(1, sides + 1);
                total += roll;
            }
            while (exploding && roll == sides);

            return total;
        }

        /// <summary>
        /// Create a breakdown string for multiple dice rolls
        /// </summary>
        /// <param name="rolls">Individual roll results</param>
        /// <param name="modifier">Optional modifier to add</param>
        /// <returns>Breakdown string</returns>
        public static string CreateBreakdown(List<int> rolls, int modifier = 0)
        {
            var breakdown = $"[{string.Join(", ", rolls)}]";
            
            if (modifier != 0)
            {
                var modifierStr = modifier >= 0 ? $" + {modifier}" : $" - {Math.Abs(modifier)}";
                breakdown += modifierStr;
            }

            return breakdown;
        }

        /// <summary>
        /// Check if a notation contains specific patterns
        /// </summary>
        /// <param name="notation">Notation to check</param>
        /// <param name="patterns">Patterns to look for</param>
        /// <returns>True if any pattern is found</returns>
        public static bool ContainsPatterns(string notation, params string[] patterns)
        {
            return patterns.Any(pattern => 
                notation.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Escape special characters in notation for safe processing
        /// </summary>
        /// <param name="notation">Notation to escape</param>
        /// <returns>Escaped notation</returns>
        public static string EscapeNotation(string notation)
        {
            return Regex.Escape(notation);
        }
    }

    /// <summary>
    /// Represents a dice expression
    /// </summary>
    public record DiceExpression(int Count, int Sides, bool IsExploding, string OriginalText);

    /// <summary>
    /// Dice notation validation result
    /// </summary>
    public record DiceNotationValidationResult(bool IsValid, List<string> Errors);
}