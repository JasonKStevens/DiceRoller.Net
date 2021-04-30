using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DiceRoller.Parser;
using Irony.Parsing;

namespace PartyDSL.Parser
{
    public class PartyCommandVisitor
    {
        private readonly IPartyManager _partyManager;
        private readonly string _prefix;
        private readonly DiceRollEvaluator _diceRollEvaluator;

        public PartyCommandVisitor(DiceRollEvaluator diceRollEvaluator, IPartyManager partyManager, string prefix)
        {
            _diceRollEvaluator = diceRollEvaluator;
            _partyManager = partyManager;
            _prefix = prefix;
        }

        private List<string> BadPartyNames = new List<string>{
            "roll",
            "dq",
        };

        public PartyResultNode Visit(ParseTreeNode node)
        {
            if (_prefix == "party")
            {
                switch (node.Term.Name)
                {
                    case "createparty":
                        var partyName = Visit(node.ChildNodes[1]).Value.Trim();

                        if (string.IsNullOrWhiteSpace(partyName))
                            return new PartyResultNode("Syntax: create <name_of_party>");

                        if (BadPartyNames.Contains(partyName.ToLower()))
                            return new PartyResultNode("Syntax: invalid party name. Bad names: " + string.Join(',', BadPartyNames));

                        _partyManager.Create(partyName);

                        return new PartyResultNode("Created.");

                    case "listparties":
                        var parties = _partyManager.GetAll();

                        if (parties.Count() < 1)
                            return new PartyResultNode("No parties have been created!");

                        return new PartyResultNode(string.Join(Environment.NewLine, parties.Select(x => x.Name)));    
                        
                    case "deleteparty":
                        partyName = Visit(node.ChildNodes[1]).Value;

                        if (string.IsNullOrWhiteSpace(partyName))
                            return new PartyResultNode("Syntax: delete <name_of_party>");

                        var party = _partyManager.Delete(partyName);

                        return new PartyResultNode($"{party.Name} deleted.");

                    case "saveconfig":
                        return new PartyResultNode(_partyManager.Serialize());

                    case "loadconfig":
                        var json = node.ChildNodes[1].Token.Text.Replace("|", "");
                        _partyManager.Hydrate(json);

                        parties = _partyManager.GetAll();

                        if (parties.Count() < 1)
                            return new PartyResultNode("No parties have been created!");

                        return new PartyResultNode(string.Join(Environment.NewLine, parties.Select(x => x.Name)));    

                    case "help":
                        return new PartyResultNode(PartyGrammar.HelpText());

                    case "partyname":
                        return new PartyResultNode(node.Token.Text);                

                    case "expression":
                        return Visit(node.ChildNodes[0]);
                }
            }

            switch (node.Term.Name)
            {
                case "membername":
                case "varname":
                case "value":
                    return new PartyResultNode(node.Token.Text);

                case "addmember":
                    var memberName = Visit(node.ChildNodes[1]).Value;
                    
                    var party = _partyManager.GetParty(_prefix);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {_prefix}");

                    PartyMember master = null;

                    if (node.ChildNodes.Count > 2)
                    {
                        var masterName = Visit(node.ChildNodes[2]).Value;

                        master = party.GetMember(masterName);

                        if (master == null)
                            return new PartyResultNode($"Could not find party member {masterName} in party {_prefix}");
                    }

                    party.AddMember(memberName, master);

                    return new PartyResultNode($"{memberName} added.");

                case "removemember":
                    memberName = Visit(node.ChildNodes[1]).Value;

                    party = _partyManager.GetParty(_prefix);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {_prefix}");

                    party.RemoveMember(memberName);

                    return new PartyResultNode($"{memberName} removed.");                

                case "show":
                    party = _partyManager.GetParty(_prefix);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {_prefix}");

                    if (node.ChildNodes.Count < 2)
                    {   //This is just a plain 'show members'
                        var membersToShow = party.GetAll();

                        if (membersToShow.Count() < 1)
                            return new PartyResultNode("No members have been added!");

                        return new PartyResultNode(string.Join(Environment.NewLine, membersToShow.Select(x => x.Name)));    
                    }

                    var rollName = Visit(node.ChildNodes[1]).Value;
                    
                    return new PartyResultNode(party.GetLastRoll(rollName));

                case "setvalue":
                    var varName = Visit(node.ChildNodes[1]).Value;
                    memberName = Visit(node.ChildNodes[2]).Value;
                    var expression = node.ChildNodes[3].Token.Text;
                    expression = expression.Replace("\"", "");

                    party = _partyManager.GetParty(_prefix);
                    if (party == null)
                        return new PartyResultNode($"Could not find party named {_prefix}");
                    
                    var member = party.GetMember(memberName);

                    if (member == null)
                        return new PartyResultNode($"Could not find party member {memberName} in party {_prefix}");

                    member.SetRoll(varName, expression);

                    return new PartyResultNode($"Set roll '{varName}' to '{expression}' for {memberName} in party {_prefix}");

                case "roll":
                    rollName = Visit(node.ChildNodes[1]).Value;

                    party = _partyManager.GetParty(_prefix);
                    if (party == null)
                        return new PartyResultNode($"Could not find party named {_prefix}");
                    
                    var members = party.GetAll();

                    var results = new List<PartyMemberWithRoll>();

                    foreach (var mem in members.Where(x => x.Master == null).ToList())
                    {
                        var roll = mem.GetRoll(rollName);
                        DiceResultNode rollValue = new DiceResultNode(0);

                        if (!string.IsNullOrWhiteSpace(roll))
                            rollValue = _diceRollEvaluator.Evaluate(roll);

                        results.Add(new PartyMemberWithRoll(mem, rollValue.Value, rollValue.Breakdown));
                    }

                    foreach (var mem in members.Where(x => x.Master != null).ToList())
                    {
                        var roll = mem.GetRoll(rollName);
                        DiceResultNode rollValue = new DiceResultNode(0);

                        if (!string.IsNullOrWhiteSpace(roll))
                            rollValue = _diceRollEvaluator.Evaluate(roll);

                        var masterRoll = results.Single(x => x.PartyMember == mem.Master).Roll;

                        var adjustedValue = rollValue.Value;
                        if (adjustedValue > masterRoll) adjustedValue = masterRoll;
                        if (adjustedValue < masterRoll) adjustedValue = masterRoll-10;
                        if (adjustedValue < 0) adjustedValue = 0;

                        results.Add(new PartyMemberWithRoll(mem, adjustedValue, rollValue.Breakdown));
                    }


                    var sb = new StringBuilder();

                    foreach (var roll in results.OrderByDescending(x => x.Roll))
                    {
                        sb.AppendLine(roll.Roll + ": " + roll.PartyMember.Name + " " + roll.RollReason);
                    }

                    var result = sb.ToString();
                    party.StoreRoll(rollName, result)                    ;

                    return new PartyResultNode(sb.ToString());

                case "expression":
                    return Visit(node.ChildNodes[0]);
            }

            throw new InvalidOperationException($"Unrecognizable term '{node.Term.Name}'.");
        }


    }
}
