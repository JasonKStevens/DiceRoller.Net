using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting.Core;

/// <summary>
/// Context information for making formatting decisions
/// </summary>
public class FormattingContext
{
    public string Platform { get; set; } = "discord";
    public bool IsInteractive { get; set; } = true;
    public bool SupportsStyling { get; set; } = true;
    public bool SupportsEmojis { get; set; } = true;
    public bool SupportsMarkdown { get; set; } = true;
    public bool SupportsHtml { get; set; } = false;
    public int MaxLineLength { get; set; } = 2000;
    public int MaxLines { get; set; } = 50;
    public string UserId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public bool IsMobile { get; set; }
    public string UserAgent { get; set; } = "";
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
    
    /// <summary>
    /// Previous results in the session for pattern analysis
    /// </summary>
    public List<DiceResultNode> SessionHistory { get; set; } = new();
    
    /// <summary>
    /// Frequency of usage patterns
    /// </summary>
    public Dictionary<string, int> UsagePatterns { get; set; } = new();
    
    /// <summary>
    /// User's explicit preference if available
    /// </summary>
    public ResultFormat? UserPreferredFormat { get; set; }
    
    /// <summary>
    /// Whether this is a batch operation
    /// </summary>
    public bool IsBatchOperation { get; set; }
    
    /// <summary>
    /// Number of results in current batch
    /// </summary>
    public int BatchSize { get; set; } = 1;
    
    /// <summary>
    /// Whether the platform has space constraints (computed property)
    /// </summary>
    public bool HasSpaceConstraints => MaxLineLength < 1000 || IsMobile;
    
    /// <summary>
    /// Whether the platform supports rich text (computed property)
    /// </summary>
    public bool SupportsRichText => SupportsMarkdown || SupportsStyling;
}