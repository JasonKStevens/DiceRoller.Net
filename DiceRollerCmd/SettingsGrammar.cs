using Irony.Parsing;
using System;
using System.Text;

namespace DiceRollerCmd;

public class SettingsGrammar : Grammar
{
    public SettingsGrammar() : base(false)
    {
        // Terminals
        //var json = new StringLiteral("json", "|", StringOptions.NoEscapes);
        var stringText = new RegexBasedTerminal("stringtext", @"[\w\.\-]+");
        var equalSign = ToTerm("=");


        // Nonterminals
        var list = new NonTerminal("list");
        var expression = new NonTerminal("expression");
        var equals = new NonTerminal("equals");

        equals.Rule = stringText + equalSign + stringText;
        list.Rule = ToTerm("list") | Empty | stringText;
        expression.Rule = equals | list | Empty;

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