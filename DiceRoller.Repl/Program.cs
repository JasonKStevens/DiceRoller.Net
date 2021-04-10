using DiceRoller.Dice;
using DiceRoller.Parser;
using System;

namespace DiceRoller.Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("eg. (3d10 + 5) / 2 + 1d2!");
            Console.WriteLine();

            var evaluator = new Evaluator(new RandomNumberGenerator());

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input == "exit" || input == "quit")
                    break;

                try
                {
                    var evaluation = evaluator.Evaluate(input);
                    Console.WriteLine(evaluation.Value + " Reason: " + evaluation.Breakdown);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }
            }
        }
    }
}
