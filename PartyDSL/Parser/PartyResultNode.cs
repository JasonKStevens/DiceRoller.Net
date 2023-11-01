using DiceRoller.Parser;

namespace PartyDSL.Parser
{
    public class PartyResultNode
    {
        public string Value { get; private set; }
        public string Breakdown { get; private set; }
        public TypedResult TypedResult { get; set; } = new TypedResult();

        public PartyResultNode(string value) : this(value, value.ToString())
        {
        }

        public PartyResultNode(string value, string breakdown, TypedResult typedResult = null)
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