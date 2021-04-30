using DiceRoller;
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

        public readonly PartyCommandEvaluator _evaluator;

        public PartyCommandPocessor(PartyCommandEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public (bool, string) Process(DiscordUserInfo userInfo, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, string.Empty);
 
            var tokens = commandText.Split(" ",StringSplitOptions.None);
            var prefix = tokens[0].ToLower();

            if ((prefix != Prefix) && (!_evaluator.HasParty(prefix.Replace("!",""))))
                return (false, string.Empty);

            var result = _evaluator.Evaluate(prefix.Replace("!",""), string.Join(' ', tokens, 1, tokens.Length-1));

            if (result.Value.Contains("```"))
                return (true, userInfo.DisplayName + ": " + result.Value);

            return (true, userInfo.DisplayName + ": " + FormatResultNode(userInfo, result));
        }

        private string FormatResultNode(DiscordUserInfo user, PartyResultNode node)
        {
            var builder = new StringBuilder();
            builder.Append("```"  +node.Value + "```");
            return builder.ToString();        
        }

        private string Emotify(string value)
        {
            return value
                    .Replace("0", ":zero:")
                    .Replace("1", ":one:")
                    .Replace("2", ":two:")
                    .Replace("3", ":three:")
                    .Replace("4", ":four:")
                    .Replace("5", ":five:")
                    .Replace("6", ":six:")
                    .Replace("7", ":seven:")
                    .Replace("8", ":eight:")
                    .Replace("9", ":nine:");
        }        
    }

}
