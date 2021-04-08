using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiceRoller.Dice;
using Irony.Parsing;

namespace DiceRoller.Parser
{
    public class Visitor
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public Visitor(IRandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator;
        }

        public ResultNode Visit(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "number":
                    var value = Convert.ToSingle(node.Token.Text, CultureInfo.InvariantCulture.NumberFormat);
                    return new ResultNode(value);

                case "add":
                    return EvaluateOperation(node, (l, r) => l + r, "+");

                case "subtract":
                    return EvaluateOperation(node, (l, r) => l - r, "-");

                case "multiply":
                    return EvaluateOperation(node, (l, r) => l * r, "*");

                case "divide":
                    return EvaluateOperation(node, (l, r) => l / r, "/");

                case "min":
                    return EvaluateFuncOperation(node, (l, r) => (l < r) ? r : l, "min");

                case "repeat":
                    return EvaluateRepeatOperation(node);

                case "roll":
                    return EvaluateDiceExpression(node);
            }

            throw new InvalidOperationException($"Unrecognizable term {node.Term.Name}.");
        }

        private ResultNode EvaluateDiceExpression(ParseTreeNode node)
        {
            (var leftNode, var rightNode) = GetBinaryNodes(node);

            var count = (int) (leftNode?.Value ?? 1);
            var dice = (int) rightNode.Value;

            var postfix = node.ChildNodes.Count > 3 ? node.ChildNodes[3].Token.Text : null;
            var isExploding = postfix == "!";
            const int maxExplodingCount = 10;

            var roll = Enumerable.Range(0, count)
                .Select(_ => RollGenerator(dice, isExploding)
                    .TakeUntilInclusive((r, i) => !r.Exploded || i >= maxExplodingCount)
                )
                .SelectMany(r => r)
                .ToArray();

            var value = roll.Sum(r => r.Roll);
            var breakdown = $"[{string.Join(", ", roll.Select(r => r.ToString()))}]";
            return new ResultNode(value, breakdown);
        }

        private IEnumerable<DiceRoll> RollGenerator(int dice, bool isExploding)
        {
            while (true)
                yield return new DiceRoll(dice, _randomNumberGenerator.Next(dice) + 1, isExploding);
        }

        private ResultNode EvaluateOperation(ParseTreeNode node, Func<float, float, float> operation, string symbol)
        {
            (var leftNode, var rightNode) = GetBinaryNodes(node);

            var value = operation(leftNode.Value, rightNode.Value);
            return new ResultNode(value, $"{leftNode.Breakdown} {symbol} {rightNode.Breakdown}");
        }

        private ResultNode EvaluateRepeatOperation(ParseTreeNode node)
        {
            var leftNode = node.ChildNodes[1];
            var count = Visit(node.ChildNodes[3]).Value;

            var breakdown = new List<string>();
            float total = 0;
            for (int i = 0; i < count; i++)
            {
                var result = Visit(leftNode);
                breakdown.Add(result.Breakdown);
                total += result.Value;
            }

            return new ResultNode(total, String.Join(", ", breakdown.ToArray()));
        }

        private ResultNode EvaluateFuncOperation(ParseTreeNode node, Func<float, float, float> operation, string symbol)
        {
            var leftNode = Visit(node.ChildNodes[1]);
            var rightNode = Visit(node.ChildNodes[3]);

            var value = operation(leftNode.Value, rightNode.Value);
            
            return new ResultNode(value, $"{symbol}({leftNode.Breakdown}, {rightNode.Breakdown})");
        }

        private (ResultNode left, ResultNode right) GetBinaryNodes(ParseTreeNode node)
        {
            var isBinary = node.ChildNodes.Count >= 3;

            var leftNode = isBinary ? Visit(node.ChildNodes[0]) : null;
            var rightNode = isBinary ? Visit(node.ChildNodes[2]) : Visit(node.ChildNodes[1]);

            return (leftNode, rightNode);
        }

        private (TLeft left, TRight right) GetBinaryValues<TLeft, TRight>(ParseTreeNode node)
        {
            (var leftNode, var rightNode) = GetBinaryNodes(node);

            var leftValue = leftNode == null ? default : (TLeft) Convert.ChangeType(leftNode?.Value, typeof(TLeft));
            var rightValue = rightNode == null ? default : (TRight) Convert.ChangeType(rightNode?.Value, typeof(TRight));

            return (leftValue, rightValue);
        }
    }
}
