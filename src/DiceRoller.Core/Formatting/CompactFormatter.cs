using DiceRoller.Core.Parser;
using System.Text;
using System.Text.RegularExpressions;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Compact formatter that creates horizontal output with minimal line breaks
/// </summary>
public class CompactFormatter : IAdaptiveBreakdownFormatter
{
    private readonly BreakdownTextParser _textParser;

    public CompactFormatter()
    {
        _textParser = new BreakdownTextParser();
    }

    /// <summary>
    /// Formats a dice expression breakdown in a compact horizontal style
    /// </summary>
    public string FormatBreakdown(TypedResult typedResult, string breakdown, string originalExpression)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
            return FormatFallbackBreakdown(breakdown);

        var sb = new StringBuilder();
        
        // Add main expression with result
        var expressionName = _textParser.ExtractExpressionName(originalExpression);
        var finalResult = ExtractFinalResult(breakdown);
        sb.AppendLine($"**{expressionName}** = **{finalResult}**");
        
        // Format the breakdown compactly
        var compactBreakdown = FormatNodeCompact(typedResult, breakdown);
        if (!string.IsNullOrEmpty(compactBreakdown))
        {
            sb.AppendLine(compactBreakdown);
        }
        
        var result = sb.ToString().TrimEnd();
        
        // Apply smart truncation if needed to stay within Discord limits
        if (result.Length > 1900) // Leave room for Discord formatting
        {
            result = CreateSmartTruncation(result);
        }
        
