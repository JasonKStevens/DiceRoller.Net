using System.Text;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Handles tree structure formatting with symbols and indentation
/// </summary>
public class TreeStructureFormatter
{
    private const string TREE_BRANCH = "├─ ";
    private const string TREE_LAST = "└─ ";
    private const string TREE_PIPE = "│  ";
    private const string TREE_SPACE = "   ";

    /// <summary>
    /// Creates a tree prefix for a child node
    /// </summary>
    /// <param name="parentPrefix">The prefix from the parent level</param>
    /// <param name="isLast">Whether this is the last child at this level</param>
    /// <returns>The prefix to use for this node</returns>
    public string CreateNodePrefix(string parentPrefix, bool isLast)
    {
        return parentPrefix + (isLast ? TREE_LAST : TREE_BRANCH);
    }

    /// <summary>
    /// Creates a prefix for children of a node
    /// </summary>
    /// <param name="parentPrefix">The prefix from the parent level</param>
    /// <param name="isLast">Whether the parent is the last child at its level</param>
    /// <returns>The prefix to use for children</returns>
    public string CreateChildPrefix(string parentPrefix, bool isLast)
    {
        return parentPrefix + (isLast ? TREE_SPACE : TREE_PIPE);
    }

    /// <summary>
    /// Formats a tree header with emoji
    /// </summary>
    /// <param name="title">The title text</param>
    /// <param name="emoji">The emoji to use (default: 🎲)</param>
    /// <returns>The formatted header</returns>
    public string FormatTreeHeader(string title, string emoji = "🎲")
    {
        return $"{emoji} {title}";
    }

    /// <summary>
    /// Formats a tree node line
    /// </summary>
    /// <param name="prefix">The tree prefix</param>
    /// <param name="content">The content to display</param>
    /// <returns>The formatted line</returns>
    public string FormatTreeNode(string prefix, string content)
    {
        return prefix + content;
    }

    /// <summary>
    /// Creates a complete tree structure from lines
    /// </summary>
    /// <param name="header">The tree header</param>
    /// <param name="lines">The content lines</param>
    /// <returns>The complete tree as a string</returns>
    public string BuildTree(string header, IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine(header);
        
        foreach (var line in lines)
        {
            sb.AppendLine(line);
        }
        
        return sb.ToString().TrimEnd();
    }
}