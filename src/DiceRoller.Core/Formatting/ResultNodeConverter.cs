using DiceRoller.Core.Parser;
using System.Text.RegularExpressions;
using System.Text;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Enhanced implementation for converting TypedResult to DiceResultNode with proper parsing of complex expressions
/// </summary>
public class ResultNodeConverter : IResultNodeConverter
{
    public DiceResultNode Convert(TypedResult typedResult)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
        {
            return new DiceResultNode(0, "No result", typedResult);
        }

        // Extract numeric value from the result
        var value = ExtractNumericValue(typedResult);
        
        // Build structured breakdown from the typed result
        var breakdown = BuildStructuredBreakdown(typedResult);
        
        return new DiceResultNode(value, breakdown, typedResult);
    }
    
    private float ExtractNumericValue(TypedResult result)
    {
        // First try to extract from the main text
        if (!string.IsNullOrEmpty(result.Text))
        {
            var value = ExtractNumberFromText(result.Text);
            if (value.HasValue)
                return value.Value;
        }
        
        // For complex structures, look at the first numeric sub-result
        foreach (var subResult in result.SubText)
        {
            var value = ExtractNumericValue(subResult);
            if (value != 0)
                return value;
        }
        
        return 0;
    }
    
    private float? ExtractNumberFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        
        // Look for various number patterns
        var patterns = new[]
        {
            @"Result:\s*([+-]?\d+(?:\.\d+)?)",
            @"Total:\s*([+-]?\d+(?:\.\d+)?)",
            @"Value:\s*([+-]?\d+(?:\.\d+)?)",
            @"^([+-]?\d+(?:\.\d+)?)$",  // Just a number
            @"([+-]?\d+(?:\.\d+)?)$"   // Number at the end
        };
        
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && float.TryParse(match.Groups[1].Value, out var value))
            {
                return value;
            }
        }
        
        return null;
    }
    
    private string BuildStructuredBreakdown(TypedResult result)
    {
        if (result == null || result == TypedResult.Null)
            return "No breakdown available";
        
        return result.NodeType switch
        {
            NodeType.Repeat => BuildRepeatBreakdown(result),
            NodeType.Min => BuildMinBreakdown(result),
            NodeType.DiceRoll => BuildDiceRollBreakdown(result),
            NodeType.DiceRollTotal => BuildDiceRollTotalBreakdown(result),
            NodeType.Operator => BuildOperatorBreakdown(result),
            NodeType.Lookup => BuildLookupBreakdown(result),
            NodeType.StepFunc => BuildStepFuncBreakdown(result),
            NodeType.Comment => BuildCommentBreakdown(result),
            NodeType.Text => BuildTextBreakdown(result),
            NodeType.None => BuildGenericBreakdown(result),
            _ => BuildGenericBreakdown(result)
        };
    }
    
    private string BuildRepeatBreakdown(TypedResult result)
    {
        var sb = new StringBuilder();
        
        // Build individual iteration results from SubText
        var iterationResults = new List<string>();
        foreach (var subResult in result.SubText)
        {
            var iterationBreakdown = BuildStructuredBreakdown(subResult);
            if (!string.IsNullOrEmpty(iterationBreakdown) && iterationBreakdown != "No breakdown available")
            {
                iterationResults.Add(iterationBreakdown);
            }
        }
        
        if (iterationResults.Any())
        {
            sb.Append($"repeat({iterationResults.Count} iterations): ");
            sb.Append($"[{string.Join(", ", iterationResults)}] = {result.Text}");
            return sb.ToString();
        }
        
        return $"repeat: {result.Text}";
    }
    
    private string BuildMinBreakdown(TypedResult result)
    {
        // Extract the actual values that were compared from SubText
        var subValues = new List<string>();
        foreach (var subResult in result.SubText)
        {
            var subBreakdown = BuildStructuredBreakdown(subResult);
            if (!string.IsNullOrEmpty(subBreakdown) && subBreakdown != "No breakdown available")
            {
                subValues.Add(subBreakdown);
            }
        }
        
        if (subValues.Count >= 2)
        {
            return $"min({subValues[0]}, {subValues[1]}) = {result.Text}";
        }
        else if (subValues.Count == 1)
        {
            return $"min({subValues[0]}, ?) = {result.Text}";
        }
        
        return $"min result: {result.Text}";
    }
    
    private string BuildDiceRollBreakdown(TypedResult result)
    {
        if (!string.IsNullOrEmpty(result.Text))
        {
            // Handle exploding dice notation
            if (result.Text.Contains("!"))
            {
                return FormatExplodingDice(result.Text);
            }
            
            // Handle regular dice rolls
            var match = Regex.Match(result.Text, @"\[([^\]]+)\]");
            if (match.Success)
            {
                return match.Value; // Return the dice roll in brackets
            }
        }
        
        return result.Text ?? "dice roll";
    }
    
    private string BuildDiceRollTotalBreakdown(TypedResult result)
    {
        // For dice roll totals, combine individual dice rolls
        var diceValues = new List<string>();
        bool hasExplodingDice = false;
        
        foreach (var subResult in result.SubText)
        {
            if (subResult.NodeType == NodeType.DiceRoll)
            {
                var diceText = subResult.Text ?? "?";
                diceValues.Add(diceText);
                if (diceText.Contains("!"))
                {
                    hasExplodingDice = true;
                }
            }
        }
        
        if (diceValues.Any())
        {
            var result_text = $"[{string.Join(", ", diceValues)}]";
            if (hasExplodingDice)
            {
                result_text += "!"; // Mark the whole roll as exploding if any dice exploded
            }
            return result_text;
        }
        
        return result.Text ?? "dice total";
    }
    
    private string BuildOperatorBreakdown(TypedResult result)
    {
        // For operators, look for the operator in SubText and combine properly
        if (result.SubText.Count >= 3)
        {
            var left = BuildStructuredBreakdown(result.SubText[0]);
            var operatorText = result.SubText[1].Text; // The operator should be in the middle
            var right = BuildStructuredBreakdown(result.SubText[2]);
            
            if (operatorText == ">")
            {
                // Special handling for comparisons
                var leftValue = ExtractNumericValue(result.SubText[0]);
                var rightValue = ExtractNumericValue(result.SubText[2]);
                var resultValue = leftValue > rightValue;
                return $"({left} > {right}) = {resultValue}";
            }
            else if (operatorText == "<")
            {
                var leftValue = ExtractNumericValue(result.SubText[0]);
                var rightValue = ExtractNumericValue(result.SubText[2]);
                var resultValue = leftValue < rightValue;
                return $"({left} < {right}) = {resultValue}";
            }
            else if (operatorText == "+")
            {
                return $"{left} + {right}";
            }
            else if (!string.IsNullOrEmpty(operatorText))
            {
                return $"{left} {operatorText} {right}";
            }
        }
        else if (result.SubText.Count >= 2)
        {
            var left = BuildStructuredBreakdown(result.SubText[0]);
            var right = BuildStructuredBreakdown(result.SubText[1]);
            return $"{left} + {right}"; // Default to addition if no operator found
        }
        
        return result.Text ?? "operation";
    }
    
    private string BuildLookupBreakdown(TypedResult result)
    {
        if (!string.IsNullOrEmpty(result.Text))
        {
            return result.Text;
        }
        
        return BuildGenericBreakdown(result);
    }
    
    private string BuildStepFuncBreakdown(TypedResult result)
    {
        if (!string.IsNullOrEmpty(result.Text))
        {
            return $"step({result.Text})";
        }
        
        return BuildGenericBreakdown(result);
    }
    
    private string BuildCommentBreakdown(TypedResult result)
    {
        // Comments don't contribute to breakdown
        return "";
    }
    
    private string BuildTextBreakdown(TypedResult result)
    {
        return result.Text ?? "";
    }
    
    private string BuildGenericBreakdown(TypedResult result)
    {
        if (!string.IsNullOrEmpty(result.Text))
        {
            return result.Text;
        }
        
        // Combine sub-results
        var parts = new List<string>();
        foreach (var subResult in result.SubText)
        {
            var subBreakdown = BuildStructuredBreakdown(subResult);
            if (!string.IsNullOrEmpty(subBreakdown) && subBreakdown != "No breakdown available")
            {
                parts.Add(subBreakdown);
            }
        }
        
        return parts.Any() ? string.Join(" ", parts) : "No breakdown available";
    }
    
    private string FormatExplodingDice(string text)
    {
        // Handle exploding dice notation like "6! 3" -> "6! + 3"
        var explodingPattern = @"(\d+!)\s+(\d+)";
        var match = Regex.Match(text, explodingPattern);
        if (match.Success)
        {
            return $"{match.Groups[1].Value} + {match.Groups[2].Value}";
        }
        
        return text;
    }
    
    private string ExtractOperator(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        
        var operators = new[] { ">=", "<=", "==", "!=", ">", "<", "+", "-", "*", "/", "%" };
        foreach (var op in operators)
        {
            if (text.Contains(op))
                return op;
        }
        
        return "";
    }
}