using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Manages the selection and application of appropriate formatters based on expression complexity
/// </summary>
public class AdaptiveFormatterManager
{
    private readonly IComplexityAnalyzer _complexityAnalyzer;
    private readonly IAdaptiveBreakdownFormatter _enhancedFormatter;

    public AdaptiveFormatterManager(
        IComplexityAnalyzer complexityAnalyzer,
        IAdaptiveBreakdownFormatter enhancedFormatter)
    {
        _complexityAnalyzer = complexityAnalyzer ?? throw new ArgumentNullException(nameof(complexityAnalyzer));
        _enhancedFormatter = enhancedFormatter ?? throw new ArgumentNullException(nameof(enhancedFormatter));
    }

    /// <summary>
    /// Formats a breakdown using the appropriate formatter based on complexity
    /// </summary>
    /// <param name="typedResult">The TypedResult structure to analyze and format</param>
    /// <param name="breakdown">The raw breakdown text</param>
    /// <param name="originalExpression">The original dice expression</param>
    /// <returns>The formatted breakdown text</returns>
    public string FormatBreakdown(TypedResult typedResult, string breakdown, string originalExpression)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
        {
            return FormatFallbackBreakdown(breakdown);
        }

        var complexity = _complexityAnalyzer.AnalyzeComplexity(typedResult);
        
        return complexity switch
        {
            ComplexityLevel.Simple => FormatSimpleBreakdown(breakdown),
            ComplexityLevel.Moderate => FormatModerateBreakdown(breakdown),
            ComplexityLevel.Complex or 
            ComplexityLevel.VeryComplex or 
            ComplexityLevel.Extreme => _enhancedFormatter.FormatBreakdown(typedResult, breakdown, originalExpression),
            _ => FormatFallbackBreakdown(breakdown)
        };
    }

    /// <summary>
    /// Simple formatting for basic expressions
    /// </summary>
    private string FormatSimpleBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";

        // For simple expressions, just wrap in code formatting
        return $"`{breakdown}`";
    }

    /// <summary>
    /// Moderate formatting with some structure but no tree symbols
    /// </summary>
    private string FormatModerateBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";

        // Handle specific patterns for moderate complexity
        if (breakdown.Contains("min("))
        {
            var minMatch = System.Text.RegularExpressions.Regex.Match(breakdown,
                @"min\(([^,]+),\s*([^)]+)\)\s*=\s*(\d+)");
            if (minMatch.Success)
            {
                var expr1 = minMatch.Groups[1].Value.Trim();
                var expr2 = minMatch.Groups[2].Value.Trim();
                var result = minMatch.Groups[3].Value.Trim();
                
                return $"```\nmin({expr1}, {expr2}) = {result}\n```";
            }
        }

        if (breakdown.Contains("repeat("))
        {
            var repeatMatch = System.Text.RegularExpressions.Regex.Match(breakdown,
                @"repeat\(([^,]+),\s*(\d+)\):\s*\[([^\]]+)\]\s*=\s*(\d+)");
            if (repeatMatch.Success)
            {
                var expression = repeatMatch.Groups[1].Value.Trim();
                var iterations = repeatMatch.Groups[2].Value.Trim();
                var values = repeatMatch.Groups[3].Value.Trim();
                var total = repeatMatch.Groups[4].Value.Trim();
                
                return $"```\nrepeat({expression}, {iterations}):\nResults: [{values}]\nTotal: {total}\n```";
            }
        }

        // Handle comparisons
        if (breakdown.Contains(" > ") || breakdown.Contains(" < ") ||
            breakdown.Contains(">=") || breakdown.Contains("<="))
        {
            var compMatch = System.Text.RegularExpressions.Regex.Match(breakdown,
                @"\(([^)]+)\)\s*=\s*(True|False)");
            if (compMatch.Success)
            {
                var comparison = compMatch.Groups[1].Value.Trim();
                var result = compMatch.Groups[2].Value.Trim();
                
                return $"```\nComparison: {comparison}\nResult: {result}\n```";
            }
        }

        // Default moderate formatting
        return $"```\n{breakdown}\n```";
    }

    /// <summary>
    /// Fallback formatting when no TypedResult is available
    /// </summary>
    private string FormatFallbackBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";

        // Use the existing regex-based formatting for compatibility
        return FormatWithRegexPatterns(breakdown);
    }

    /// <summary>
    /// Legacy regex-based formatting for compatibility
    /// </summary>
    private string FormatWithRegexPatterns(string breakdown)
    {
        // Handle repeat functions
        if (breakdown.Contains("repeat("))
        {
            var repeatMatch = System.Text.RegularExpressions.Regex.Match(breakdown, 
                @"repeat\(([^,]+),\s*(\d+)\):\s*\[([^\]]+)\]\s*=\s*(\d+)");
            if (repeatMatch.Success)
            {
                var expression = repeatMatch.Groups[1].Value;
                var iterations = repeatMatch.Groups[2].Value;
                var values = repeatMatch.Groups[3].Value;
                var total = repeatMatch.Groups[4].Value;
                
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"```");
                sb.AppendLine($"repeat({expression}, {iterations}):");
                var valueList = values.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < valueList.Length; i++)
                {
                    sb.AppendLine($"  Roll {i + 1}: {valueList[i]}");
                }
                sb.AppendLine($"  Total: {total}");
                sb.AppendLine($"```");
                return sb.ToString();
            }
        }
        
        // Handle min functions
        if (breakdown.Contains("min("))
        {
            var minMatch = System.Text.RegularExpressions.Regex.Match(breakdown,
                @"min\(([^,]+),\s*([^)]+)\)\s*=\s*(\d+)");
            if (minMatch.Success)
            {
                var expr1 = minMatch.Groups[1].Value;
                var expr2 = minMatch.Groups[2].Value;
                var result = minMatch.Groups[3].Value;
                
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"```");
                sb.AppendLine($"min({expr1}, {expr2}):");
                sb.AppendLine($"  Chose minimum: {result}");
                sb.AppendLine($"```");
                return sb.ToString();
            }
        }
        
        // Handle comparisons
        if (breakdown.Contains(" > ") || breakdown.Contains(" < "))
        {
            var compMatch = System.Text.RegularExpressions.Regex.Match(breakdown,
                @"\(([^)]+)\)\s*=\s*(True|False)");
            if (compMatch.Success)
            {
                var comparison = compMatch.Groups[1].Value;
                var result = compMatch.Groups[2].Value;
                
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"```");
                sb.AppendLine($"Comparison: {comparison}");
                sb.AppendLine($"Result: {result}");
                sb.AppendLine($"```");
                return sb.ToString();
            }
        }
        
        // Handle exploding dice
        if (breakdown.Contains("!"))
        {
            return $"`{breakdown}` (exploding dice)";
        }
        
        // Default formatting
        return $"`{breakdown}`";
    }
}