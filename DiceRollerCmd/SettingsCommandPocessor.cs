using DiceRoller.Parser;
using DiscordRollerBot;
using PartyDSL.Parser;
using System;
using System.Collections.Generic;

namespace DiceRollerCmd;

public class SettingsCommandPocessor : ICommandProcessor
{
    public string Prefix => "!settings";
    public List<string> Prefixes = new List<string>() {"!settings", "\\settings"};

    public readonly SettingsCommandEvaluator _evaluator;

    public SettingsCommandPocessor(SettingsCommandEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public (bool, TypedResult) ProcessTyped(string userId, string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText)) return (false, TypedResult.Null);

        var tokens = commandText.Split(" ",StringSplitOptions.None);
        var prefix = tokens[0].ToLower();

        if (!Prefixes.Contains(prefix))
            return (false, TypedResult.Null);

        var result = _evaluator.Evaluate(prefix.Replace("!",""), string.Join(' ', tokens, 1, tokens.Length-1), userId);

        return (true, result.TypedResult);
    }
}