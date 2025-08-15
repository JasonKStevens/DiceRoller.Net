namespace DiceRoller.Core.Formatting;

/// <summary>
/// Configuration options for complexity analysis thresholds
/// </summary>
public class ComplexityThresholdOptions
{
    /// <summary>
    /// Threshold for Simple complexity level (default: 3)
    /// </summary>
    public int SimpleThreshold { get; set; } = 3;

    /// <summary>
    /// Threshold for Moderate complexity level (default: 6)
    /// </summary>
    public int ModerateThreshold { get; set; } = 6;

    /// <summary>
    /// Threshold for Complex complexity level (default: 10)
    /// </summary>
    public int ComplexThreshold { get; set; } = 10;

    /// <summary>
    /// Threshold for VeryComplex complexity level (default: 15)
    /// </summary>
    public int VeryComplexThreshold { get; set; } = 15;

    /// <summary>
    /// Validates that thresholds are in ascending order
    /// </summary>
    public void Validate()
    {
        if (SimpleThreshold >= ModerateThreshold)
            throw new InvalidOperationException("SimpleThreshold must be less than ModerateThreshold");

        if (ModerateThreshold >= ComplexThreshold)
            throw new InvalidOperationException("ModerateThreshold must be less than ComplexThreshold");

        if (ComplexThreshold >= VeryComplexThreshold)
            throw new InvalidOperationException("ComplexThreshold must be less than VeryComplexThreshold");

        if (SimpleThreshold < 1)
            throw new InvalidOperationException("SimpleThreshold must be at least 1");
    }
}