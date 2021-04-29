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
        private readonly DiceRollEvaluator _diceRollEvaluator;

        public PartyCommandVisitor(DiceRollEvaluator diceRollEvaluator, IPartyManager partyManager)
        {
            _diceRollEvaluator = diceRollEvaluator;
            _partyManager = partyManager;
        }

        public PartyResultNode Visit(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "partyname":
                case "membername":
                case "varname":
                case "value":
                    return new PartyResultNode(node.Token.Text);

                case "createparty":
                    var partyName = Visit(node.ChildNodes[1]).Value;

                    if (string.IsNullOrWhiteSpace(partyName))
                        return new PartyResultNode("Syntax: create <name_of_party>");

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

                case "addmember":
                    var memberName = Visit(node.ChildNodes[1]).Value;
                    partyName = Visit(node.ChildNodes[2]).Value;

                    party = _partyManager.GetParty(partyName);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {partyName}");

                    PartyMember master = null;

                    if (node.ChildNodes.Count > 3)
                    {
                        var masterName = Visit(node.ChildNodes[3]).Value;

                        master = party.GetMember(masterName);

                        if (master == null)
                            return new PartyResultNode($"Could not find party member {masterName} in party {partyName}");
                    }

                    party.AddMember(memberName, master);

                    return new PartyResultNode($"{memberName} added.");

                case "removemember":
                    memberName = Visit(node.ChildNodes[1]).Value;
                    partyName = Visit(node.ChildNodes[2]).Value;

                    party = _partyManager.GetParty(partyName);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {partyName}");

                    party.RemoveMember(memberName);

                    return new PartyResultNode($"{memberName} removed.");                

                case "showmembers":
                    partyName = Visit(node.ChildNodes[1]).Value;

                    party = _partyManager.GetParty(partyName);

                    if (party == null)
                        return new PartyResultNode($"Could not find party named {partyName}");

                    var members = party.GetAll();

                    if (members.Count() < 1)
                        return new PartyResultNode("No members have been added!");

                    return new PartyResultNode(string.Join(Environment.NewLine, members.Select(x => x.Name)));    

                case "setvalue":
                    var varName = Visit(node.ChildNodes[1]).Value;
                    memberName = Visit(node.ChildNodes[2]).Value;
                    partyName = Visit(node.ChildNodes[3]).Value;
                    var expression = node.ChildNodes[4].Token.Text;
                    expression = expression.Replace("\"", "");

                    party = _partyManager.GetParty(partyName);
                    if (party == null)
                        return new PartyResultNode($"Could not find party named {partyName}");
                    
                    var member = party.GetMember(memberName);

                    if (member == null)
                        return new PartyResultNode($"Could not find party member {memberName} in party {partyName}");

                    member.SetRoll(varName, expression);

                    return new PartyResultNode($"Set roll '{varName}' to '{expression}' for {memberName} in party {partyName}");

                case "roll":
                    varName = Visit(node.ChildNodes[1]).Value;
                    partyName = Visit(node.ChildNodes[2]).Value;

                    party = _partyManager.GetParty(partyName);
                    if (party == null)
                        return new PartyResultNode($"Could not find party named {partyName}");
                    
                    members = party.GetAll();

                    var results = new List<PartyMemberWithRoll>();

                    foreach (var mem in members.Where(x => x.Master == null).ToList())
                    {
                        var roll = mem.GetRoll(varName);
                        DiceResultNode rollValue = new DiceResultNode(0);

                        if (!string.IsNullOrWhiteSpace(roll))
                            rollValue = _diceRollEvaluator.Evaluate(roll);

                        results.Add(new PartyMemberWithRoll(mem, rollValue.Value, rollValue.Breakdown));
                    }

                    foreach (var mem in members.Where(x => x.Master != null).ToList())
                    {
                        var roll = mem.GetRoll(varName);
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
                        sb.AppendLine(roll.Roll + ": " + roll.PartyMember.Name + " [" + roll.RollReason + "]");
                    }

                    return new PartyResultNode(sb.ToString());


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

                case "expression":
                    return Visit(node.ChildNodes[0]);
            }

            throw new InvalidOperationException($"Unrecognizable term '{node.Term.Name}'.");
        }
    }
}
