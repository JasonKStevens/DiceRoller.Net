using DiceRoller.Core.Parser;
using System.Text;
using System.Text.RegularExpressions;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Core detail preservation logic for maintaining mathematical context in formatted results
/// </summary>
public class DetailPreservingFormatter
{
    private readonly BreakdownTextParser _textParser;

    public DetailPreservingFormatter(BreakdownTextParser textParser)
    {
        _textParser = textParser ?? throw new ArgumentNullException(nameof(textParser));
    }

    /// <summary>
    /// Preserves calculation details for repeat iterations
    /// </summary>
    /// <param name="iterationValue">The result value of the iteration</param>
    /// <param name="fullBreakdown">The complete breakdown text to search in</param>
    /// <returns>Detailed breakdown showing how the value was calculated</returns>
    public string PreserveIterationDetails(string iterationValue, string fullBreakdown)
    {
        if (string.IsNullOrEmpty(iterationValue) || string.IsNullOrEmpty(fullBreakdown))
            return iterationValue;

        // Look for detailed calculation patterns that result in this value
        var detailPatterns = new[]
        {
            // Exploding dice: "6! → 6+4 + 4 + 3 + 2 = 17"
            $@"([^=]*!?[^=]*)\s*=\s*{Regex.Escape(iterationValue)}(?:\s|$)",
            
            // Complex expressions: "(3d6+2) = 17"
            $@"\(([^)]+)\)\s*=\s*{Regex.Escape(iterationValue)}(?:\s|$)",
            
            // Function calls: "min(19, 20) = 17"
            $@"(min|max|step)\([^)]+\)\s*=\s*{Regex.Escape(iterationValue)}(?:\s|$)",
            
            // Simple arithmetic: "5+4+3+2+3 = 17"
            $@"([0-9+\-*\/\s!]+)\s*=\s*{Regex.Escape(iterationValue)}(?:\s|$)"
        };

        foreach (var pattern in detailPatterns)
        {
            var match = Regex.Match(fullBreakdown, pattern);
            if (match.Success)
            {
                var calculation = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(calculation) && calculation != iterationValue)
                {
                    return $"{iterationValue} ({calculation})";
                }
            }
        }

        return iterationValue;
    }

    /// <summary>
    /// Extracts exploding dice details with proper mathematical notation
    /// </summary>
    /// <param name="diceExpression">The dice expression (e.g., "3d6!")</param>
    /// <param name="fullBreakdown">The complete breakdown text</param>
    /// <returns>Formatted exploding dice breakdown</returns>
    public string PreserveExplodingDiceDetails(string diceExpression, string fullBreakdown)
    {
        if (string.IsNullOrEmpty(diceExpression) || !diceExpression.Contains('!'))
            return diceExpression;

        // Look for exploding dice patterns in breakdown
        var explodingPattern = $@"{Regex.Escape(diceExpression)}[^=]*=\s*\[([^\]]+)\]";
        var match = Regex.Match(fullBreakdown, explodingPattern);

        if (match.Success)
        {
            var diceResults = match.Groups[1].Value;
            
            // Parse and format the exploding dice results
            var formattedResults = FormatExplodingDiceResults(diceResults);
            return $"{diceExpression} = [{formattedResults}]";
        }

        return diceExpression;
    }

    /// <summary>
    /// Formats exploding dice results to show explosion chains
    /// </summary>
    /// <param name="diceResults">Raw dice results from breakdown</param>
    /// <returns>Formatted results showing explosion chains</returns>
    private string FormatExplodingDiceResults(string diceResults)
    {
        if (string.IsNullOrEmpty(diceResults))
            return diceResults;

        // Split results and identify exploding chains
        var results = diceResults.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        var formatted = new List<string>();

        foreach (var result in results)
        {
            if (result.Contains('!'))
            {
                // This is an exploding die - show the explosion chain
                var explosionChain = FormatExplosionChain(result);
                formatted.Add(explosionChain);
            }
            else
            {
                formatted.Add(result);
            }
        }

        return string.Join(", ", formatted);
    }

    /// <summary>
    /// Formats a single exploding die result to show the explosion chain
    /// </summary>
    /// <param name="explodingResult">Single exploding die result (e.g., "6!")</param>
    /// <returns>Formatted explosion chain</returns>
    private string FormatExplosionChain(string explodingResult)
    {
        // For now, return as-is. This could be enhanced to show full explosion chains
        // like "6! → 6+4" if that information is available in the breakdown
        return explodingResult;
    }

    /// <summary>
    /// Preserves function call details with arguments and results
    /// </summary>
    /// <param name="functionName">Name of the function (min, max, etc.)</param>
    /// <param name="result">The result value</param>
    /// <param name="fullBreakdown">The complete breakdown text</param>
    /// <returns>Formatted function call with preserved details</returns>
    public string PreserveFunctionDetails(string functionName, string result, string fullBreakdown)
    {
        if (string.IsNullOrEmpty(functionName) || string.IsNullOrEmpty(result))
            return result;

        // Look for function call patterns
        var functionPattern = $@"{functionName}\(([^)]+)\)\s*=\s*{Regex.Escape(result)}";
        var match = Regex.Match(fullBreakdown, functionPattern);

        if (match.Success)
        {
            var arguments = match.Groups[1].Value.Trim();
            return $"{functionName}({arguments}) = {result}";
        }

        return result;
    }

    /// <summary>
    /// Determines if the result is too long and needs truncation
    /// </summary>
    /// <param name="result">The formatted result</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <returns>True if truncation is needed</returns>
    public bool NeedsTruncation(string result, int maxLength = 1800)
    {
        return !string.IsNullOrEmpty(result) && result.Length > maxLength;
    }

    /// <summary>
    /// Creates a smart truncation with "Show More" indication
    /// </summary>
    /// <param name="result">The full result text</param>
    /// <param name="maxLength">Maximum allowed length before truncation</param>
    /// <returns>Truncated result with indication</returns>
    public string CreateSmartTruncation(string result, int maxLength = 1800)
    {
        if (string.IsNullOrEmpty(result) || result.Length <= maxLength)
            return result;

        // Find a good truncation point (end of line, after complete expressions)
        var truncateAt = FindGoodTruncationPoint(result, maxLength);
        
        var truncated = result.Substring(0, truncateAt).TrimEnd();
        var remainingLines = CountRemainingLines(result, truncateAt);
        
        return $"{truncated}\n\n*... and {remainingLines} more lines (result too long for Discord)*";
    }

    /// <summary>
    /// Finds a good point to truncate at (end of line, complete expression)
    /// </summary>
    /// <param name="text">The text to truncate</param>
    /// <param name="maxLength">Maximum length</param>
    /// <returns>Index to truncate at</returns>
    private int FindGoodTruncationPoint(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text.Length;

        // Look for line breaks near the max length
        var searchStart = Math.Max(0, maxLength - 200); // Look back up to 200 chars
        var lastNewline = text.LastIndexOf('\n', maxLength, maxLength - searchStart);
        
        if (lastNewline > searchStart)
            return lastNewline;

        // If no good line break, truncate at max length
        return maxLength;
    }

    /// <summary>
    /// Counts remaining lines after truncation point
    /// </summary>
    /// <param name="text">The full text</param>
    /// <param name="truncateAt">The truncation point</param>
    /// <returns>Number of remaining lines</returns>
    private int CountRemainingLines(string text, int truncateAt)
    {
        if (truncateAt >= text.Length)
            return 0;

        var remainingText = text.Substring(truncateAt);
        return remainingText.Count(c => c == '\n');
    }
}