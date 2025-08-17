using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Test.Helpers;

// Create a test to examine the actual problematic output
var sequenceGenerator = DiceHelper.GetSequenceGenerator(6, 4, 3, 2, 1, 5, 6, 6, 3, 4, 2, 1, 6, 3, 2, 5, 4, 1, 2, 3, 4, 5);
var evaluator = new DiceRollEvaluator(sequenceGenerator);

Console.WriteLine("=== COMPLEX RESULTS PRESENTATION ANALYSIS ===\n");

// Test the problematic expression mentioned in the options document
var result = evaluator.Evaluate("repeat(min(3d6!+2, 20), 4)");

Console.WriteLine("Expression: repeat(min(3d6!+2, 20), 4)");
Console.WriteLine($"Value: {result.Value}");
Console.WriteLine("Raw Breakdown:");
Console.WriteLine("'" + result.Breakdown + "'");
Console.WriteLine($"\nBreakdown Length: {result.Breakdown.Length}");

// Test with current formatter
var converter = new ResultNodeConverter();
var diceNode = converter.Convert(result.TypedResult);
var manager = new AdaptiveFormatterManager(new ComplexityAnalyzer(), new EnhancedTextFormatter());
var formatted = manager.FormatBreakdown(result.TypedResult, result.Breakdown, "repeat(min(3d6!+2, 20), 4)");

Console.WriteLine("\nFormatted Breakdown:");
Console.WriteLine(formatted);
Console.WriteLine($"\nFormatted Length: {formatted.Length}");

// Analyze complexity
var complexityAnalyzer = new ComplexityAnalyzer();
var complexity = complexityAnalyzer.AnalyzeComplexity(result.TypedResult);
Console.WriteLine($"\nComplexity Level: {complexity}");

// Test with other expressions to show patterns
Console.WriteLine("\n=== OTHER PROBLEMATIC EXPRESSIONS ===");

var expressions = new[]
{
    "repeat(3d6!, 5)",
    "min(repeat(2d10!, 3), 50) + max(3d8!, 15)",
    "repeat(min(3d6!+2, 20), 6)",
    "20d100!",
    "(3d6! > 15) + (4d8! < 20) + (repeat(2d4, 3) >= 10)"
};

foreach (var expr in expressions)
{
    try
    {
        var testResult = evaluator.Evaluate(expr);
        var testComplexity = complexityAnalyzer.AnalyzeComplexity(testResult.TypedResult);
        var testFormatted = manager.FormatBreakdown(testResult.TypedResult, testResult.Breakdown, expr);
        
        Console.WriteLine($"\nExpression: {expr}");
        Console.WriteLine($"Value: {testResult.Value}");
        Console.WriteLine($"Complexity: {testComplexity}");
        Console.WriteLine($"Raw breakdown length: {testResult.Breakdown.Length}");
        Console.WriteLine($"Formatted length: {testFormatted.Length}");
        Console.WriteLine($"Lines in formatted: {testFormatted.Split('\n').Length}");
        
        // Show first 200 chars of breakdown
        var preview = testResult.Breakdown.Length > 200 
            ? testResult.Breakdown.Substring(0, 200) + "..."
            : testResult.Breakdown;
        Console.WriteLine($"Raw preview: '{preview}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error with '{expr}': {ex.Message}");
    }
}