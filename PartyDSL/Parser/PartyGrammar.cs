using Irony.Parsing;
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
            var memberName = new IdentifierTerminal("membername");
            var varName = new IdentifierTerminal("varname");
            var value = new StringLiteral("value", "\"", StringOptions.NoEscapes | StringOptions.AllowsDoubledQuote);
            var json = new StringLiteral("json", "|", StringOptions.NoEscapes);

            // Nonterminals
            var expression = new NonTerminal("expression");

            var create = new NonTerminal("createparty");
            var loadConfig = new NonTerminal("loadconfig");
            var saveConfig = new NonTerminal("saveconfig");
            var addMember = new NonTerminal("addmember");
            var showMembers = new NonTerminal("showmembers");
            var removeMember = new NonTerminal("removemember");
            var deleteParty = new NonTerminal("deleteparty");
            var roll = new NonTerminal("roll");
            var help = new NonTerminal("help");

            var setValue = new NonTerminal("setvalue");
            var listParties = new KeyTerm("list", "listparties");

            // Rules
            expression.Rule = create | addMember | listParties | showMembers | removeMember | deleteParty | setValue | roll | loadConfig | saveConfig | help;

            create.Rule = new KeyTerm("create", "create") + partyName;
            loadConfig.Rule = new KeyTerm("load", "load") + json;
            saveConfig.Rule = new KeyTerm("save", "save");
            var addTerm = new KeyTerm("add", "add");
            addMember.Rule = addTerm + memberName + "to" + partyName + "as" + "an" + "ally" + "of" + memberName | addTerm + memberName + "to" + partyName;
            var showTerm = new KeyTerm("show", "show");
            showMembers.Rule = showTerm + "members" + "of" + partyName;
            removeMember.Rule = new KeyTerm("remove", "remove") + memberName + "from" + partyName;
            deleteParty.Rule = new KeyTerm("delete", "delete") + partyName;
            help.Rule = new KeyTerm("help", "help");

            setValue.Rule = new KeyTerm("set", "set") + varName + "for" + memberName + "in" + "party" + partyName + "to" + value;
            var rollTerm = new KeyTerm("roll", "roll");
            roll.Rule = rollTerm + varName + "for" + "party" + partyName;

            this.MarkPunctuation("to", "add", "create", "list", "as", "an", "ally", "of", "members", "from", "delete", "for", "in", "party");

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

            sb.AppendLine("!party add <memberName> to <partyName>");
            sb.AppendLine("Adds a new member to the party.");

            sb.AppendLine("!party add <memberName> to <partyName> as an ally of <masterName>");
            sb.AppendLine("Adds a new ally to the party that belongs to the master.");

            sb.AppendLine("!party set <rollName> for <memberName> in party <partyName> to <rollDefinition>");
            sb.AppendLine("Configures a named roll for the member in the party.");

            sb.AppendLine("!party roll <rollName> for party <partyName>");
            sb.AppendLine("Executes the named roll for each party member, returning in descending order. This is currently configured for doing INITIATIVE in dq, so agents will have their rolls adjusted.");

            sb.AppendLine("```");            

            _helpText = sb.ToString();

            return _helpText;
        }
    }
}
