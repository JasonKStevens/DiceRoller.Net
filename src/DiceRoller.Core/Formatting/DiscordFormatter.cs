using DiceRoller.Core.Formatting.Core;
using DiceRoller.Core.Parser;
using System.Text;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Discord-specific formatting visitor with verbosity awareness
/// </summary>
public class DiscordFormatter
{
    protected FormattingContext Context { get; }
    protected ResultFormat Format { get; }
    private readonly AdaptiveFormatterManager _adaptiveFormatterManager;
    
    public DiscordFormatter(FormattingContext context, ResultFormat format, AdaptiveFormatterManager adaptiveFormatterManager)
    {
        Context = context;
        Format = format;
        _adaptiveFormatterManager = adaptiveFormatterManager ?? throw new ArgumentNullException(nameof(adaptiveFormatterManager));
    }
    
    /// <summary>
    /// Visit a typed result and format it according to Discord capabilities
    /// </summary>
    public FormattedResult Visit(DiceResultNode result, string originalExpression)
    {
        var formattedResult = new FormattedResult
        {
            Value = result.Value,
            UsedFormat = Format,
            Context = Context,
            TypedResult = result.TypedResult,
            Metadata = new FormattingMetadata
            {
                FormatterUsed = nameof(DiscordFormatter),
                Reason = $"Discord platform formatting with {Format} verbosity"
            }
        };
        
        switch (Format)
        {
            case ResultFormat.Terse:
                formattedResult.PrimaryText = FormatTerse(result, originalExpression);
                break;
                
            case ResultFormat.Standard:
                formattedResult.PrimaryText = FormatStandard(result, originalExpression);
                break;
                
            case ResultFormat.Detailed:
                formattedResult.PrimaryText = FormatDetailed(result, originalExpression);
                formattedResult.IsExpandable = true;
                formattedResult.CollapsedSummary = FormatStandard(result, originalExpression);
                formattedResult.ExpandedDetail = FormatExpandedDetail(result, originalExpression);
                break;
                
            case ResultFormat.Narrative:
                formattedResult.PrimaryText = FormatNarrative(result, originalExpression);
                break;
                
            case ResultFormat.Technical:
                formattedResult.PrimaryText = FormatTechnical(result, originalExpression);
                formattedResult.IsExpandable = true;
                break;
                
            default:
                formattedResult.PrimaryText = FormatStandard(result, originalExpression);
                break;
        }
        
        // Add Discord-specific interactive elements
        AddDiscordInteractiveElements(formattedResult, result, originalExpression);
        
        // Add alternative formats using Discord markdown
        AddDiscordAlternativeFormats(formattedResult, result, originalExpression);
        
        return formattedResult;
    }
    
    private string FormatTerse(DiceResultNode result, string originalExpression)
    {
        var value = FormatValue(result.Value);
        var comment = ExtractComment(originalExpression);
        
        return string.IsNullOrEmpty(comment) ? value : $"{value} {comment}";
    }
    
    private string FormatStandard(DiceResultNode result, string originalExpression)
    {
        var sb = new StringBuilder();
        
        // Add dice emoji
        sb.Append(GetEmoji("🎲", "[DICE]"));
        sb.Append(" ");
        
        // Add result value in bold
        sb.Append(ApplyBoldStyle(FormatValue(result.Value)));
        
        // Add breakdown if available
        var breakdown = ExtractSimpleBreakdown(result.Breakdown);
        if (!string.IsNullOrEmpty(breakdown) && breakdown != FormatValue(result.Value))
        {
            sb.Append($" ({breakdown})");
        }
        
        // Add comment
        var comment = ExtractComment(originalExpression);
        if (!string.IsNullOrEmpty(comment))
        {
            sb.Append($" {comment}");
        }
        
        return TruncateIfNeeded(sb.ToString());
    }
    
    private string FormatDetailed(DiceResultNode result, string originalExpression)
    {
        var sb = new StringBuilder();
        
        // Title with emoji
        sb.AppendLine($"{GetEmoji("🎲", "[DICE]")} **Dice Roll Result**");
        
        // Expression if available
        if (!string.IsNullOrEmpty(originalExpression))
        {
            var expression = originalExpression.Split('#')[0].Trim();
            sb.AppendLine($"**Expression:** `{expression}`");
        }
        
        // Result
        sb.AppendLine($"**Result:** {ApplyBoldStyle(FormatValue(result.Value))}");
        
        // Enhanced Breakdown
        if (!string.IsNullOrEmpty(result.Breakdown))
        {
            var formattedBreakdown = FormatComplexBreakdown(result.TypedResult, result.Breakdown, originalExpression);
            sb.AppendLine($"**Breakdown:**");
            sb.AppendLine(formattedBreakdown);
        }
        
        // Comment
        var comment = ExtractComment(originalExpression);
        if (!string.IsNullOrEmpty(comment))
        {
            sb.AppendLine($"**Note:** {comment.Substring(1).Trim()}");
        }
        
        return TruncateIfNeeded(sb.ToString().Trim());
    }
    
