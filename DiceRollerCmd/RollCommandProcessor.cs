using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiceRoller;
using DiceRoller.DragonQuest;
using DiceRoller.Heroes;
using DiceRoller.Parser;
using DiscordRollerBot;
using Irony.Parsing;

namespace DiceRollerCmd
{
    public class RollCommandProcessor : ICommandProcessor
    {
        public string Prefix => "!roll";

        private readonly DiceRollEvaluator _evaluator;
        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;
        private readonly UserAliases _aliases = new UserAliases();
        private readonly SpeedTable _speeds;
        private readonly LocationTable _locations;

        public RollCommandProcessor(DiceRollEvaluator evaluator,  DQLookupTables dqTables, HerosLookupTables heroesTables)
        {
            _evaluator = evaluator;
            _injuries = dqTables.Injuries;
            _backfires = dqTables.Backfires;
            _fears = dqTables.Fear;
            _speeds = heroesTables.Speeds;
            _locations = heroesTables.Locations;
        }


        public (bool, string) Process(DiscordUserInfo userInfo, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, string.Empty);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!tokens[0].Equals(Prefix,StringComparison.InvariantCultureIgnoreCase))
                return (false, null);

            string aliasName;
            string aliasInstruction;
            ParseTree tree;

            if (tokens.Length < 2)
            {
                var tkns = new List<string>(tokens);
                tkns.Add("3d6");
                tokens = tkns.ToArray();
            }

            //what are we dealing with
            switch (tokens[1].ToLower())
            {
                case "backfire": 
                    return (true, GenerateResult(_backfires, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "specgrev":
                case "injury":
                    return (true, GenerateResult(_injuries, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "fear":
                    return (true, GenerateResult(_fears, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length-2) : ""));

                case "speed":
                    return (true, GenerateResult(_speeds, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "hitlocation":
                case "hitloc":
                    return (true, GenerateResult(_locations, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "addalias":
                    if (tokens.Length < 4)
                        return (true, $"Cannot add an alias with no instructions. Syntax is: {Prefix} {tokens[1]} <name> <instruction>");

                    //TODO: make sure the user is not using an alias name that can be evaluated by the evaluator!!!!
                    aliasName = tokens[2].ToLower();
                    aliasInstruction = string.Join(' ', tokens, 3, tokens.Length-3);
                    tree = _evaluator.Parse(aliasInstruction);

                    _aliases.AddUpdate(userInfo.Id, aliasName, tree);

                    return (true, $"Alias '{aliasName}' added");  

                case "removealias":
                case "deletealias":
                    if (tokens.Length < 3)
                        return (true, "Cannot remove an alias with no name. Syntax is: {Prefix} {tokens[0]} <name>");

                    aliasName = tokens[2].ToLower();

                    _aliases.Remove(userInfo.Id, aliasName);

                    return (true, $"Alias '{aliasName}' removed");

                case "listalias":
                    var list = _aliases.GetAliasList(userInfo.Id);

                    return (true, $"{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, list)}");

                case "help":
                    return (true, Constants.GetHelpText());

                default:
                    aliasName = tokens[1].ToLower();

                    tree = _aliases.Get(userInfo.Id, aliasName);

                    if (tree != null)
                        return (true, FormatResultNode(userInfo, _evaluator.Evaluate(tree)));

                    return (true, FormatResultNode(userInfo, _evaluator.Evaluate(string.Join(' ', tokens, 1, tokens.Length-1))));
            }
        }

        private string FormatResultNode(DiscordUserInfo user, DiceResultNode node)
        {
            var builder = new StringBuilder();
            builder.Append("   "  + Emotify(node.Value) + "  ");
            if (node.Breakdown.Length > 100)
            {
                builder.AppendLine();
                builder.Append("Reason:  ");
                builder.AppendLine();
                builder.AppendLine("||" + node.Breakdown + "||");
            } else {
                builder.Append("Reason:  ");
                builder.AppendLine(node.Breakdown);
            }

            return builder.ToString();        
        }

        private string Emotify(float value)
        {
            return value.ToString()
//                            .Replace("-", ":traffic_light:")
                            .Replace("-", "**--** ")
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


        private string GenerateResult(LookupTable table, string instructions)
        {
            if (string.IsNullOrWhiteSpace(instructions))
                instructions = table.GetRoll();

            int roll = ( int ) _evaluator.Evaluate(instructions).Value;

            return "__**" + Emotify(roll) + "**__" +Environment.NewLine + "```styl" + Environment.NewLine + table.LookupResult(roll) + "```";
        }
    }

    
}
