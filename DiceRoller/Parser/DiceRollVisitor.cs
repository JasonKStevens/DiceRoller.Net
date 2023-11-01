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
                    return new DiceResultNode(value, value.ToString(),new TypedResult(){NodeType = 0, Text = node.Token.Text});

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
                    return EvaluateMinOperation(node, (l, r) => (l < r) ? r : l, "min");

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
            var rankedText = new TypedResult
            {
                NodeType = NodeType.DiceRollTotal, 
                Text = value.ToString(), 
                SubText = roll.Select(r => new TypedResult() {NodeType = NodeType.DiceRoll, Text = r.ToString()}).ToList()
            };
            return new DiceResultNode(value, breakdown, rankedText);
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
            var rankedText = TypedResult.NewUnnamedTripartComposite(NodeType.Operator, leftNode?.TypedResult, new TypedResult() {NodeType = NodeType.None, Text = symbol}, rightNode?.TypedResult);
            rankedText.Text = value.ToString();

            return new DiceResultNode(value, $"{leftNode?.Breakdown ?? ""} {symbol} {rightNode?.Breakdown ?? ""}", rankedText);
        }


        private DiceResultNode EvaluateRepeatOperation(ParseTreeNode node)
        {
            var leftNode = node.ChildNodes[1];
            var count = Visit(node.ChildNodes[3]).Value;

            var breakdown = new List<string>();
            var rankedText = new TypedResult(){ NodeType = NodeType.Repeat};
            float total = 0;
            for (int i = 0; i < count; i++)
            {
                var result = Visit(leftNode);
                breakdown.Add(result.Breakdown);
                rankedText.SubText.Add(result.TypedResult);
                total += result.Value;
            }
            rankedText.Text = total.ToString();

            return new DiceResultNode(total, "(" + String.Join(", ", breakdown.ToArray()) + ")", rankedText);
        }

        private DiceResultNode Generate(LookupTable lookupTable)
        {
            var diceRoll = RollGenerator(100, isExploding: false).First();
            var injury = lookupTable.LookupResult(diceRoll.Roll);

            var rankedText = TypedResult.NewSimpleResult(NodeType.Lookup, injury);

            return new DiceResultNode(diceRoll.Roll, injury, rankedText);
        }

        private DiceResultNode EvaluateMinOperation(ParseTreeNode node, Func<float, float, float> operation, string symbol)
        {
            var leftNode = Visit(node.ChildNodes[1]);
            var rightNode = Visit(node.ChildNodes[3]);

            var value = operation(leftNode.Value, rightNode.Value);
            var rankedText = new TypedResult(){ NodeType = NodeType.Min, Text = value.ToString()};
            rankedText.SubText.Add(leftNode.TypedResult);
            rankedText.SubText.Add(rightNode.TypedResult);


            return new DiceResultNode(value, $"{symbol}({leftNode.Breakdown}, {rightNode.Breakdown}) => **{value}**", rankedText);
        }




        private DiceResultNode EvaluateStepOperation(ParseTreeNode node)
        {
            var stepValue = Visit(node.ChildNodes[1]);

            var result = _stepEvalFunc((int) stepValue.Value);

            var rankedText = new TypedResult(){ NodeType = NodeType.StepFunc, Text = result.Value.ToString()};
            rankedText.SubText.Add(result.TypedResult);



            return new DiceResultNode(result.Value, $"{result.Breakdown}", rankedText);
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
