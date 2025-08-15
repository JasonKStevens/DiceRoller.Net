using Irony.Parsing;
using System;
using System.Text;

namespace DiceRollerCmd;

public class SettingsGrammar : Grammar
{
    public SettingsGrammar() : base(false)
    {
        // Terminals
        var json = new StringLiteral("json", "|", StringOptions.NoEscapes);
        //var stringText = new StringLiteral("stringtext");
        var equalSign = ToTerm("=");
        var setTerm = ToTerm("set");
        var deleteTerm = ToTerm("delete");


        // Nonterminals
        var expression = new NonTerminal("expression");

        var stringText = new IdentifierTerminal("stringtext");
        var equals = new NonTerminal("equals");
        var list = new NonTerminal("list");
        var help = new NonTerminal("help");
        var delete = new NonTerminal("delete");
        var save = new NonTerminal("save");
        var load = new NonTerminal("load");


        equals.Rule = setTerm + stringText + equalSign + stringText;
        list.Rule = new KeyTerm("list","list");
        help.Rule = new KeyTerm("help","help");
        delete.Rule = deleteTerm + stringText;
        save.Rule = new KeyTerm("save","save");
        load.Rule = new KeyTerm("load","load") + json;

        expression.Rule = list | help | equals | delete | save | load;

        Root = expression;
    }

    private static string _helpText = "";
    public static string HelpText()
    {
        if (!string.IsNullOrWhiteSpace(_helpText)) return _helpText;

        var sb = new StringBuilder();
        sb.AppendLine("");
        sb.AppendLine("```llvm");
        sb.AppendLine("!settings usage:");
        sb.AppendLine("");

        sb.AppendLine("!settings list");
        sb.AppendLine("Lists all settings.");
        sb.AppendLine("");
        sb.AppendLine("!settings delete <name>");
        sb.AppendLine("Deletes the setting <name>");
        sb.AppendLine("");
        sb.AppendLine("!settings set <name>=<value>");
        sb.AppendLine("Sets the <value> of a specific setting <name>");
        sb.AppendLine("");
        sb.AppendLine("The following settings can be used to turn off various parts of the buttons displayed with messages (per user):");
        sb.AppendLine("  showDiceButtons=false");
        sb.AppendLine("  showHitLocations=false");
        sb.AppendLine("  showSpecialButtons=false");
        sb.AppendLine("    -  this will hide the Spec Griev etc buttons");

        sb.AppendLine("```");

        _helpText = sb.ToString();

        return _helpText;
    }
}