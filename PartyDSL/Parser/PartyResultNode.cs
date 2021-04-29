namespace PartyDSL.Parser
{
    public class PartyResultNode
    {
        public string Value { get; private set; }
        public string Breakdown { get; private set; }

        public PartyResultNode(string value) : this(value, value.ToString())
        {
        }

        public PartyResultNode(string value, string breakdown)
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