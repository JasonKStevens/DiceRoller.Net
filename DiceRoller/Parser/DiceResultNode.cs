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
}