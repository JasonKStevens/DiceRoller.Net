using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller.DragonQuest;
using DiceRoller.Parser;
using DiscordRollerBot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiceRollerCmd
{
    public class BotHost : IHostedService
    {
        private readonly ILogger<BotHost> _logger;
        private readonly IDiscordApi _discordInterface;
        private readonly Evaluator _evaluator;
        private readonly GrievousInjuries _injuries;
        private readonly Backfires _backfires;
        private readonly FearResult _fears;

        private readonly string _helpText;

        public BotHost(ILogger<BotHost> logger, IHostApplicationLifetime appLifetime, IDiscordApi discord, Evaluator evaluator, DQLookupTables tables)
        {
            _logger = logger;
            _discordInterface = discord;
            _evaluator = evaluator;
            _injuries = tables.Injuries;
            _backfires = tables.Backfires;
            _fears = tables.Fear;

            _discordInterface.AddHandler("!roll", HandleDiceRolls);
            _discordInterface.AddHandler("!injury", (ins) => LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!specgrev", (ins) => LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!backfire", (ins) => LookupResult(_backfires, ins));
            _discordInterface.AddHandler("!fear", (ins) => LookupResult(_fears, ins));
            _discordInterface.AddHandler("!help", (ins) => _helpText);

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

        

        private string HandleDiceRolls(string instructions)
        {
            var result = _evaluator.Evaluate(instructions);

            var builder = new StringBuilder();
            builder.Append("   __**"  + result.Value + "**__  ");
            if (result.Breakdown.Length > 50)
            {
                builder.AppendLine();
                builder.Append("Reason:  ");
                builder.AppendLine();
                builder.AppendLine("||" + result.Breakdown + "||");
            } else {
                builder.Append("Reason:  ");
                builder.AppendLine(result.Breakdown);
            }

            return builder.ToString();
        }

        private string LookupResult(LookupTable table, string instructions)
        {
            int roll = 0;

            if (string.IsNullOrWhiteSpace(instructions))
            {
                roll = ( int ) _evaluator.Evaluate("d100").Value;
            } else
            {
                roll = (int) _evaluator.Evaluate(instructions).Value;
                // if (!Int32.TryParse(instructions.Trim(), out roll))
                // {
                //     return "Huh?";
                // }
            }
            
            return "__**" + roll + "**__" +Environment.NewLine + "```styl" + Environment.NewLine + table.LookupResult(roll) + "```";
//            return "__**" + roll + "**__" + Environment.NewLine + table.LookupResult(roll);
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Stop();
        }
    }
}
