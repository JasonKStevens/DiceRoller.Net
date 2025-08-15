using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Test.Helpers;

var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 4, 1, 6, 3, 3, 3, 3, 2, 6, 11, 11, 9, 1, 3, 5, 16, 16, 14, 6, 1, 3, 4);
var evaluator = new DiceRollEvaluator(sequenceGenerator);
var converter = new ResultNodeConverter();

// Test repeat expression
var result = evaluator.Evaluate("repeat(d6, 3)");
var converted = converter.Convert(result.TypedResult);

Console.WriteLine("=== REPEAT BREAKDOWN ===");
Console.WriteLine($"Value: {converted.Value}");
Console.WriteLine($"Breakdown: {converted.Breakdown}");
Console.WriteLine($"TypedResult.NodeType: {converted.TypedResult?.NodeType}");
Console.WriteLine($"TypedResult.Text: {converted.TypedResult?.Text}");
Console.WriteLine($"TypedResult.SubText.Count: {converted.TypedResult?.SubText?.Count}");

// Test min expression  
var result2 = evaluator.Evaluate("min(3d6, 10)");
var converted2 = converter.Convert(result2.TypedResult);

Console.WriteLine("\n=== MIN BREAKDOWN ===");
Console.WriteLine($"Value: {converted2.Value}");
Console.WriteLine($"Breakdown: {converted2.Breakdown}");
Console.WriteLine($"TypedResult.NodeType: {converted2.TypedResult?.NodeType}");
Console.WriteLine($"TypedResult.Text: {converted2.TypedResult?.Text}");
Console.WriteLine($"TypedResult.SubText.Count: {converted2.TypedResult?.SubText?.Count}");

// Test comparison expression
var result3 = evaluator.Evaluate("3d6 > 12");
var converted3 = converter.Convert(result3.TypedResult);

Console.WriteLine("\n=== COMPARISON BREAKDOWN ===");
Console.WriteLine($"Value: {converted3.Value}");
Console.WriteLine($"Breakdown: {converted3.Breakdown}");
Console.WriteLine($"TypedResult.NodeType: {converted3.TypedResult?.NodeType}");
Console.WriteLine($"TypedResult.Text: {converted3.TypedResult?.Text}");
Console.WriteLine($"TypedResult.SubText.Count: {converted3.TypedResult?.SubText?.Count}");