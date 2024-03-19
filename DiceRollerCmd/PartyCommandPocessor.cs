using DiceRoller;
using DiceRoller.Parser;
using DiscordRollerBot;
using PartyDSL.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiceRollerCmd
{
    public class PartyCommandPocessor : ICommandProcessor
    {
        public string Prefix => "!party";
        public List<string> Prefixes = new List<string>() {"!party", "\\party"};

        public readonly PartyCommandEvaluator _evaluator;

        public PartyCommandPocessor(PartyCommandEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, TypedResult.Null);

            var tokens = commandText.Split(" ",StringSplitOptions.None);
            var prefix = tokens[0].ToLower();

            if (!Prefixes.Contains(prefix) && ( !_evaluator.HasParty(prefix.Replace("!", "")) ))
                return (false, TypedResult.Null);

            var result = _evaluator.Evaluate(prefix.Replace("!",""), string.Join(' ', tokens, 1, tokens.Length-1));

            return (true, result.TypedResult);
        }
    }

}
