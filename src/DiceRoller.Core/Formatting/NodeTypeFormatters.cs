using DiceRoller.Core.Parser;
using System.Text;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Handles formatting of different node types using strategy pattern
/// </summary>
public class NodeTypeFormatters
{
    private readonly TreeStructureFormatter _treeFormatter;
    private readonly BreakdownTextParser _textParser;
    private readonly DetailPreservingFormatter _detailFormatter;
    private Action<TypedResult, StringBuilder, string, bool, string>? _formatNodeCallback;

    public NodeTypeFormatters(TreeStructureFormatter treeFormatter, BreakdownTextParser textParser)
    {
        _treeFormatter = treeFormatter ?? throw new ArgumentNullException(nameof(treeFormatter));
        _textParser = textParser ?? throw new ArgumentNullException(nameof(textParser));
        _detailFormatter = new DetailPreservingFormatter(_textParser);
    }

    /// <summary>
    /// Sets the callback for formatting child nodes (to avoid circular dependencies)
    /// </summary>
    public void SetFormatNodeCallback(Action<TypedResult, StringBuilder, string, bool, string> formatNodeCallback)
    {
        _formatNodeCallback = formatNodeCallback;
    }

    /// <summary>
    /// Formats a repeat function node with its iterations
    /// </summary>
    public void FormatRepeatNode(TypedResult node, StringBuilder sb, string currentPrefix, string childPrefix, string fullBreakdown)
    {
        var iterationsCount = _textParser.ExtractIterationsCount(fullBreakdown);
        sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Repeat {iterationsCount} times"));
        
        // Format sub-nodes as iterations with preserved details
        if (node.SubText != null)
        {
            for (int i = 0; i < node.SubText.Count; i++)
            {
                var isLastIteration = i == node.SubText.Count - 1;
                var iterationPrefix = _treeFormatter.CreateNodePrefix(childPrefix, isLastIteration);
                var subNode = node.SubText[i];
                
                // Preserve calculation details for this iteration
                var detailedValue = _detailFormatter.PreserveIterationDetails(subNode.Text, fullBreakdown);
                
                sb.AppendLine(_treeFormatter.FormatTreeNode(iterationPrefix, $"Iteration {i + 1}: {detailedValue}"));
            }
        }
    }

    /// <summary>
    /// Formats a min function node
    /// </summary>
    public void FormatMinNode(TypedResult node, StringBuilder sb, string currentPrefix, string childPrefix, string fullBreakdown)
    {
        var minInfo = _textParser.ExtractMinInfo(fullBreakdown);
        if (minInfo.IsValid)
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"min({minInfo.Expression1}, {minInfo.Expression2}) = {minInfo.Result}"));
        }
        else
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"min function = {node.Text}"));
            FormatSubNodes(node, sb, childPrefix);
        }
    }

    /// <summary>
    /// Formats a dice roll node with exploding dice details
    /// </summary>
    public void FormatDiceRollNode(TypedResult node, StringBuilder sb, string currentPrefix, string fullBreakdown)
    {
        var diceInfo = _textParser.ExtractDiceRollInfo(node.Text, fullBreakdown);
        if (!string.IsNullOrEmpty(diceInfo))
        {
            // Further enhance with preserved exploding dice details
            var enhancedDiceInfo = _detailFormatter.PreserveExplodingDiceDetails(node.Text, fullBreakdown);
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, enhancedDiceInfo));
        }
        else
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Dice roll: {node.Text}"));
        }
    }

    /// <summary>
    /// Formats an operator node
    /// </summary>
    public void FormatOperatorNode(TypedResult node, StringBuilder sb, string currentPrefix, string childPrefix)
    {
        // Check if this is a comparison operation by looking at sub-nodes
        if (node.SubText != null && node.SubText.Count == 3)
        {
            var left = node.SubText[0];
            var op = node.SubText[1];
            var right = node.SubText[2];
            
            // Check if the operator is a comparison operator
            if (op.Text == ">" || op.Text == "<" || op.Text == ">=" || op.Text == "<=")
            {
                var result = node.Text == "1" ? "True" : "False";
                sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Comparison: {left.Text} {op.Text} {right.Text} = {result}"));
                return;
            }
        }
        
        sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Operation: {node.Text}"));
        FormatSubNodes(node, sb, childPrefix);
    }

    /// <summary>
    /// Formats a lookup node (comparisons, game tables)
    /// </summary>
    public void FormatLookupNode(TypedResult node, StringBuilder sb, string currentPrefix, string fullBreakdown)
    {
        var compInfo = _textParser.ExtractComparisonInfo(fullBreakdown);
        if (compInfo.IsValid)
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Comparison: {compInfo.Expression} = {compInfo.Result}"));
        }
        else
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Lookup: {node.Text}"));
        }
    }

    /// <summary>
    /// Formats a step function node
    /// </summary>
    public void FormatStepFunctionNode(TypedResult node, StringBuilder sb, string currentPrefix, string childPrefix)
    {
        sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, $"Step function: {node.Text}"));
        FormatSubNodes(node, sb, childPrefix);
    }

    /// <summary>
    /// Formats a generic node type
    /// </summary>
    public void FormatGenericNode(TypedResult node, StringBuilder sb, string currentPrefix, string childPrefix)
    {
        if (!string.IsNullOrEmpty(node.Text))
        {
            sb.AppendLine(_treeFormatter.FormatTreeNode(currentPrefix, node.Text));
        }
        FormatSubNodes(node, sb, childPrefix);
    }

    /// <summary>
    /// Formats all sub-nodes of a given node
    /// </summary>
    public void FormatSubNodes(TypedResult node, StringBuilder sb, string childPrefix)
    {
        if (node.SubText == null || node.SubText.Count == 0)
            return;

        for (int i = 0; i < node.SubText.Count; i++)
        {
            var isLast = i == node.SubText.Count - 1;
            _formatNodeCallback?.Invoke(node.SubText[i], sb, childPrefix, isLast, "");
        }
    }
}