using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Test.Helpers;

// Create a test to examine the actual problematic output
var sequenceGenerator = DiceHelper.GetSequenceGenerator(6, 4, 3, 2, 1, 5, 6, 6, 3, 4, 2, 1, 6, 3, 2, 5, 4, 1, 2, 3, 4, 5);
var evaluator = new DiceRollEvaluator(sequenceGenerator);

// Test the problematic expression mentioned in the options document
var result = evaluator.Evaluate("repeat(min(3d6!+2, 20), 4)");

Console.WriteLine("Expression: repeat(min(3d6!+2, 20), 4)");
Console.WriteLine($"Value: {result.Value}");
Console.WriteLine("Raw Breakdown:");
Console.WriteLine(result.Breakdown);
Console.WriteLine("\nBreakdown Length: " + result.Breakdown.Length);

// Test with current formatter
var converter = new ResultNodeConverter();
var diceNode = converter.Convert(result.TypedResult);
var manager = new AdaptiveFormatterManager(new ComplexityAnalyzer(), new EnhancedTextFormatter());
var formatted = manager.FormatBreakdown(result.TypedResult, result.Breakdown, "repeat(min(3d6!+2, 20), 4)");

Console.WriteLine("\nFormatted Breakdown:");
Console.WriteLine(formatted);
Console.WriteLine("\nFormatted Length: " + formatted.Length);

// Analyze complexity
var complexityAnalyzer = new ComplexityAnalyzer();
var complexity = complexityAnalyzer.AnalyzeComplexity(result.TypedResult);
Console.WriteLine($"\nComplexity Level: {complexity}");