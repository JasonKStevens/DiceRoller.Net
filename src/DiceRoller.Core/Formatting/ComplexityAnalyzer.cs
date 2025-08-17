using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Analyzes the complexity of dice expressions for adaptive formatting
/// </summary>
public class ComplexityAnalyzer : IComplexityAnalyzer
{
    private readonly Dictionary<NodeType, int> _nodeTypeComplexityMap;
    private readonly ComplexityThresholdOptions _thresholds;

    public ComplexityAnalyzer(ComplexityThresholdOptions thresholds = null)
    {
        _thresholds = thresholds ?? new ComplexityThresholdOptions();
        _thresholds.Validate();
        
        _nodeTypeComplexityMap = new Dictionary<NodeType, int>
        {
            { NodeType.None, 0 },
            { NodeType.Text, 0 },
            { NodeType.Comment, 0 },
            { NodeType.DiceRoll, 1 },
            { NodeType.DiceRollTotal, 1 },
            { NodeType.Operator, 1 },
            { NodeType.Lookup, 1 },
            { NodeType.Min, 2 },
            { NodeType.Max, 2 },
            { NodeType.StepFunc, 2 },
            { NodeType.StepFuncDef, 2 },
            { NodeType.Repeat, 4 } // Adjusted to balance complexity - repeat is significant but not overwhelming
        };
    }
    /// <summary>
    /// Analyzes the complexity of a TypedResult structure
    /// </summary>
    /// <param name="typedResult">The TypedResult to analyze</param>
    /// <returns>The complexity level of the expression</returns>
    public ComplexityLevel AnalyzeComplexity(TypedResult typedResult)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
            return ComplexityLevel.Simple;

        var complexity = CalculateComplexityScore(typedResult, 0);
        
        return complexity switch
        {
            var c when c <= _thresholds.SimpleThreshold => ComplexityLevel.Simple,
            var c when c <= _thresholds.ModerateThreshold => ComplexityLevel.Moderate,
            var c when c <= _thresholds.ComplexThreshold => ComplexityLevel.Complex,
            var c when c <= _thresholds.VeryComplexThreshold => ComplexityLevel.VeryComplex,
            _ => ComplexityLevel.Extreme
        };
    }

    /// <summary>
    /// Calculates a numeric complexity score for the TypedResult tree
    /// </summary>
    private int CalculateComplexityScore(TypedResult node, int depth)
    {
        if (node == null || node == TypedResult.Null)
            return 0;

        int score = 0;

        // Base complexity score based on node type
        score += GetNodeTypeComplexity(node.NodeType);

        // Lighter depth penalty (deeper nesting increases complexity slightly)
        score += depth / 2;

        // Recursively calculate complexity for sub-nodes
        // BUT: Don't count individual dice in a dice roll as complexity
        if (node.SubText != null)
        {
            // Special handling for dice rolls - don't count individual dice as complexity
            if (node.NodeType == NodeType.DiceRoll || node.NodeType == NodeType.DiceRollTotal)
            {
                // For dice rolls, just add a small fixed amount regardless of dice count
                // This prevents 20d100 from being scored as complex
                if (node.SubText.Count > 10)
                {
                    score += 1; // Small penalty for very large dice pools
                }
            }
            else
            {
                // For non-dice nodes, recursively calculate complexity
                foreach (var subNode in node.SubText)
                {
                    score += CalculateComplexityScore(subNode, depth + 1);
                }

                // Light penalty for having many sub-nodes (but not for dice rolls)
                if (node.SubText.Count > 3)
                {
                    score += (node.SubText.Count - 3) / 2; // Reduced penalty
                }
            }
        }

        // Special handling for certain patterns
        score += GetPatternComplexity(node);

        // Apply repeat complexity scaling - repeats should scale more gradually
        if (node.NodeType == NodeType.Repeat && node.SubText != null)
        {
            // Instead of multiplying by iteration count, add a scaling factor
            var iterationCount = node.SubText.Count;
            if (iterationCount > 5)
            {
                score += (iterationCount - 5) / 3; // Add complexity for large iteration counts more gradually
            }
            
            // Check if any sub-nodes contain exploding dice - this adds complexity
            foreach (var subNode in node.SubText)
            {
                if (subNode.Text?.Contains('!') == true)
                {
                    score += 1; // Each exploding dice iteration adds complexity
                    break; // Only count this bonus once per repeat node
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Gets the base complexity score for a node type
    /// </summary>
    private int GetNodeTypeComplexity(NodeType nodeType)
    {
        return _nodeTypeComplexityMap.TryGetValue(nodeType, out var complexity) ? complexity : 1;
    }

    /// <summary>
    /// Gets additional complexity based on text patterns
    /// </summary>
    private int GetPatternComplexity(TypedResult node)
    {
        if (string.IsNullOrEmpty(node.Text))
            return 0;

        int score = 0;

        // Only add complexity for exploding dice if there are other complexities
        // Simple exploding dice like 20d100! shouldn't be considered complex
        bool hasExplodingDice = node.Text.Contains('!');

        // Comparisons add complexity
        if (node.Text.Contains('>') || node.Text.Contains('<') || 
            node.Text.Contains(">=") || node.Text.Contains("<="))
            score += 1;

        // Multiple operators in the same text
        var operatorCount = CountOperators(node.Text);
        if (operatorCount > 1)
            score += operatorCount - 1;

        // Large dice pools are already handled in CalculateComplexityScore
        // No need to add extra complexity here
        
        // Only add exploding dice complexity if there are other complex features
        // This prevents simple expressions like 20d100! from being marked complex
        if (hasExplodingDice && (score > 0 || node.SubText?.Count > 0))
            score += 2; // Increased from 1 to 2 to ensure repeat+exploding is Complex

        return score;
    }

    /// <summary>
    /// Counts the number of operators in text
    /// </summary>
    private int CountOperators(string text)
    {
        int count = 0;
        var operators = new[] { '+', '-', '*', '/', '(', ')' };
        
        foreach (char c in text)
        {
            if (operators.Contains(c))
                count++;
        }
        
        return count;
    }

}