    /// <summary>
    /// Format complex breakdowns in a more readable way for detailed view
    /// </summary>
    private string FormatComplexBreakdown(TypedResult typedResult, string breakdown, string originalExpression)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";
        
        // Use adaptive formatting for enhanced breakdown display
        return _adaptiveFormatterManager.FormatBreakdown(typedResult, breakdown, originalExpression);
    }
    
    private string FormatNarrative(DiceResultNode result, string originalExpression)
    {
        var narratives = new[]
        {
            $"{GetEmoji("✨")} The dice tumble across the table, finally settling on **{FormatValue(result.Value)}**.",
            $"{GetEmoji("🎲")} With a gentle roll, the dice come to rest showing **{FormatValue(result.Value)}**.",
            $"{GetEmoji("✨")} The dice dance briefly before revealing **{FormatValue(result.Value)}**."
        };
        
        var random = new Random((int)(result.Value * 1000));
        var narrative = narratives[random.Next(narratives.Length)];
        
        var comment = ExtractComment(originalExpression);
        if (!string.IsNullOrEmpty(comment))
        {
            var commentText = comment.Substring(1).Trim();
            narrative += $" This roll was for {commentText.ToLower()}.";
        }
        
        return TruncateIfNeeded(narrative);
    }
    
    private string FormatTechnical(DiceResultNode result, string originalExpression)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("```");
        sb.AppendLine("=== TECHNICAL DEBUG ===");
        sb.AppendLine($"Value: {FormatValue(result.Value)}");
        sb.AppendLine($"Expression: {originalExpression}");
        sb.AppendLine($"Breakdown: {result.Breakdown}");
        sb.AppendLine($"Timestamp: {DateTime.UtcNow:HH:mm:ss}");
        sb.AppendLine("=======================");
        sb.AppendLine("```");
        
        return sb.ToString();
    }
    
    private string FormatExpandedDetail(DiceResultNode result, string originalExpression)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("||**Expanded Details**");
        sb.AppendLine($"Full Breakdown: `{result.Breakdown}`");
        sb.AppendLine($"Original Expression: `{originalExpression}`");
        sb.AppendLine($"Formatted at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC||");
        
        return sb.ToString();
    }
    
    private void AddDiscordInteractiveElements(FormattedResult result, DiceResultNode diceResult, string originalExpression)
    {
        // Re-roll button (would be implemented as a Discord component)
        result.InteractiveElements.Add(new InteractiveElement
        {
            Type = "button",
            Content = "🔄 Re-roll",
            Action = "reroll",
            Properties = { ["expression"] = originalExpression }
        });
        
        // Show different formats button
        if (Format != ResultFormat.Detailed)
        {
            result.InteractiveElements.Add(new InteractiveElement
            {
                Type = "button",
                Content = "📊 Details",
                Action = "show_detailed",
                Properties = { }
            });
        }
        
        // Copy result button
        result.InteractiveElements.Add(new InteractiveElement
        {
            Type = "button",
            Content = "📋 Copy",
            Action = "copy",
            Properties = { ["text"] = FormatValue(diceResult.Value) }
        });
    }
    
    private void AddDiscordAlternativeFormats(FormattedResult result, DiceResultNode diceResult, string originalExpression)
    {
        // Spoiler format for detailed information
        if (Format != ResultFormat.Detailed)
        {
            result.AlternativeFormats["spoiler"] = $"||{FormatDetailed(diceResult, originalExpression)}||";
        }
        
        // Code block format for technical information
        result.AlternativeFormats["code_block"] = $"```\n{FormatValue(diceResult.Value)}\n{diceResult.Breakdown}\n```";
        
        // Minimal format
        result.AlternativeFormats["minimal"] = FormatValue(diceResult.Value);
        
        // Inline code format
        result.AlternativeFormats["inline_code"] = $"`{FormatValue(diceResult.Value)}`";
    }
    
    protected string ApplyBoldStyle(string text)
    {
        return $"**{text}**";
    }
    
    protected string ApplyItalicStyle(string text)
    {
        return $"*{text}*";
    }
    
    protected string ApplyCodeStyle(string text)
    {
        return $"`{text}`";
    }
    
    protected string ApplyHighlightStyle(string text)
    {
        // Discord doesn't have highlight, use bold instead
        return ApplyBoldStyle(text);
    }
    
    protected virtual string GetEmoji(string emoji, string fallback = "")
    {
        return Context.SupportsEmojis ? emoji : fallback;
    }
    
    /// <summary>
    /// Extract comment from expression
    /// </summary>
    protected string ExtractComment(string expression)
    {
        if (string.IsNullOrEmpty(expression) || !expression.Contains('#'))
            return "";
        
        var commentIndex = expression.IndexOf('#');
        return expression.Substring(commentIndex).Trim();
    }
    
    /// <summary>
    /// Extract simple breakdown from full breakdown string
    /// </summary>
    protected string ExtractSimpleBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "";
        
        // Handle structured breakdowns from the new ResultNodeConverter
        if (breakdown.Contains("repeat("))
        {
            // For repeat functions, extract the summary
            var match = System.Text.RegularExpressions.Regex.Match(breakdown, @"repeat\([^:]+:\s*\[([^\]]+)\]");
            if (match.Success)
            {
                return $"[{match.Groups[1].Value}]";
            }
        }
        
        if (breakdown.Contains("min("))
        {
            // For min functions, extract the result
            var match = System.Text.RegularExpressions.Regex.Match(breakdown, @"min\([^=]+=\s*(\d+)");
            if (match.Success)
            {
                return $"min result: {match.Groups[1].Value}";
            }
        }
        
        // Handle comparisons
        if (breakdown.Contains(" > ") || breakdown.Contains(" < "))
        {
            var match = System.Text.RegularExpressions.Regex.Match(breakdown, @"\(([^)]+)\)\s*=\s*(True|False)");
            if (match.Success)
            {
                return $"{match.Groups[1].Value} = {match.Groups[2].Value}";
            }
        }
        
        // Handle exploding dice
        if (breakdown.Contains("!"))
        {
            return breakdown; // Exploding dice are already well formatted
        }
        
        // Handle regular dice in brackets
        if (breakdown.StartsWith('[') && breakdown.EndsWith(']'))
        {
            return breakdown; // Already well formatted
        }
        
        // Original logic for backwards compatibility
        var parts = breakdown.Split(':', 2);
        if (parts.Length > 1)
        {
            var breakdownPart = parts[1].Trim();
            if (breakdownPart.StartsWith('[') && breakdownPart.Contains(']'))
            {
                return breakdownPart;
            }
        }
        
        return breakdown;
    }
    
    /// <summary>
    /// Format a numeric value appropriately for the platform
    /// </summary>
    protected virtual string FormatValue(float value)
    {
        return value == (int)value ? ((int)value).ToString() : value.ToString("0.##");
    }
    
    /// <summary>
    /// Get the maximum allowed length for output
    /// </summary>
    protected virtual int GetMaxLength()
    {
        return Format switch
        {
            ResultFormat.Terse => Math.Min(50, Context.MaxLineLength),
            ResultFormat.Standard => Math.Min(200, Context.MaxLineLength),
            ResultFormat.Detailed => Context.MaxLineLength,
            ResultFormat.Narrative => Math.Min(500, Context.MaxLineLength),
            ResultFormat.Technical => int.MaxValue,
            _ => Context.MaxLineLength
        };
    }
    
    /// <summary>
    /// Truncate text to fit constraints using intelligent strategies
    /// </summary>
    protected string TruncateIfNeeded(string text, int? maxLength = null)
    {
        var limit = maxLength ?? GetMaxLength();
        if (text.Length <= limit)
            return text;
        
        return ApplyIntelligentTruncation(text, limit);
    }
    
    /// <summary>
    /// Apply intelligent truncation strategies that preserve important information
    /// </summary>
    private string ApplyIntelligentTruncation(string text, int maxLength)
    {
        // Strategy 1: Try to truncate breakdown while preserving structure
        var truncated = TruncateBreakdownIntelligently(text, maxLength);
        if (truncated.Length <= maxLength)
            return truncated;
        
        // Strategy 2: Extract and preserve essential components
        return ExtractEssentialComponents(text, maxLength);
    }
    
    /// <summary>
    /// Truncate breakdown content while preserving dice emoji, total, and comments
    /// </summary>
    private string TruncateBreakdownIntelligently(string text, int maxLength)
    {
        // Look for breakdown pattern like "🎲 **123** (breakdown content) comment"
        var emojiIndex = text.IndexOf("🎲");
        var resultMatch = System.Text.RegularExpressions.Regex.Match(text, @"\*\*(\d+(?:\.\d+)?)\*\*");
        var commentMatch = System.Text.RegularExpressions.Regex.Match(text, @"(#[^#]*)$");
        
        if (!resultMatch.Success)
            return text.Substring(0, Math.Min(maxLength - 3, text.Length)) + "...";
        
        // Extract essential parts
        var emoji = emojiIndex >= 0 ? "🎲 " : "";
        var result = resultMatch.Value;
        var comment = commentMatch.Success ? " " + commentMatch.Value : "";
        
        // Find breakdown content in parentheses
        var breakdownMatch = System.Text.RegularExpressions.Regex.Match(text, @"\(([^)]+)\)");
        if (!breakdownMatch.Success)
            return emoji + result + comment; // No breakdown to truncate
        
        var breakdownContent = breakdownMatch.Groups[1].Value;
        
        // Calculate space available for breakdown
        var fixedParts = emoji + result + " (" + ")" + comment;
        var availableSpace = maxLength - fixedParts.Length - 3; // Reserve space for "..."
        
        if (availableSpace <= 10) // Not enough space for meaningful breakdown
            return emoji + result + comment;
        
        // Truncate breakdown intelligently
        var truncatedBreakdown = TruncateDiceBreakdown(breakdownContent, availableSpace);
        
        return emoji + result + " (" + truncatedBreakdown + ")" + comment;
    }
    
    /// <summary>
    /// Truncate dice breakdown showing first few results and total count
    /// </summary>
    private string TruncateDiceBreakdown(string breakdown, int maxLength)
    {
        // Parse dice results - look for patterns like "123 45! 67 89!" or "[45, 67, 89!]"
        var dicePattern = @"(\d+!?)";
        var matches = System.Text.RegularExpressions.Regex.Matches(breakdown, dicePattern);
        
        if (matches.Count <= 3) // Short enough, just truncate normally
            return breakdown.Length <= maxLength ? breakdown : breakdown.Substring(0, maxLength - 3) + "...";
        
        // Show first few dice and indicate total
        var firstFew = new List<string>();
        var totalCount = matches.Count;
        var currentLength = 0;
        
        for (int i = 0; i < matches.Count && i < 5; i++) // Show up to 5 dice
        {
            var diceValue = matches[i].Value;
            var addition = (i > 0 ? ", " : "") + diceValue;
            
            if (currentLength + addition.Length + $"... ({totalCount} dice)".Length > maxLength)
                break;
            
            firstFew.Add(diceValue);
            currentLength += addition.Length;
        }
        
        if (firstFew.Count == 0)
            return "...";
        
        if (firstFew.Count >= totalCount)
            return string.Join(", ", firstFew);
        
        return $"{string.Join(", ", firstFew)}... (showing first {firstFew.Count} of {totalCount} dice)";
    }
    
    /// <summary>
    /// Extract essential components when breakdown truncation isn't enough
    /// </summary>
    private string ExtractEssentialComponents(string text, int maxLength)
    {
        // Extract the most important parts: emoji, result value, and comment
        var emojiMatch = System.Text.RegularExpressions.Regex.Match(text, @"🎲\s*");
        var resultMatch = System.Text.RegularExpressions.Regex.Match(text, @"\*\*(\d+(?:\.\d+)?)\*\*");
        var commentMatch = System.Text.RegularExpressions.Regex.Match(text, @"(#[^#]*)$");
        
        var parts = new List<string>();
        
        if (emojiMatch.Success)
            parts.Add("🎲");
        
        if (resultMatch.Success)
            parts.Add($"**{resultMatch.Groups[1].Value}**");
        else
            parts.Add("**Result**"); // Fallback
        
        if (commentMatch.Success)
            parts.Add(commentMatch.Groups[1].Value);
        
        var result = string.Join(" ", parts);
        
        // If still too long, just show result and comment
        if (result.Length > maxLength && commentMatch.Success)
        {
            var withoutEmoji = string.Join(" ", parts.Skip(1));
            if (withoutEmoji.Length <= maxLength)
                return withoutEmoji;
        }
        
        return result.Length <= maxLength ? result : result.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// Text styling options
/// </summary>
public enum TextStyle
{
    None,
    Bold,
    Italic,
    Code,
    Highlight
}