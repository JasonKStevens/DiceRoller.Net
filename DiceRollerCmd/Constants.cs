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
                sb.AppendLine("!roll <instructions>");
                sb.AppendLine("Executes dice rolls based upon the instructions passed. Die are structure in the xdf format, where x is the number of dice to roll and f the number of faces on the die. e.g. 3d10 will roll 3 ten-sided dice.");
                sb.AppendLine("Dice can be combined with +, -, / and * operators. Brackes '(' and ')' can be used to enforce operator precedence.");
                sb.AppendLine("Exploding dice can be requested using the ! operator after the faces specifier. e.g. d10! or 5d3!");
                sb.AppendLine("Comments can be added with the # symbol. Note that anything after the # will be treated as a comment, so it should be the last command used in an instruction. e.g. 4d6 #roll 4d6");
                sb.AppendLine("");
                sb.AppendLine("The 'min' command can be used to specify a minimum outcome. e.g. min(d10-4,1) will roll a d10 subtract 4 and if the number is less than 1, 1 will be the result.");
                sb.AppendLine("The 'repeat' command can be used to repeat an instruction multiple times. e.g. repeat(d10,8) will roll a d10 eight times.");
                sb.AppendLine("The 'step' command can be used to roll the appropriate dice combination as specified by the Earth Dawn 4th edition system. e.g. !roll step 10. This will roll 2d8 exploding dice.");
                sb.AppendLine("");
                sb.AppendLine("");
                sb.AppendLine("!injury [instruction]");
                sb.AppendLine("!specgrev [instruction]");
                sb.AppendLine("Generates a Grevious Injury from the DQ Grevious Injust table. If an instruction is not specified, it will roll a d100 to determine the result. Otherwise it will use the instruction to generate the result. e.g. !injury 10");
                sb.AppendLine("");
                sb.AppendLine("!fear [instruction]");
                sb.AppendLine("Generates a Fear result from the DQ Fright table. If an instruction is not specified, it will roll a d100 to determine the result. Otherwise it will use the instruction to generate the result. e.g. !fear 10");
                sb.AppendLine("");
                sb.AppendLine("!backfire [instruction]");
                sb.AppendLine("Generates a Fear result from the DQ Fright table. If an instruction is not specified, it will roll a d100 to determine the result. Otherwise it will use the instruction to generate the result. e.g. !backfire 10");
                sb.AppendLine("```");

                _helpText = sb.ToString();
            }

            return _helpText;
        }
    }
}
