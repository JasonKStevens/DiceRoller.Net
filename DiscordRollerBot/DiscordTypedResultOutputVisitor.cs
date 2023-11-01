using DiceRoller;
using DiceRoller.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace DiscordRollerBot
{
    public class DiscordTypedResultOutputVisitor : ITypedResultOutputVisitor
    {
        private int _maxDepth = 1;

        public DiscordTypedResultOutputVisitor(int maxDepth)
        {
            _maxDepth = maxDepth;
        }

        private string Emotify(float value)
        {
            return value.ToString()
                //                            .Replace("-", ":traffic_light:")
                .Replace("-", "**--** ")
                .Replace("0", ":zero:")
                .Replace("1", ":one:")
                .Replace("2", ":two:")
                .Replace("3", ":three:")
                .Replace("4", ":four:")
                .Replace("5", ":five:")
                .Replace("6", ":six:")
                .Replace("7", ":seven:")
                .Replace("8", ":eight:")
                .Replace("9", ":nine:");
        }

        public string Visit(TypedResult result, int depth)
        {
            if (( result == null ) || ( result == TypedResult.Null )) return "";

            var output = new StringBuilder();

            switch (result.NodeType)
            {
                case NodeType.DiceRoll:
                    output.Append(result.Text);
                    break;
                case NodeType.DiceRollTotal:
                    if (( depth != _maxDepth ) && ( result.SubText.Where(x => x.NodeType != NodeType.Comment).Count() > 1 ))
                        output.Append("[" + VisitSubTexts(result.SubText, depth + 1, ", ") + "]");
                    else
                        output.Append(result.Text);
                    break;
                case NodeType.Operator:
                    if (( depth == _maxDepth ) || ( !result.SubText.Any() )) 
                        output.Append(result.Text);
                    else
                        output.Append(VisitOperatorSubTexts(result, depth));
                    break;
                case NodeType.Repeat:
                    if (( depth == _maxDepth ) || ( !result.SubText.Any() )) 
                        output.Append(result.Text);
                    else
                        output.Append("{" + VisitSubTexts(result.SubText, depth + 1, ", ") + "}");
                    break;
                case NodeType.Lookup:
                    if (( depth == _maxDepth ) || ( !result.SubText.Any() )) 
                        output.Append(result.Text);
                    else
                        output.Append("```styl" + Environment.NewLine + VisitSubTexts(result.SubText, depth + 1, " - ") + "```");
                    break;
                case NodeType.StepFunc:
                    if (( depth != _maxDepth ) && ( result.SubText.Where(x => x.NodeType != NodeType.Comment).Count() > 0 ))
                        output.Append("[" + VisitSubTexts(result.SubText, depth + 1, ", ") + "]");
                    else
                        output.Append(result.Text);
                    break;
                case NodeType.Min:
                    if (( depth != _maxDepth ) && ( result.SubText.Where(x => x.NodeType != NodeType.Comment).Count() > 0 ))
                        output.Append("[" + VisitSubTexts(result.SubText, depth + 1, ", ") + "]");
                    else
                        output.Append(result.Text);
                    break;
                case NodeType.None:
                default:
                    if (( depth == _maxDepth ) || ( !result.SubText.Any() )) 
                        output.Append(result.Text);
                    else
                        output.Append(VisitSubTexts(result.SubText, depth + 1));
                    break;
            }

            if (depth == 1)
            {
                var addSpoiler = output.Length > 100 && (result.NodeType != NodeType.Lookup);
                if (addSpoiler)
                {
                    output.Insert(0, "||");
                    output.Append("||");
                }

                float val;
                if (float.TryParse(result.Text, out val))
                {
                    var sep = " ";

                    var commentNode = result.SubText.FirstOrDefault(x => x.NodeType == NodeType.Comment);
                    string comment = "";
                    string preComment = "";
                    if (commentNode != null)
                        comment = " " + commentNode.Text;

                    if (addSpoiler)
                    {
                        sep = Environment.NewLine;
                        preComment = " " + comment;
                        comment = "";
                    }
                    


                    output.Insert(0,Emotify(val) + preComment + sep + "Reason: ");
                    if (comment.Length > 0) output.Append(comment);
                }
            }


            return output.ToString();
        }

        private string VisitSubTexts(List<TypedResult> results, int depth, string separator = "")
        {
            var result = new StringBuilder();

            foreach (var subText in results)
            {
                if (subText.NodeType == NodeType.Comment) continue;
                result.Append(Visit(subText, depth));
                result.Append(separator);
            }

            if ((result.Length > 0 ) && (separator.Length > 0))
            {
                //remove last character
                result.Remove(result.Length - separator.Length, separator.Length);
            }

            return result.ToString();
        }

        private List<string> _booleanOperations = new List<string>()
        {
            "<","<=","==",">=",">"
        };
        private string VisitOperatorSubTexts(TypedResult typedResult, int depth)
        {
            var result = new StringBuilder();

            var operation = typedResult.SubText[1];
            var isBoolOp = _booleanOperations.Contains(operation.Text);
            if (typedResult.Text == "1" && isBoolOp)
            {
                result.Append(DiscordFormattingStrings.EmphasizeValue(VisitSubTexts(typedResult.SubText, depth + 1)));
                return result.ToString();
            }

            if (!isBoolOp)
            {
                if (typedResult.SubText.Any(x => x.SubText.Any()))
                    return VisitSubTexts(typedResult.SubText, depth + 1);

                result.Append(typedResult.Text);
                return result.ToString();
            }
            else
                return VisitSubTexts(typedResult.SubText, depth + 1);
        }
    }

    public static class DiscordFormattingStrings
    {
        public static string EmphasizeValue(string value) => $" **__[{value}]__** ";
    }
}