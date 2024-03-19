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
        public List<string> Prefixes = new List<string>() {"!roll", "/roll"};
        public List<string> TypePrefixes = new List<string>() {"!repeat", "/repeat"};

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

        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, TypedResult.Null);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!tokens[0].Equals(Prefix, StringComparison.InvariantCultureIgnoreCase))
                return (false, null);

            string aliasName;
            string aliasInstruction;
            ParseTree tree;

            if (tokens.Length < 2)
            {
                var tkns = new List<string>(tokens) {"3d6"};
                tokens = tkns.ToArray();
            }

            //what are we dealing with
            switch (tokens[1].ToLower())
            {
                case "backfire":
                    return (true, GenerateTypedResult(_backfires, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "specgrev":
                case "injury":
                    return (true, GenerateTypedResult(_injuries, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "fear":
                    return (true, GenerateTypedResult(_fears, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "speed":
                    return (true, GenerateTypedResult(_speeds, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "hitlocation":
                case "hitloc":
                    return (true, GenerateTypedResult(_locations, tokens.Length > 2 ? string.Join(' ', tokens, 2, tokens.Length - 2) : ""));

                case "addalias":
                    if (tokens.Length < 4)
                        return (true, TypedResult.NewSimpleResult($"Cannot add an alias with no instructions. Syntax is: {Prefix} {tokens[1]} <name> <instruction>"));

                    //TODO: make sure the user is not using an alias name that can be evaluated by the evaluator!!!!
                    aliasName = tokens[2].ToLower();
                    aliasInstruction = string.Join(' ', tokens, 3, tokens.Length - 3);
                    tree = _evaluator.Parse(aliasInstruction);

                    _aliases.AddUpdate(userId, aliasName, tree);

                    return (true, TypedResult.NewSimpleResult($"Alias '{aliasName}' added"));

                case "removealias":
                case "deletealias":
                    if (tokens.Length < 3)
                        return (true, TypedResult.NewSimpleResult("Cannot remove an alias with no name. Syntax is: {Prefix} {tokens[0]} <name>"));

                    aliasName = tokens[2].ToLower();

                    _aliases.Remove(userId, aliasName);

                    return (true, TypedResult.NewSimpleResult($"Alias '{aliasName}' removed"));

                case "listalias":
                    var list = _aliases.GetAliasList(userId);

                    return (true, TypedResult.NewSimpleResult($"{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, list)}"));

                case "help":
                    return (true, TypedResult.NewSimpleResult(Constants.GetHelpText()));

                default:
                    aliasName = tokens[1].ToLower();

                    tree = _aliases.Get(userId, aliasName);

                    if (tree != null)
                        return (true, _evaluator.Evaluate(tree).TypedResult);

                    return (true, _evaluator.Evaluate(string.Join(' ', tokens, 1, tokens.Length - 1)).TypedResult);
            }
    
        }

        private TypedResult GenerateTypedResult(LookupTable table, string instructions)
        {
            if (string.IsNullOrWhiteSpace(instructions))
                instructions = table.GetRoll();

            int roll = ( int ) _evaluator.Evaluate(instructions).Value;

            var typedResult = new TypedResult(){ NodeType = NodeType.Lookup, Text = roll.ToString()};
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, roll.ToString()));
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.None, table.LookupResult(roll)));

            return typedResult;
        }
    }

    
}
