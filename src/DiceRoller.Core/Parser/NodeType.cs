namespace DiceRoller.Core.Parser
{

public enum NodeType
{
    None,
    DiceRoll,
    DiceRollTotal,
    Operator,
    Repeat,
    Lookup,
    Min,
    Max,
    Text,
    StepFunc,
    Comment,
    StepFuncDef
}
}