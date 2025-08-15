using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Core.Parser;
using DSharpPlus.Entities;
using System.Text;

namespace DiceRoller.Discord;

/// <summary>
/// Discord adapter for Progressive Disclosure formatting system
/// </summary>
public class ProgressiveDisclosureDiscordAdapter
{
    private readonly IResultNodeConverter _converter;
    private readonly AdaptiveFormatterManager _adaptiveFormatterManager;
    private readonly string _userId;
    private readonly bool _isEphemeral;
    
    public ProgressiveDisclosureDiscordAdapter(
        IResultNodeConverter converter,
        AdaptiveFormatterManager adaptiveFormatterManager,
        string userId = "", 
        bool isEphemeral = false)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _adaptiveFormatterManager = adaptiveFormatterManager ?? throw new ArgumentNullException(nameof(adaptiveFormatterManager));
        _userId = userId;
        _isEphemeral = isEphemeral;
    }
    
    /// <summary>
    /// Process a TypedResult into a Discord-formatted response
    /// </summary>
    public DiscordFormattedResponse ProcessResult(
        TypedResult typedResult, 
        string originalExpression, 
        ResultFormat? preferredFormat = null)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
        {
            return new DiscordFormattedResponse
            {
                Content = "Could not process the dice roll expression.",
                RequiresFile = false,
                Components = new List<DiscordComponent>()
            };
        }
        
        // Convert TypedResult to DiceResultNode
        var diceResultNode = _converter.Convert(typedResult);
        
        // Create formatting context for Discord
        var context = CreateDiscordContext();
        
        // Determine the best format to use
        var format = DetermineFormat(diceResultNode, preferredFormat, context);
        
        // Format the result using DiscordFormatter
        var formatter = new DiscordFormatter(context, format, _adaptiveFormatterManager);
        var formattedResult = formatter.Visit(diceResultNode, originalExpression);
        
        // Convert to Discord response
        return CreateDiscordResponse(formattedResult, diceResultNode, originalExpression);
    }
    
    private FormattingContext CreateDiscordContext()
    {
        return new FormattingContext
        {
            Platform = "discord",
            IsInteractive = true,
            SupportsStyling = true,
            SupportsEmojis = true,
            SupportsMarkdown = true,
            SupportsHtml = false,
            MaxLineLength = 2000,
            MaxLines = 50,
            UserId = _userId,
            SessionId = $"discord_{_userId}_{DateTime.UtcNow:yyyyMMdd}",
            IsMobile = false,
            UserAgent = "Discord Bot"
        };
    }
    
    private ResultFormat DetermineFormat(
        DiceResultNode result, 
        ResultFormat? preferredFormat, 
        FormattingContext context)
    {
        // Use preferred format if specified
        if (preferredFormat.HasValue && preferredFormat.Value != ResultFormat.Auto)
        {
            return preferredFormat.Value;
        }
        
        // Analyze complexity and determine appropriate format
        var complexity = AnalyzeComplexity(result);
        
        return complexity switch
        {
            ComplexityLevel.Simple => ResultFormat.Standard,
            ComplexityLevel.Medium => ResultFormat.Standard,
            ComplexityLevel.Complex => ResultFormat.Detailed,
            ComplexityLevel.VeryComplex => ResultFormat.Detailed,
            _ => ResultFormat.Standard
        };
    }
    
    private ComplexityLevel AnalyzeComplexity(DiceResultNode result)
    {
        var score = 0;
        
        // Factor in breakdown length
        if (!string.IsNullOrEmpty(result.Breakdown))
        {
            if (result.Breakdown.Length > 100) score += 3;
            else if (result.Breakdown.Length > 50) score += 2;
            else if (result.Breakdown.Length > 20) score += 1;
        }
        
        // Factor in value size
        if (Math.Abs(result.Value) > 100) score += 1;
        if (Math.Abs(result.Value) > 1000) score += 2;
        
        // Check for special patterns in breakdown
        if (!string.IsNullOrEmpty(result.Breakdown))
        {
            if (result.Breakdown.Contains("[") && result.Breakdown.Contains("]")) score += 1;
            if (result.Breakdown.Contains("exploding") || result.Breakdown.Contains("!")) score += 2;
            if (result.Breakdown.Contains("repeat") || result.Breakdown.Contains("min")) score += 2;
        }
        
        // Check TypedResult structure
        if (result.TypedResult != null && result.TypedResult.SubText.Count > 3) score += 2;
        
        return score switch
        {
            <= 2 => ComplexityLevel.Simple,
            <= 5 => ComplexityLevel.Medium,
            <= 8 => ComplexityLevel.Complex,
            _ => ComplexityLevel.VeryComplex
        };
    }
    
    private DiscordFormattedResponse CreateDiscordResponse(
        FormattedResult formattedResult, 
        DiceResultNode diceResult, 
        string originalExpression)
    {
        var response = new DiscordFormattedResponse();
        
        // Always use intelligent truncation instead of file attachments
        // The DiscordFormatter now handles intelligent truncation internally
        response.Content = formattedResult.PrimaryText;
        response.RequiresFile = false;
        
        // Since we never require files, we can always include interactive components
        response.Components = CreateDiscordComponents(formattedResult, diceResult, originalExpression);
        
        return response;
    }
    
    private List<DiscordComponent> CreateDiscordComponents(
        FormattedResult formattedResult, 
        DiceResultNode diceResult, 
        string originalExpression)
    {
        var components = new List<DiscordComponent>();
        
        try
        {
            var buttons = new List<DiscordButtonComponent>();
            
            // Re-roll button - always include this one
            buttons.Add(new DiscordButtonComponent(
                DSharpPlus.ButtonStyle.Primary,
                $"reroll_{Guid.NewGuid():N}",
                "🔄 Re-roll",
                false
            ));
            
            // Show details button (if not already detailed)
            if (formattedResult.UsedFormat != ResultFormat.Detailed && formattedResult.IsExpandable)
            {
                buttons.Add(new DiscordButtonComponent(
                    DSharpPlus.ButtonStyle.Secondary,
                    $"details_{Guid.NewGuid():N}",
                    "📊 Details",
                    false
                ));
            }
            
            // Copy result button
            buttons.Add(new DiscordButtonComponent(
                DSharpPlus.ButtonStyle.Secondary,
                $"copy_{Guid.NewGuid():N}",
                "📋 Copy",
                false
            ));
            
            // Add buttons to action row (Discord limit: 5 buttons per row, but use max 3 for safety)
            if (buttons.Any())
            {
                var safeButtonCount = Math.Min(buttons.Count, 3);
                var buttonRow = new DiscordActionRowComponent(buttons.Take(safeButtonCount).ToArray());
                components.Add(buttonRow);
            }
        }
        catch (Exception)
        {
            // If component creation fails, return empty list to avoid breaking the response
            return new List<DiscordComponent>();
        }
        
        return components;
    }
    
    private string FormatValue(float value)
    {
        return value == (int)value ? ((int)value).ToString() : value.ToString("0.##");
    }
}

/// <summary>
/// Response data for Discord formatting
/// </summary>
public class DiscordFormattedResponse
{
    public string Content { get; set; } = "";
    public bool RequiresFile { get; set; }
    public string? FileName { get; set; }
    public byte[]? FileContent { get; set; }
    public List<DiscordComponent> Components { get; set; } = new();
}

/// <summary>
/// Complexity levels for determining formatting
/// </summary>
public enum ComplexityLevel
{
    Simple,
    Medium,
    Complex,
    VeryComplex
}