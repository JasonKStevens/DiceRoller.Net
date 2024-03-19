using System;
using DiceRoller;
using DiceRoller.DragonQuest;
using DiceRoller.Parser;
using DiscordRollerBot;

namespace DiceRollerCmd
{
    public class DQCommandProcessor : ICommandProcessor
    {
        public string Prefix => "!dq";

        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;

        public DQCommandProcessor(DQLookupTables tables)
        {
            _injuries = tables.Injuries;
            _backfires = tables.Backfires;
            _fears = tables.Fear;
        }

        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, null);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!tokens[0].Equals(Prefix, StringComparison.InvariantCultureIgnoreCase))
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

        private TypedResult LookupTypedResult(LookupTable table, string roll)
        {
            if (string.IsNullOrWhiteSpace(roll))
                return TypedResult.NewSimpleResult("No roll was specified!");

            int iRoll = Convert.ToInt32(roll);

            var typedResult = new TypedResult(){ NodeType = NodeType.Lookup, Text = roll};
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, roll));
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.None, table.LookupResult(iRoll)));

            return typedResult;
        }
    }
}
