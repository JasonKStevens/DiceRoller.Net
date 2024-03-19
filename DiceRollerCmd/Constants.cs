using System.Text;

namespace DiceRollerCmd
{
    internal static class Constants
    {
        private static string _helpText = null;

        internal static string GetHelpText()
        {
            if (_helpText == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine("```llvm");
                sb.AppendLine("DqRoller.NET usage:");
                sb.AppendLine("");
                sb.AppendLine("!roll <dice>");
                sb.AppendLine("Executes dice rolls based upon the instructions passed. Die are structure in the xdf format, where x is the number of dice to roll and f the number of faces on the die. e.g. 3d10 will roll 3 ten-sided dice.");
                sb.AppendLine("Dice can be combined with +, -, / and * operators. Brackes '(' and ')' can be used to enforce operator precedence.");
                sb.AppendLine("Exploding dice can be requested appending the ! operator. e.g. d10! or 5d3!");
                sb.AppendLine("Comments can be added with the # symbol. Note that anything after the # will be treated as a comment, so it should be the last command used in an instruction. e.g. 4d6 #roll 4d6");
                sb.AppendLine("");
                sb.AppendLine("'min' specify a minimum outcome. e.g. min(d10-4,1) if the number is less than 1, 1 will be the result.");
                sb.AppendLine("'repeat' repeat an instruction multiple times. e.g. repeat(d10,8) will roll a d10 eight times.");
                sb.AppendLine("'step' roll the appropriate dice combination as specified by the Earth Dawn 4th edition system.");
                sb.AppendLine("'injury' generate a Specific Grevious Injury result");
                sb.AppendLine("'fear' generate a Fear result");
                sb.AppendLine("'backfire' generate a Backfire result");
                sb.AppendLine("'hitloc [bodyType]_[aimZone]' generate a hit location for the bodyType and aiming zone: [humanoid/quadruped/avian/serpent]_[high/mid/low/legs/arms]");
                sb.AppendLine("'help' command will show this help");
                sb.AppendLine("");
                sb.AppendLine("Aliases");
                sb.AppendLine(" 'addalias' allows definition of an alias. Syntax: !roll addalias <name> <instructions>. e.g. !roll addalias percentile d100.");
                sb.AppendLine(" 'removealias' or 'deletealias' delete an alias. Syntax: !roll removealias <name>. e.g. !roll deletealias percentile.");
                sb.AppendLine(" 'listalias' list your aliases. Syntax: !roll listalias");
                sb.AppendLine("");
                sb.AppendLine("!dq injury <number> - lookup a Specific Grevious Injury result");
                sb.AppendLine("!dq fear <number> - lookup a Fear result");
                sb.AppendLine("!dq backfire <number> - lookup a Backfire result");
                sb.AppendLine("```");

                _helpText = sb.ToString();
            }

            return _helpText;
        }
    }
}