        return result;
    }

    /// <summary>
    /// Formats a TypedResult node in compact horizontal style
    /// </summary>
    private string FormatNodeCompact(TypedResult node, string fullBreakdown)
    {
        if (node == null || node == TypedResult.Null)
            return "";

        return node.NodeType switch
        {
            NodeType.DiceRoll => FormatDiceRollCompact(node, fullBreakdown),
            NodeType.Repeat => FormatRepeatCompact(node, fullBreakdown),
            NodeType.Min => FormatMinCompact(node, fullBreakdown),
            NodeType.Max => FormatMaxCompact(node, fullBreakdown),
            NodeType.Operator => FormatOperatorCompact(node, fullBreakdown),
            NodeType.Lookup => FormatLookupCompact(node, fullBreakdown),
            NodeType.StepFunc or NodeType.StepFuncDef => FormatStepFunctionCompact(node),
            NodeType.Comment => FormatCommentCompact(node),
            _ => FormatGenericCompact(node)
        };
    }

    /// <summary>
    /// Formats dice roll nodes horizontally: 5d6: [3, 5, 2, 6, 1] = 17
    /// </summary>
    private string FormatDiceRollCompact(TypedResult node, string fullBreakdown)
    {
        if (string.IsNullOrEmpty(node.Text))
            return "";

        // Extract dice roll details from breakdown
        var dicePattern = @"(\d*d\d+!?)\s*=\s*\[([^\]]+)\]\s*=\s*(\d+)";
        var match = Regex.Match(fullBreakdown, dicePattern);
        
        if (match.Success)
        {
            var diceExpression = match.Groups[1].Value;
            var rolls = match.Groups[2].Value;
            var total = match.Groups[3].Value;
            return $"{diceExpression}: [{rolls}] = {total}";
        }

        // Fallback: look for simpler patterns
        var simplePattern = @"(\d*d\d+!?)[^=]*=\s*(\d+)";
        var simpleMatch = Regex.Match(fullBreakdown, simplePattern);
        if (simpleMatch.Success)
        {
            return $"{simpleMatch.Groups[1].Value} = {simpleMatch.Groups[2].Value}";
        }

        return $"Dice: {node.Text}";
    }

    /// <summary>
    /// Formats repeat functions: repeat(3d6, 4): [12, 8, 15, 10]
    /// </summary>
    private string FormatRepeatCompact(TypedResult node, string fullBreakdown)
    {
        var repeatInfo = _textParser.ExtractRepeatInfo(fullBreakdown);
        if (repeatInfo.IsValid)
        {
            var values = string.Join(", ", repeatInfo.Values);
            return $"repeat({repeatInfo.Expression}, {repeatInfo.Count}): [{values}]";
        }

        // Fallback: try to extract from SubText
        if (node.SubText != null && node.SubText.Count > 0)
        {
            var values = node.SubText.Select(x => x.Text).Where(x => !string.IsNullOrEmpty(x));
            return $"repeat: [{string.Join(", ", values)}]";
        }

        return $"repeat: {node.Text}";
    }

    /// <summary>
    /// Formats min functions: min(3d6+2, 18) = 12
    /// </summary>
    private string FormatMinCompact(TypedResult node, string fullBreakdown)
    {
        var minInfo = _textParser.ExtractMinInfo(fullBreakdown);
        if (minInfo.IsValid)
        {
            return $"min({minInfo.Expression1}, {minInfo.Expression2}) = {minInfo.Result}";
        }

        // Try to extract from SubText
        if (node.SubText != null && node.SubText.Count >= 2)
        {
            var arg1 = node.SubText[0].Text;
            var arg2 = node.SubText[1].Text;
            return $"min({arg1}, {arg2}) = {node.Text}";
        }

        return $"min = {node.Text}";
    }

    /// <summary>
    /// Formats max functions: max(2d8, 10) = 10
    /// </summary>
    private string FormatMaxCompact(TypedResult node, string fullBreakdown)
    {
        // Similar to min but for max function
        var maxPattern = @"max\(([^,]+),\s*([^)]+)\)\s*=\s*(\d+)";
        var match = Regex.Match(fullBreakdown, maxPattern);
        
        if (match.Success)
        {
            return $"max({match.Groups[1].Value.Trim()}, {match.Groups[2].Value.Trim()}) = {match.Groups[3].Value}";
        }

        // Try to extract from SubText
        if (node.SubText != null && node.SubText.Count >= 2)
        {
            var arg1 = node.SubText[0].Text;
            var arg2 = node.SubText[1].Text;
            return $"max({arg1}, {arg2}) = {node.Text}";
        }

        return $"max = {node.Text}";
    }

    /// <summary>
    /// Formats operator nodes compactly
    /// </summary>
    private string FormatOperatorCompact(TypedResult node, string fullBreakdown)
    {
        // Check if this is a comparison operation
        if (node.SubText != null && node.SubText.Count == 3)
        {
            var left = node.SubText[0];
            var op = node.SubText[1];
            var right = node.SubText[2];
            
            // Check if the operator is a comparison operator
            if (op.Text == ">" || op.Text == "<" || op.Text == ">=" || op.Text == "<=")
            {
                var result = node.Text == "1" ? "True" : "False";
                return $"{left.Text} {op.Text} {right.Text} = {result}";
            }
        }

        // For arithmetic operations, format sub-expressions
        if (node.SubText != null && node.SubText.Count > 0)
        {
            var parts = node.SubText.Select(sub => FormatNodeCompact(sub, fullBreakdown))
                                   .Where(x => !string.IsNullOrEmpty(x));
            
            if (parts.Any())
            {
                return string.Join(" ", parts);
            }
        }

        return $"Operation: {node.Text}";
    }

    /// <summary>
    /// Formats lookup nodes (comparisons, game tables)
    /// </summary>
    private string FormatLookupCompact(TypedResult node, string fullBreakdown)
    {
        var compInfo = _textParser.ExtractComparisonInfo(fullBreakdown);
        if (compInfo.IsValid)
        {
            return $"{compInfo.Expression} = {compInfo.Result}";
        }

        return $"Lookup: {node.Text}";
    }

    /// <summary>
    /// Formats step function nodes
    /// </summary>
    private string FormatStepFunctionCompact(TypedResult node)
    {
        return $"step: {node.Text}";
    }

    /// <summary>
    /// Formats comment nodes
    /// </summary>
    private string FormatCommentCompact(TypedResult node)
    {
        if (!string.IsNullOrEmpty(node.Text))
        {
            return $"*{node.Text.Trim()}*";
        }
        return "";
    }

    /// <summary>
    /// Formats generic nodes
    /// </summary>
    private string FormatGenericCompact(TypedResult node)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(node.Text))
        {
            parts.Add(node.Text);
        }

        if (node.SubText != null && node.SubText.Count > 0)
        {
            var subParts = node.SubText.Select(sub => FormatNodeCompact(sub, ""))
                                      .Where(x => !string.IsNullOrEmpty(x));
            parts.AddRange(subParts);
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Extracts the final result from breakdown text
    /// </summary>
    private string ExtractFinalResult(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "0";

        // Look for the final result pattern
        var finalMatch = Regex.Match(breakdown, @"=\s*(\d+)\s*$");
        if (finalMatch.Success)
        {
            return finalMatch.Groups[1].Value;
        }

        // Fallback: try to find any number at the end
        var numberMatch = Regex.Match(breakdown, @"(\d+)\s*$");
        if (numberMatch.Success)
        {
            return numberMatch.Groups[1].Value;
        }

        return "0";
    }

    /// <summary>
    /// Creates smart truncation for long results
    /// </summary>
    private string CreateSmartTruncation(string result)
    {
        if (result.Length <= 1900)
            return result;

        var lines = result.Split('\n');
        if (lines.Length <= 1)
        {
            // Single line truncation
            return result.Substring(0, 1850) + "... *(truncated)*";
        }

        // Multi-line truncation - keep header and truncate details
        var sb = new StringBuilder();
        sb.AppendLine(lines[0]); // Keep the main result line
        
        var remainingLength = 1800 - lines[0].Length;
        for (int i = 1; i < lines.Length && remainingLength > 50; i++)
        {
            if (lines[i].Length <= remainingLength)
            {
                sb.AppendLine(lines[i]);
                remainingLength -= lines[i].Length + 1;
            }
            else
            {
                break;
            }
        }
        
        sb.AppendLine("... *(details truncated)*");
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Fallback formatting when TypedResult is not available
    /// </summary>
    private string FormatFallbackBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";

        return $"```\n{breakdown}\n```";
    }
}