using DiceRoller.Core.Parser;
using System.Text;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Enhanced text formatter that creates hierarchical output with tree symbols
/// </summary>
public class EnhancedTextFormatter : IAdaptiveBreakdownFormatter
{
    private readonly TreeStructureFormatter _treeFormatter;
    private readonly BreakdownTextParser _textParser;
    private readonly NodeTypeFormatters _nodeFormatters;

    public EnhancedTextFormatter()
    {
        _treeFormatter = new TreeStructureFormatter();
        _textParser = new BreakdownTextParser();
        _nodeFormatters = new NodeTypeFormatters(_treeFormatter, _textParser);
        _nodeFormatters.SetFormatNodeCallback(FormatNode);
    }

    /// <summary>
    /// Formats a dice expression breakdown with hierarchical tree structure
    /// </summary>
    public string FormatBreakdown(TypedResult typedResult, string breakdown, string originalExpression)
    {
        if (typedResult == null || typedResult == TypedResult.Null)
            return FormatFallbackBreakdown(breakdown);

        var sb = new StringBuilder();
        
        // Add main expression header
        var expressionName = _textParser.ExtractExpressionName(originalExpression);
        var header = _treeFormatter.FormatTreeHeader(expressionName);
        sb.AppendLine(header);
        
        // Format the tree structure
        FormatNode(typedResult, sb, "", true, breakdown);
        
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Recursively formats a TypedResult node as part of a tree structure
    /// </summary>
    private void FormatNode(TypedResult node, StringBuilder sb, string prefix, bool isLast, string fullBreakdown)
    {
        if (node == null || node == TypedResult.Null)
            return;

        var currentPrefix = _treeFormatter.CreateNodePrefix(prefix, isLast);
        var childPrefix = _treeFormatter.CreateChildPrefix(prefix, isLast);

        switch (node.NodeType)
        {
            case NodeType.Repeat:
                _nodeFormatters.FormatRepeatNode(node, sb, currentPrefix, childPrefix, fullBreakdown);
                break;
                
            case NodeType.Min:
                _nodeFormatters.FormatMinNode(node, sb, currentPrefix, childPrefix, fullBreakdown);
                break;
                
            case NodeType.DiceRoll:
                _nodeFormatters.FormatDiceRollNode(node, sb, currentPrefix, fullBreakdown);
                break;
                
            case NodeType.Operator:
                _nodeFormatters.FormatOperatorNode(node, sb, currentPrefix, childPrefix);
                break;
                
            case NodeType.Lookup:
                _nodeFormatters.FormatLookupNode(node, sb, currentPrefix, fullBreakdown);
                break;
                
            case NodeType.StepFunc:
            case NodeType.StepFuncDef:
                _nodeFormatters.FormatStepFunctionNode(node, sb, currentPrefix, childPrefix);
                break;
                
            default:
                _nodeFormatters.FormatGenericNode(node, sb, currentPrefix, childPrefix);
                break;
        }
    }


    /// <summary>
    /// Fallback formatting when TypedResult is not available
    /// </summary>
    private string FormatFallbackBreakdown(string breakdown)
    {
        if (string.IsNullOrEmpty(breakdown))
            return "`No breakdown available`";

        return $"```\n{breakdown}\n```";
    }

}