namespace DiceRoller.Parser
{
    public class DiceResultNode
    {
        public float Value { get; private set; }
        public string Breakdown { get; private set; }

        public DiceResultNode(int value) : this(value, value.ToString())
        {
        }

        public DiceResultNode(float value) : this(value, value.ToString())
        {
        }

        public DiceResultNode(float value, string breakdown)
        {
            Value = value;
            Breakdown = breakdown;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}