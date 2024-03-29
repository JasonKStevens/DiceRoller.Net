﻿using Irony.Parsing;
using System;
using System.Text;

namespace PartyDSL.Parser
{
    public class PartyGrammar : Grammar
    {
        public PartyGrammar() : base(false)
        {
            // Terminals
            var partyName = new IdentifierTerminal("partyname");
            // var memberName = new IdentifierTerminal("membername");
            var memberName = new RegexBasedTerminal("membername", @"[\w\.\-]+");
            var rollName = new IdentifierTerminal("varname");
            var value = new StringLiteral("value", "\"", StringOptions.NoEscapes | StringOptions.AllowsDoubledQuote);
            var number = new NumberLiteral("number", NumberOptions.IntOnly);
            number.DefaultIntTypes = new TypeCode[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
            number.DefaultFloatType = TypeCode.Single;

            var json = new StringLiteral("json", "|", StringOptions.NoEscapes);

            // Nonterminals
            var expression = new NonTerminal("expression");

            var create = new NonTerminal("createparty");
            var listParties = new KeyTerm("list", "listparties");
            var loadConfig = new NonTerminal("loadconfig");
            var saveConfig = new NonTerminal("saveconfig");
            var deleteParty = new NonTerminal("deleteparty");

            var addMember = new NonTerminal("addmember");
            var show = new NonTerminal("show");
            var removeMember = new NonTerminal("removemember");
            var roll = new NonTerminal("roll");
            var init = new NonTerminal("init");
            var help = new NonTerminal("help");
            var clearLucky = new NonTerminal("clearluckynumbers");
            var addLucky = new NonTerminal("addluckynumber");

            var setValue = new NonTerminal("setvalue");

            // Rules
            expression.Rule = create | addMember | listParties | show | removeMember | deleteParty | setValue | roll | loadConfig | saveConfig | help | clearLucky | addLucky;

            create.Rule = new KeyTerm("create", "create") + partyName;
            deleteParty.Rule = new KeyTerm("delete", "delete") + partyName;
            loadConfig.Rule = new KeyTerm("load", "load") + json;
            saveConfig.Rule = new KeyTerm("save", "save");

            var addTerm = new KeyTerm("add", "add");
            addMember.Rule = addTerm + memberName + "as" + "an" + "ally" + "of" + memberName | addTerm + memberName;
            
            var showTerm = new KeyTerm("show", "show");
            show.Rule = showTerm + "members" | showTerm + "last" + rollName;
            
            removeMember.Rule = new KeyTerm("remove", "remove") + memberName;
            help.Rule = new KeyTerm("help", "help");

            setValue.Rule = new KeyTerm("set", "set") + rollName + "for" + memberName + "to" + value;
            var rollTerm = new KeyTerm("roll", "roll");
            roll.Rule = rollTerm + rollName;

            clearLucky.Rule = new KeyTerm("clearluckynumbers", "clearluckynumbers") + "for" + memberName;
            addLucky.Rule = new KeyTerm("addluckynumber", "addluckynumber") + number + "to" + memberName;

            this.MarkPunctuation("to", "add", "create", "list", "as", "an", "ally", "of", "members", "from", "delete", "for", "in", "party", "last");

            Root = expression;
        }

        private static string _helpText = "";
        public static string HelpText()
        {
            if (!string.IsNullOrWhiteSpace(_helpText)) return _helpText;

            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("```llvm");
            sb.AppendLine("!Party usage:");
            sb.AppendLine("");

            sb.AppendLine("!party create <name>");
            sb.AppendLine("Creates a party with the given <name>.");
            sb.AppendLine("");
            sb.AppendLine("!party list");
            sb.AppendLine("Lists all parties.");
            sb.AppendLine("");
            sb.AppendLine("!party delete <name>");
            sb.AppendLine("Delete the party with the given <name>.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> add <memberName>");
            sb.AppendLine("Adds a new member to the party.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> add <memberName> as an ally of <masterName>");
            sb.AppendLine("Adds a new ally to the party that belongs to the master.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> set <rollName> for <memberName> to <rollDefinition>");
            sb.AppendLine("Configures a named roll for the member in the party.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> roll <rollName>");
            sb.AppendLine("Executes the named roll for each party member, returning in descending order. This is currently configured for doing INITIATIVE in dq, so agents will have their rolls adjusted.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> show members");
            sb.AppendLine("Lists the members of the party.");
            sb.AppendLine("");

            sb.AppendLine("!<partyName> show last <rollName>");
            sb.AppendLine("Shows the last roll by the party for the specific roll name.");

            sb.AppendLine("```");            

            _helpText = sb.ToString();

            return _helpText;
        }
    }
}
