using System.Text.RegularExpressions;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Handles parsing of breakdown text using regex patterns
/// </summary>
public class BreakdownTextParser
{
    /// <summary>
    /// Information extracted from repeat function breakdown
    /// </summary>
    public class RepeatInfo
    {
        public bool IsValid { get; set; }
        public string Expression { get; set; } = "";
        public int Count { get; set; }
        public List<string> Values { get; set; } = new();
    }

    /// <summary>
    /// Information extracted from min function breakdown
    /// </summary>
    public class MinInfo
    {
        public bool IsValid { get; set; }
        public string Expression1 { get; set; } = "";
        public string Expression2 { get; set; } = "";
        public string Result { get; set; } = "";
        public string DetailBreakdown { get; set; } = "";
    }

    /// <summary>
    /// Information extracted from comparison breakdown
    /// </summary>
    public class ComparisonInfo
    {
        public bool IsValid { get; set; }
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";
    }

    /// <summary>
    /// Extracts repeat function information from breakdown text
    /// </summary>
    /// <param name="breakdown">The breakdown text to parse</param>
    /// <returns>RepeatInfo containing parsed data</returns>
    public RepeatInfo ExtractRepeatInfo(string breakdown)
    {
        var repeatMatch = Regex.Match(breakdown, @"repeat\(([^,]+),\s*(\d+)\):\s*\[([^\]]+)\]");
        if (repeatMatch.Success)
        {
            var values = repeatMatch.Groups[3].Value
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return new RepeatInfo
            {
                IsValid = true,
                Expression = repeatMatch.Groups[1].Value.Trim(),
                Count = int.Parse(repeatMatch.Groups[2].Value),
                Values = values
            };
        }

        return new RepeatInfo { IsValid = false };
    }

    /// <summary>
    /// Extracts min function information from breakdown text
    /// </summary>
    /// <param name="breakdown">The breakdown text to parse</param>
    /// <returns>MinInfo containing parsed data</returns>
    public MinInfo ExtractMinInfo(string breakdown)
    {
        var minMatch = Regex.Match(breakdown, @"min\(([^,]+),\s*([^)]+)\)\s*=\s*(\d+)");
        if (minMatch.Success)
        {
            return new MinInfo
            {
                IsValid = true,
                Expression1 = minMatch.Groups[1].Value.Trim(),
                Expression2 = minMatch.Groups[2].Value.Trim(),
                Result = minMatch.Groups[3].Value.Trim()
            };
        }

        return new MinInfo { IsValid = false };
    }

    /// <summary>
    /// Extracts comparison information from breakdown text
    /// </summary>
    /// <param name="breakdown">The breakdown text to parse</param>
    /// <returns>ComparisonInfo containing parsed data</returns>
    public ComparisonInfo ExtractComparisonInfo(string breakdown)
    {
        var compMatch = Regex.Match(breakdown, @"\(([^)]+)\)\s*=\s*(True|False)");
        if (compMatch.Success)
        {
            return new ComparisonInfo
            {
                IsValid = true,
                Expression = compMatch.Groups[1].Value.Trim(),
                Result = compMatch.Groups[2].Value.Trim()
            };
        }

        return new ComparisonInfo { IsValid = false };
    }

    /// <summary>
    /// Extracts dice roll information with exploding dice details
    /// </summary>
    /// <param name="nodeText">The dice roll text</param>
    /// <param name="fullBreakdown">The full breakdown to search in</param>
    /// <returns>Formatted dice roll information</returns>
    public string ExtractDiceRollInfo(string nodeText, string fullBreakdown)
    {
        if (string.IsNullOrEmpty(nodeText))
            return "";

        // Look for exploding dice patterns
        if (nodeText.Contains('!'))
        {
            var explodingMatch = Regex.Match(fullBreakdown, $@"{Regex.Escape(nodeText)}[^=]*=\s*\[([^\]]+)\]");
            if (explodingMatch.Success)
            {
                return $"{nodeText} = [{explodingMatch.Groups[1].Value}]";
            }
        }

        return nodeText;
    }

    /// <summary>
    /// Extracts detailed breakdown for a specific iteration value
    /// </summary>
    /// <param name="value">The value to find details for</param>
    /// <param name="fullBreakdown">The full breakdown to search in</param>
    /// <returns>Detailed breakdown text</returns>
    public string ExtractIterationDetail(string value, string fullBreakdown)
    {
        // Look for detailed breakdown that matches this value
        var detailMatch = Regex.Match(fullBreakdown, $@"([^=]+)=\s*{Regex.Escape(value)}\s");
        if (detailMatch.Success)
        {
            return detailMatch.Groups[1].Value.Trim();
        }

        return "";
    }

    /// <summary>
    /// Extracts expression name from the original expression
    /// </summary>
    /// <param name="originalExpression">The original dice expression</param>
    /// <returns>The extracted expression name</returns>
    public string ExtractExpressionName(string originalExpression)
    {
        if (string.IsNullOrEmpty(originalExpression))
            return "Dice Expression";

        var expression = originalExpression.Split('#')[0].Trim();
        return string.IsNullOrEmpty(expression) ? "Dice Expression" : expression;
    }

    /// <summary>
    /// Extracts iterations count from repeat breakdown
    /// </summary>
    /// <param name="fullBreakdown">The full breakdown text</param>
    /// <returns>The iterations count as a string</returns>
    public string ExtractIterationsCount(string fullBreakdown)
    {
        var iterationsMatch = Regex.Match(fullBreakdown, @"repeat\((\d+) iterations\)");
        return iterationsMatch.Success ? iterationsMatch.Groups[1].Value : "unknown";
    }
}