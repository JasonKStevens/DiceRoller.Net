using NUnit.Framework;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;

namespace DiceRoller.Test;

[TestFixture]
public class IntelligentTruncationFixture
{
    private DiceRollEvaluator _evaluator;
    private IResultNodeConverter _converter;
    
    [SetUp]
    public void SetUp()
    {
        // Create a sequence of dice rolls that will produce large results
        var longSequence = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            longSequence.Add(100); // Each die shows 100 for maximum explosion
        }
        
        var sequenceGenerator = DiceHelper.GetSequenceGenerator(longSequence.ToArray());
        _evaluator = new DiceRollEvaluator(sequenceGenerator);
        _converter = new ResultNodeConverter();
    }

    [Test]
    public void When_result_under_2000_chars_Should_not_truncate()
    {
        // Given - Simple roll that won't be long
        var result = _evaluator.Evaluate("3d6");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format with Discord formatter
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
        var formattedResult = formatter.Visit(diceResultNode, "3d6");
        
        // Then - Should not be truncated
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Does.Not.Contain("..."));
        Assert.That(formattedResult.PrimaryText, Does.Not.Contain("truncated"));
    }

    [Test]
    public void When_exploding_dice_result_too_long_Should_truncate_intelligently()
    {
        // Given - Large exploding dice that will generate very long output
        var result = _evaluator.Evaluate("20d100!");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format with Discord formatter using intelligent truncation
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
        var formattedResult = formatter.Visit(diceResultNode, "20d100!");
        
        // Then - Should be truncated but preserve essential information
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("🎲"));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("...").Or.Contains("showing first"));
    }

    [Test]
    public void When_detailed_format_too_long_Should_truncate_preserving_structure()
    {
        // Given - Large result in detailed format
        var result = _evaluator.Evaluate("15d100!");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format with detailed format
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Detailed);
        var formattedResult = formatter.Visit(diceResultNode, "15d100!");
        
        // Then - Should maintain structure but truncate breakdown
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("**Dice Roll Result**"));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("**Result:**"));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
    }

    [Test]
    public void When_breakdown_very_long_Should_summarize_with_sample()
    {
        // Given - Very long breakdown from many exploding dice
        var result = _evaluator.Evaluate("25d100!");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Apply intelligent truncation to breakdown
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
        var formattedResult = formatter.Visit(diceResultNode, "25d100!");
        
        // Then - Should show summary with sample results
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
        
        // Should either show sample dice or indicate truncation
        var hasShowingFirst = formattedResult.PrimaryText.Contains("showing first");
        var hasTruncationIndicator = formattedResult.PrimaryText.Contains("...");
        Assert.That(hasShowingFirst || hasTruncationIndicator, Is.True);
    }

    [Test]
    public void When_multiple_lines_too_long_Should_preserve_essential_sections()
    {
        // Given - Complex expression that generates multi-line output
        var result = _evaluator.Evaluate("repeat(10d100!, 3)");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format with detailed view
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Detailed);
        var formattedResult = formatter.Visit(diceResultNode, "repeat(10d100!, 3)");
        
        // Then - Should keep headers and result, truncate breakdown details
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("**Result:**"));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
    }

    [Test] 
    public void When_terse_format_requested_Should_always_fit_in_limit()
    {
        // Given - Large result but terse format requested
        var result = _evaluator.Evaluate("50d100!");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format in terse mode
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Terse);
        var formattedResult = formatter.Visit(diceResultNode, "50d100!");
        
        // Then - Should always be very short
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(100));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
    }

    [Test]
    public void When_comment_present_Should_preserve_comment_in_truncated_result()
    {
        // Given - Large result with comment
        var result = _evaluator.Evaluate("20d100! # massive damage roll");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format and truncate
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
        var formattedResult = formatter.Visit(diceResultNode, "20d100! # massive damage roll");
        
        // Then - Should preserve comment even when truncated
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        Assert.That(formattedResult.PrimaryText, Contains.Substring("massive damage roll"));
        Assert.That(formattedResult.PrimaryText, Contains.Substring(diceResultNode.Value.ToString()));
    }

    [Test]
    public void When_truncation_applied_Should_indicate_more_details_available()
    {
        // Given - Result that requires truncation
        var result = _evaluator.Evaluate("30d100!");
        var diceResultNode = _converter.Convert(result.TypedResult);
        
        // When - Format with truncation
        var context = CreateDiscordContext();
        var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
        var formattedResult = formatter.Visit(diceResultNode, "30d100!");
        
        // Then - Should indicate there are more details available
        Assert.That(formattedResult.PrimaryText.Length, Is.LessThan(2000));
        
        // Should have some indicator that content was truncated or summarized
        var indicators = new[] { "...", "showing first", "truncated", "summary" };
        var hasIndicator = indicators.Any(indicator => 
            formattedResult.PrimaryText.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        
        Assert.That(hasIndicator, Is.True, "Should indicate when content is truncated");
    }

    private FormattingContext CreateDiscordContext()
    {
        return new FormattingContext
        {
            Platform = "discord",
            IsInteractive = true,
            SupportsStyling = true,
            SupportsEmojis = true,
            SupportsMarkdown = true,
            SupportsHtml = false,
            MaxLineLength = 2000,
            MaxLines = 50,
            UserId = "test_user",
            SessionId = "test_session",
            IsMobile = false,
            UserAgent = "Discord Bot"
        };
    }
}