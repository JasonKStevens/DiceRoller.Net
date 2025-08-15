namespace DiceRoller.Core.Formatting;

/// <summary>
/// Represents the complexity level of a dice expression for adaptive formatting
/// </summary>
public enum ComplexityLevel
{
    /// <summary>
    /// Simple expressions (single dice, basic arithmetic)
    /// Example: d20, 3d6+2
    /// </summary>
    Simple = 0,
    
    /// <summary>
    /// Moderate expressions (multiple dice groups, simple functions)
    /// Example: 2d8+1d6, min(3d6, 15)
    /// </summary>
    Moderate = 1,
    
    /// <summary>
    /// Complex expressions (nested functions, comparisons)
    /// Example: repeat(d20, 3), 3d6 > 12
    /// </summary>
    Complex = 2,
    
    /// <summary>
    /// Very complex expressions (multiple nesting levels, exploding dice)
    /// Example: repeat(min(3d6!+2, 20), 4)
    /// </summary>
    VeryComplex = 3,
    
    /// <summary>
    /// Extremely complex expressions (deep nesting, multiple complex operations)
    /// Example: repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10
    /// </summary>
    Extreme = 4
}