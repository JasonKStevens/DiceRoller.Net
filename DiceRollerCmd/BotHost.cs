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


        public BotHost(ILogger<BotHost> logger, IHostApplicationLifetime appLifetime, IDiscordApi discord, Evaluator evaluator, DQLookupTables tables)
        {
            _logger = logger;
            _discordInterface = discord;
            _evaluator = evaluator;
            _injuries = tables.Injuries;
            _backfires = tables.Backfires;
            _fears = tables.Fear;

            _discordInterface.AddHandler("!roll", HandleDiceRolls);
            _discordInterface.AddHandler("!injury", (ins) => 
            LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!specgrev", (ins) => LookupResult(_injuries, ins));
            _discordInterface.AddHandler("!backfire", (ins) => LookupResult(_backfires, ins));
            _discordInterface.AddHandler("!fear", (ins) => LookupResult(_fears, ins));
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
