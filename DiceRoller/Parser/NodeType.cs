namespace DiceRoller.Parser;

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