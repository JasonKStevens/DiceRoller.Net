using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiceRoller.Dice;
using DiceRoller.DragonQuest;
using Irony.Parsing;

namespace DiceRoller.Parser
{
    public class DiceRollVisitor
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly Func<int, DiceResultNode> _stepEvalFunc;

        public DiceRollVisitor(IRandomNumberGenerator randomNumberGenerator, Func<int, DiceResultNode> stepEvalFunc)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _stepEvalFunc = stepEvalFunc;
        }

        public DiceResultNode Visit(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "number":
                    var value = Convert.ToSingle(node.Token.Text, CultureInfo.InvariantCulture.NumberFormat);
                    return new DiceResultNode(value);

                case "add":
                    return EvaluateOperation(node, (l, r) => l + r, "+");

                case "subtract":
                    return EvaluateOperation(node, (l, r) => l - r, "-");

                case "multiply":
                    return EvaluateOperation(node, (l, r) => l * r, "*");

                case "divide":
                    return EvaluateOperation(node, (l, r) => l / r, "/");

                case "less-than":
                    return EvaluateOperation(node, (l, r) => l < r ? 1 : 0, "<");

                case "less-than-or-equal-to":
                    return EvaluateOperation(node, (l, r) => l <= r ? 1 : 0, "<=");

                case "equal-to":
                    return EvaluateOperation(node, (l, r) => l == r ? 1 : 0, "=");

                case "greater-than-or-equal-to":
                    return EvaluateOperation(node, (l, r) => l >= r ? 1 : 0, ">=");

                case "greater-than":
                    return EvaluateOperation(node, (l, r) => l > r ? 1 : 0, ">");

                case "min":
                    return EvaluateFuncOperation(node, (l, r) => (l < r) ? r : l, "min");

                case "repeat":
                    return EvaluateRepeatOperation(node);

                case "step":
                    return EvaluateStepOperation(node);

                case "roll":
                    return EvaluateDiceExpression(node);

                case "expression":
                case "numeric-expression":
                    return Visit(node.ChildNodes[0]);
            }

            throw new InvalidOperationException($"Unrecognizable term '{node.Term.Name}'.");
        }

        private DiceResultNode EvaluateDiceExpression(ParseTreeNode node)
        {
            (var leftNode, var rightNode) = GetDiceNodes(node);

            var count = (int) (leftNode?.Value ?? 1);
            var dice = (int) rightNode.Value;

            var isExploding = node.ChildNodes.Last()?.Token?.Text == "!";
            const int maxExplodingCount = 10;

            var roll = Enumerable.Range(0, count)
                .Select(_ => RollGenerator(dice, isExploding)
                    .TakeUntilInclusive((r, i) => !r.Exploded || i >= maxExplodingCount)
                )
                .SelectMany(r => r)
                .ToArray();

            var value = roll.Sum(r => r.Roll);
            var breakdown = $"[{string.Join(", ", roll.Select(r => r.ToString()))}]";
            return new DiceResultNode(value, breakdown);
        }

        private IEnumerable<DiceRoll> RollGenerator(int dice, bool isExploding)
        {
            while (true)
                yield return new DiceRoll(dice, _randomNumberGenerator.Next(dice) + 1, isExploding);
        }

        private DiceResultNode EvaluateOperation(ParseTreeNode node, Func<float, float, float> operation, string symbol)
        {
            (var leftNode, var rightNode) = GetBinaryNodes(node);

            var value = operation(leftNode?.Value ?? 0, rightNode?.Value ?? 0);
            return new DiceResultNode(value, $"{leftNode?.Breakdown ?? ""} {symbol} {rightNode?.Breakdown ?? ""}");
        }

        private DiceResultNode EvaluateRepeatOperation(ParseTreeNode node)
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

            return new DiceResultNode(total, "(" + String.Join(", ", breakdown.ToArray()) + ")");
        }

        private DiceResultNode Generate(LookupTable lookupTable)
        {
            var diceRoll = RollGenerator(100, isExploding: false).First();
            var injury = lookupTable.LookupResult(diceRoll.Roll);
            return new DiceResultNode(diceRoll.Roll, injury);
        }

        private DiceResultNode EvaluateFuncOperation(ParseTreeNode node, Func<float, float, float> operation, string symbol)
        {
            var leftNode = Visit(node.ChildNodes[1]);
            var rightNode = Visit(node.ChildNodes[3]);

            var value = operation(leftNode.Value, rightNode.Value);
            
            return new DiceResultNode(value, $"{symbol}({leftNode.Breakdown}, {rightNode.Breakdown}) => **{value}**");
        }




        private DiceResultNode EvaluateStepOperation(ParseTreeNode node)
        {
            var stepValue = Visit(node.ChildNodes[1]);

            var result = _stepEvalFunc((int) stepValue.Value);

           
            return new DiceResultNode(result.Value, $"{result.Breakdown}");
        }        

        private (DiceResultNode left, DiceResultNode right) GetBinaryNodes(ParseTreeNode node)
        {
            var isBinary = node.ChildNodes.Count >= 3;

            var leftNode = isBinary ? Visit(node.ChildNodes[0]) : null;
            var rightNode = isBinary ? Visit(node.ChildNodes[2]) : Visit(node.ChildNodes[1]);

            return (leftNode, rightNode);
        }

        private (DiceResultNode left, DiceResultNode right) GetDiceNodes(ParseTreeNode node)
        {
            var isUnary = node.ChildNodes[0].ToString() == "dice";

            var leftNode = isUnary ? null : Visit(node.ChildNodes[0]);
            var rightNode = isUnary ? Visit(node.ChildNodes[1]) : Visit(node.ChildNodes[2]);

            return (leftNode, rightNode);
        }
    }
}
