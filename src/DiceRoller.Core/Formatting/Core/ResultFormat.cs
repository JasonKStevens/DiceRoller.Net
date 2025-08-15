namespace DiceRoller.Core.Formatting.Core;

/// <summary>
/// Available result formatting styles
/// </summary>
public enum ResultFormat
{
    /// <summary>
    /// Automatically choose format based on context
    /// </summary>
    Auto = 0,
    
    /// <summary>
    /// Minimal output - just the final result
    /// </summary>
    Terse = 1,
    
    /// <summary>
    /// Balanced output - result with basic breakdown
    /// </summary>
    Standard = 2,
    
    /// <summary>
    /// Rich output with full breakdown and metadata
    /// </summary>
    Detailed = 3,
    
    /// <summary>
    /// Story-style narrative output
    /// </summary>
    Narrative = 4,
    
    /// <summary>
    /// Technical debug output with AST and timing
    /// </summary>
    Technical = 5
}