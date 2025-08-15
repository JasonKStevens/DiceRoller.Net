using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Dice;
using DiceRoller.Discord;
using System;

Console.WriteLine("Testing 20d100! for Discord API issues...");

// Create evaluator with default random number generator
var rng = new RandomNumberGenerator();
var evaluator = new DiceRollEvaluator(rng);
var converter = new ResultNodeConverter();
var adapter = new ProgressiveDisclosureDiscordAdapter(converter, "test_user", false);

// Test with a simpler expression first
var testExpression = "3d6!";
Console.WriteLine($"Testing with: {testExpression}");

var result = evaluator.Evaluate(testExpression);
Console.WriteLine($"Result: {result.Value}");
Console.WriteLine($"Breakdown: {result.Breakdown}");
Console.WriteLine($"Breakdown length: {result.Breakdown.Length}");

// Convert to TypedResult and process through Discord adapter
var typedResult = TypedResult.NewSimpleResult(result.Breakdown);
typedResult.NodeType = NodeType.Number;
var discordResponse = adapter.ProcessResult(typedResult, testExpression);

Console.WriteLine($"\nDiscord content length: {discordResponse.Content.Length}");
Console.WriteLine($"Number of components: {discordResponse.Components.Count}");

// Check component structure
foreach (var component in discordResponse.Components)
{
    Console.WriteLine($"Component type: {component.GetType().Name}");
    
    if (component is DSharpPlus.Entities.DiscordActionRowComponent actionRow)
    {
        Console.WriteLine($"  Buttons in action row: {actionRow.Components?.Count ?? 0}");
        
        if (actionRow.Components != null)
        {
            foreach (var button in actionRow.Components)
            {
                if (button is DSharpPlus.Entities.DiscordButtonComponent btnComp)
                {
                    Console.WriteLine($"    Button - CustomId: '{btnComp.CustomId}' (length: {btnComp.CustomId?.Length ?? 0})");
                    Console.WriteLine($"    Button - Label: '{btnComp.Label}' (length: {btnComp.Label?.Length ?? 0})");
                    Console.WriteLine($"    Button - Style: {btnComp.Style}");
                }
            }
        }
    }
}

Console.WriteLine($"\nDiscord content preview:");
Console.WriteLine(discordResponse.Content);

// Check if content exceeds 2000 chars
if (discordResponse.Content.Length > 2000)
{
    Console.WriteLine($"\nWARNING: Content exceeds 2000 characters! Length: {discordResponse.Content.Length}");
}
