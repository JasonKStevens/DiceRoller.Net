using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Parser;
using Irony.Parsing;

namespace DiceRoller.Core.Functions
{
    /// <summary>
    /// Contains implementations of built-in dice functions
    /// </summary>
    public static class BuiltInFunctions
    {
        /// <summary>
        /// Gets all built-in functions
        /// </summary>
        public static IEnumerable<IDiceFunction> GetBuiltInFunctions()
        {
            yield return new MinFunction();
            yield return new MaxFunction();
            yield return new RepeatFunction();
        }

        /// <summary>
        /// Implementation of the min function
        /// </summary>
        public class MinFunction : IDiceFunction
        {
            public string Name => "min";

            public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
            {
                var leftNode = context.EvaluateNode(node.ChildNodes[1]);
                var rightNode = context.EvaluateNode(node.ChildNodes[3]);

                var value = Math.Min(leftNode.Value, rightNode.Value);
                var rankedText = new TypedResult { NodeType = NodeType.Min, Text = value.ToString() };
                rankedText.SubText.Add(leftNode.TypedResult);
                rankedText.SubText.Add(rightNode.TypedResult);

                return new DiceResultNode(value, $"min({leftNode.Breakdown}, {rightNode.Breakdown}) => **{value}**", rankedText);
            }
        }

        /// <summary>
        /// Implementation of the max function
        /// </summary>
        public class MaxFunction : IDiceFunction
        {
            public string Name => "max";

            public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
            {
                var leftNode = context.EvaluateNode(node.ChildNodes[1]);
                var rightNode = context.EvaluateNode(node.ChildNodes[3]);

                var value = Math.Max(leftNode.Value, rightNode.Value);
                var rankedText = new TypedResult { NodeType = NodeType.Max, Text = value.ToString() };
                rankedText.SubText.Add(leftNode.TypedResult);
                rankedText.SubText.Add(rightNode.TypedResult);

                return new DiceResultNode(value, $"max({leftNode.Breakdown}, {rightNode.Breakdown}) => **{value}**", rankedText);
            }
        }

        /// <summary>
        /// Implementation of the repeat function
        /// </summary>
        public class RepeatFunction : IDiceFunction
        {
            public string Name => "repeat";

            public DiceResultNode Execute(ParseTreeNode node, IFunctionContext context)
            {
                var leftNode = node.ChildNodes[1];
                var count = context.EvaluateNode(node.ChildNodes[3]).Value;

                var breakdown = new List<string>();
                var rankedText = new TypedResult { NodeType = NodeType.Repeat };
                float total = 0;
                
                for (int i = 0; i < count; i++)
                {
                    var result = context.EvaluateNode(leftNode);
                    breakdown.Add(result.Breakdown);
                    rankedText.SubText.Add(result.TypedResult);
                    total += result.Value;
                }
                
                rankedText.Text = total.ToString();

                // Include "repeat" prefix if any breakdown contains complex expressions or nested structures
                var hasComplexBreakdown = breakdown.Any(b => b.Contains("(") || b.Contains("min") || b.Contains("step"));
                var breakdownText = hasComplexBreakdown 
                    ? "repeat(" + String.Join(", ", breakdown.ToArray()) + ")"
                    : "(" + String.Join(", ", breakdown.ToArray()) + ")";

                return new DiceResultNode(total, breakdownText, rankedText);
            }
        }
    }
}