using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting.Core;

/// <summary>
/// A formatted result with multiple representation options
/// </summary>
public class FormattedResult
{
    public float Value { get; set; }
    public string PrimaryText { get; set; } = "";
    public string SecondaryText { get; set; } = "";
    public string Breakdown { get; set; } = "";
    public ResultFormat UsedFormat { get; set; }
    public FormattingContext Context { get; set; } = new();
    public TypedResult? TypedResult { get; set; }
    
    /// <summary>
    /// Alternative representations for progressive enhancement
    /// </summary>
    public Dictionary<string, string> AlternativeFormats { get; set; } = new();
    
    /// <summary>
    /// Interactive elements for rich platforms
    /// </summary>
    public List<InteractiveElement> InteractiveElements { get; set; } = new();
    
    /// <summary>
    /// Metadata about the formatting decision
    /// </summary>
    public FormattingMetadata Metadata { get; set; } = new();
    
    /// <summary>
    /// Whether the result can be expanded for more detail
    /// </summary>
    public bool IsExpandable { get; set; }
    
    /// <summary>
    /// Collapsed summary for expandable results
    /// </summary>
    public string CollapsedSummary { get; set; } = "";
    
    /// <summary>
    /// Expanded detailed view
    /// </summary>
    public string ExpandedDetail { get; set; } = "";
}

/// <summary>
/// Interactive elements for rich platforms
/// </summary>
public class InteractiveElement
{
    public string Type { get; set; } = ""; // tooltip, button, link, etc.
    public string Content { get; set; } = "";
    public string Action { get; set; } = "";
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Metadata about formatting decisions
/// </summary>
public class FormattingMetadata
{
    public DateTime FormattedAt { get; set; } = DateTime.UtcNow;
    public string FormatterUsed { get; set; } = "";
    public string Reason { get; set; } = "";
    public bool WasAutoSelected { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}