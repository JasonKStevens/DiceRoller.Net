using System;
using System.Threading.Tasks;
using DiceRoller.Dice;
using DiceRoller.Parser;
using DiscordRollerBot;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace DiceRollerCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new DiscordConfiguration()
            {
                Token = "ODI2OTAxMTg0ODY1NzYzMzU4.YGTNvQ.N6EnSvR9VXIXVaQEtsIJyIlLgXI", 
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            };

            var client = new DiscordClient(config);

            var lf = LoggerFactory.Create(logging => logging.AddConsole());
            
            var logger = lf.CreateLogger<Program>();
            logger.LogError("Test");

            var botLogger = lf.CreateLogger<DiscordInterface>();

            var botConfig = new DiscordInterfaceConfiguration()
            {
                CommandPrefix = "!roll"
            };

            var bot = new DiscordInterface(client, botConfig, new Evaluator(new RandomNumberGenerator()), botLogger);

            try
            {
                bot.Start();

                var input = Console.ReadLine();
            } finally
            {
                bot.Stop();
            }

            

            // Console.WriteLine("eg. (3d10 + 5) / 2 + 1d2!");
            // Console.WriteLine();

            // var evaluator = new Evaluator(new RandomNumberGenerator());

            // while (true)
            // {
            //     Console.Write("> ");
            //     var input = Console.ReadLine();
            //     if (input == "exit" || input == "quit")
            //         break;

            //     try
            //     {
            //         var evaluation = evaluator.Evaluate(input);
            //         Console.WriteLine(evaluation.Value + " Reason: " + evaluation.Breakdown);
            //         Console.WriteLine();
            //     }
            //     catch (Exception ex)
            //     {
            //         Console.WriteLine(ex.Message);
            //         Console.WriteLine();
            //     }
            // }
        }
    }
}
