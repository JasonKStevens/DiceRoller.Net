using System.Collections.Generic;

namespace DiceRoller.Parser
{
    public class DiceResultNode
    {
        public float Value { get; private set; }
        public string Breakdown { get; private set; }
        public TypedResult TypedResult { get; private set; } = new TypedResult();

        public DiceResultNode(int value) : this(value, value.ToString())
        {
        }

        public DiceResultNode(float value) : this(value, value.ToString())
        {
        }

        public DiceResultNode(float value, string breakdown, TypedResult typedResult = null)
        {
            Value = value;
            Breakdown = breakdown;
            TypedResult = typedResult;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum NodeType
    {
        None,
        DiceRoll,
        DiceRollTotal,
        Operator,
        Repeat,
        Lookup,
        Min,
        Text,
        StepFunc,
        Comment,
        StepFuncDef
    }

    public class TypedResult
    {
        public NodeType NodeType { get; set; }
        public string Text { get; set; }

        public List<TypedResult> SubText { get; set; } = new List<TypedResult>();

        public static TypedResult Null = new TypedResult();
        public static TypedResult NewUnnamedTripartComposite(NodeType nodeType, TypedResult leftText, TypedResult middleText, TypedResult rightText)
        {
            return new TypedResult()
            {
                NodeType = nodeType,
                SubText = new List<TypedResult>(3)
                {
                    leftText, middleText, rightText
                }
            };

        }

        public static TypedResult NewSimpleResult(string text)
        {
            return new TypedResult()
            {
                NodeType = NodeType.None,
                Text = text
            };

        }

        public static TypedResult NewSimpleResult(NodeType nodeType, string text)
        {
            return new TypedResult()
            {
                NodeType = nodeType,
                Text = text
            };

        }
    }
}