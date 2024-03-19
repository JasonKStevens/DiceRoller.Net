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
        public List<string> Prefixes = new List<string>() {"!roll", "\\roll"};

        private readonly DiceRollEvaluator _evaluator;
        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;
        private readonly UserAliases _aliases = new UserAliases();
        private readonly SpeedTable _speeds;
        private readonly Dictionary<string, LookupTable> _locations;

        public RollCommandProcessor(DiceRollEvaluator evaluator, DQLookupTables dqTables, HerosLookupTables heroesTables)
        {
            _evaluator = evaluator;
            _injuries = dqTables.Injuries;
            _backfires = dqTables.Backfires;
            _fears = dqTables.Fear;
            _speeds = heroesTables.Speeds;
            _locations = new Dictionary<string, LookupTable>(); 
            AddLookupTable(new HumanoidHighLocationsTable(), "humanoid_high", "hh");
            AddLookupTable(new HumanoidArmsLocationsTable(), "humanoid_arms", "ha");
            AddLookupTable(new HumanoidMidLocationsTable(), "humanoid_mid", "hm");
            AddLookupTable(new HumanoidLegsLocationsTable(), "humanoid_legs", "hl");
            AddLookupTable(new QuadrupedHighLocationsTable(), "quadruped_high", "qh");
            AddLookupTable(new QuadrupedMidLocationsTable(), "quadruped_mid", "qm");
            AddLookupTable(new QuadrupedLowLocationsTable(), "quadruped_low", "ql");
            AddLookupTable(new AvianHighLocationsTable(), "avian_high", "ah");
            AddLookupTable(new AvianMidLocationsTable(), "avian_mid", "am");
            AddLookupTable(new AvianLowLocationsTable(), "avian_low", "al");
            AddLookupTable(new SerpentineHighLocationsTable(), "serpent_high", "sh");
            AddLookupTable(new SerpentineMidLocationsTable(), "serpent_mid", "sm");
            AddLookupTable(new SerpentineLowLocationsTable(), "serpent_low", "sl");

        }

        private void AddLookupTable(LookupTable table, params string[] names)
        {
            foreach (var name in names)
            {
                _locations.Add(name, table);
            }
        }
        
        public (bool, TypedResult) ProcessTyped(string userId, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return (false, TypedResult.Null);

            var tokens = commandText.Split(" ",StringSplitOptions.None);

            if (!Prefixes.Contains(tokens[0]))
                return (false, null);

            string aliasName;
            string aliasInstruction;
            ParseTree tree;

            if (tokens.Length < 2)
            {
                var tkns = new List<string>(tokens) {"d100"};
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
                    //test for presence of a body-type

                    var bodyType = "humanoid_mid";
                    var roll = "";

                    if (tokens.Length > 2)
                    {
                        string token;
                        token = tokens[2].ToLower();
                        if (int.TryParse(token, out var i))
                        {
                            roll = i.ToString();
                        }
                        else
                        {
                            bodyType = token;
                        }

                        if (tokens.Length > 3)
                        {
                            token = tokens[3].ToLower();
                            if (int.TryParse(token, out var i2))
                            {
                                roll = i2.ToString();
                            }
                            else
                            {
                                bodyType = token;
                            }
                        }
                    }

                    return (true, GenerateTypedLocationResult(roll, bodyType));

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

        private TypedResult GenerateTypedLocationResult(string rollInstruction, string bodyType)
        {
            if (!_locations.ContainsKey(bodyType))
                return TypedResult.NewSimpleResult($"Unknown body type: '{bodyType}'");

            var table = _locations[bodyType];

            if (string.IsNullOrWhiteSpace(rollInstruction))
                rollInstruction = table.GetRoll();

            int roll = ( int ) _evaluator.Evaluate(rollInstruction).Value;

            var typedResult = new TypedResult(){ NodeType = NodeType.Lookup, Text = roll.ToString()};
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, roll.ToString()));
            typedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.None, table.LookupResult(roll)));

            return typedResult;
        }
    }


}
