using System;
using DiceRoller;
using DiceRoller.DragonQuest;
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


        public (bool, string) Process(DiscordUserInfo userInfo, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, string.Empty);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!tokens[0].Equals(Prefix,StringComparison.InvariantCultureIgnoreCase))
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

        private string LookupResult(LookupTable table, string roll)
        {
            if (string.IsNullOrWhiteSpace(roll))
                return "No roll was specified!";

            int iRoll = Convert.ToInt32(roll);
            return "__**" + roll + "**__" +Environment.NewLine + "```styl" + Environment.NewLine + table.LookupResult(iRoll) + "```";
        }
    }
    
}
