using System;
using System.Collections.Generic;
using DiceRoller;
using DiceRoller.DragonQuest;
using DiceRoller.Parser;
using DiscordRollerBot;

namespace DiceRollerCmd
{
    public class DQCommandProcessor : ICommandProcessor
    {
        private readonly DiceRollEvaluator _evaluator;
        public string Prefix => "!dq";
        public List<string> Prefixes = new List<string>() {"!dq", "\\dq"};

        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;

        public DQCommandProcessor(DiceRollEvaluator evaluator, DQLookupTables tables)
        {
            _evaluator = evaluator;
            _injuries = tables.Injuries;
            _backfires = tables.Backfires;
            _fears = tables.Fear;
        }


        public (bool, string) Process(DiscordUserInfo userInfo, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, string.Empty);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!Prefixes.Contains(tokens[0]))
                return (false, null);

            //what are we dealing with
            switch (tokens[1].ToLower())
            {
                case "backfire": 
                    return (true, LookupResult(_backfires, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "specgrev":
                case "injury":
                    return (true, LookupResult(_injuries, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "fear":
                    return (true, LookupResult(_fears, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "help":
                    return (true, Constants.GetHelpText());
                    
                default:
                    return (false, null);
            }
        }

        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, null);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!Prefixes.Contains(tokens[0]))
                return (false, null);

            //what are we dealing with
            switch (tokens[1].ToLower())
            {
                case "backfire":
                    return (true, LookupTypedResult(_backfires, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "specgrev":
                case "injury":
                    return (true, LookupTypedResult(_injuries, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "fear":
                    return (true, LookupTypedResult(_fears, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "help":
                    return (true, TypedResult.NewSimpleResult(Constants.GetHelpText()));

                default:
                    return (false, TypedResult.Null);
            }
        }

        private string LookupResult(LookupTable table, string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                instruction = table.GetRoll();

            int roll = ( int ) _evaluator.Evaluate(instruction).Value;

            int iRoll = Convert.ToInt32(roll);
            return "__**" + roll + "**__" +Environment.NewLine + "```styl" + Environment.NewLine + table.LookupResult(iRoll) + "```";
        }

        private TypedResult LookupTypedResult(LookupTable table, string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                instruction = table.GetRoll();

            int iRoll = ( int ) _evaluator.Evaluate(instruction).Value;

            var typedResult = new TypedResult(){ NodeType = NodeType.Lookup, Text = iRoll.ToString()};
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, iRoll.ToString()));
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.None, table.LookupResult(iRoll)));

            return typedResult;
        }
    }
}
