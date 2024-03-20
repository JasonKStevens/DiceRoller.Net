using DiceRoller.Parser;

namespace DiceRollerCmd;

public class SettingResultNode
{
    public string Value { get; private set; }
    public string Breakdown { get; private set; }
    public TypedResult TypedResult { get; private set; } = new TypedResult();

    public SettingResultNode(string value) : this(value, value, DiceRoller.Parser.TypedResult.NewSimpleResult(value))
    {
    }

    public SettingResultNode(string value, string breakdown, TypedResult typedResult = null)
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