using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class EnhancedFormattingFixture
    {
        private DiceRollEvaluator _evaluator;
        private ResultNodeConverter _converter;
        
        [SetUp]
        public void SetUp()
        {
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 4, 1, 6, 3, 3, 3, 3, 2, 6, 11, 11, 9, 1, 3, 5, 16, 16, 14, 6, 1, 3, 4);
            _evaluator = new DiceRollEvaluator(sequenceGenerator);
            _converter = new ResultNodeConverter();
        }

        [Test]
        public void RepeatFunction_ShouldFormatWithIndividualIterations()
        {
            // Act
            var result = _evaluator.Evaluate("repeat(3d6, 3)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Assert
            Assert.That(converted.Breakdown, Does.Contain("repeat("));
            Assert.That(converted.Breakdown, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(result.Value, Is.GreaterThan(0));
        }

        [Test] 
        public void MinFunction_ShouldShowComparison()
        {
            // Act
            var result = _evaluator.Evaluate("min(3d6+2, 15)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Assert
            Assert.That(converted.Breakdown, Does.Contain("min("));
            Assert.That(converted.Breakdown, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(result.Value, Is.GreaterThan(0));
        }

        [Test]
        public void ExplodingDice_ShouldFormatProperly()
        {
            // Arrange - Use a sequence that will cause exploding dice (6s explode on d6!)
            var explodingSequenceGenerator = DiceHelper.GetSequenceGenerator(6, 4, 6, 2, 3);
            var explodingEvaluator = new DiceRollEvaluator(explodingSequenceGenerator);
            
            // Act 
            var result = explodingEvaluator.Evaluate("3d6!");
            var converted = _converter.Convert(result.TypedResult);
            
            // Assert
            Assert.That(converted.Breakdown, Does.Contain("!"));
            Assert.That(converted.Breakdown, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(result.Value, Is.GreaterThan(0));
        }

        [Test]
        public void Comparison_ShouldShowBooleanResult()
        {
            // Act
            var result = _evaluator.Evaluate("3d6 > 10");
            var converted = _converter.Convert(result.TypedResult);
            
            // Assert
            Assert.That(converted.Breakdown, Does.Contain(">"));
            Assert.That(converted.Breakdown, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(result.Value, Is.EqualTo(0).Or.EqualTo(1)); // Boolean result (0 or 1)
        }

        [Test]
        public void DiscordFormatter_Standard_ShouldBeReadable()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(min(3d6!+2, 20), 2)");
            var converted = _converter.Convert(result.TypedResult);
            var context = new FormattingContext { SupportsEmojis = true, MaxLineLength = 200 };
            var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
            
            // Act
            var formatted = formatter.Visit(converted, "repeat(min(3d6!+2, 20), 2)");
            
            // Assert
            Assert.That(formatted.PrimaryText, Does.Contain("🎲"));
            Assert.That(formatted.PrimaryText, Does.Contain("**"));  // Should have bold formatting
            Assert.That(formatted.PrimaryText, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(formatted.PrimaryText.Length, Is.LessThan(300)); // Should be reasonably short for standard format
        }

        [Test]
        public void DiscordFormatter_Detailed_ShouldShowBreakdown()
        {
            // Arrange
            var result = _evaluator.Evaluate("min(3d6+2, 15)");
            var converted = _converter.Convert(result.TypedResult);
            var context = new FormattingContext { SupportsEmojis = true, MaxLineLength = 2000 };
            var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Detailed);
            
            // Act
            var formatted = formatter.Visit(converted, "min(3d6+2, 15)");
            
            // Assert
            Assert.That(formatted.PrimaryText, Does.Contain("🎲"));
            Assert.That(formatted.PrimaryText, Does.Contain("**Breakdown:**"));
            Assert.That(formatted.PrimaryText, Does.Not.Contain("55 12 12 10"));  // Should not be unreadable raw output
            Assert.That(formatted.PrimaryText, Does.Contain("Expression:"));
        }

        [Test]
        public void ComplexExpression_ShouldNotShowRawTypedResultText()
        {
            // This is the main issue we're fixing - complex expressions should not show raw unreadable text
            
            // Act
            var result = _evaluator.Evaluate("repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10");
            var converted = _converter.Convert(result.TypedResult);
            
            // Assert - Should not contain the problematic raw output
            Assert.That(converted.Breakdown, Does.Not.Contain("55 12 12 10 1 5 4 + 11 11 9 1 3 5"));
            Assert.That(converted.Breakdown, Does.Not.StartWith("55 12 12 10"));
            
            // Should contain structured breakdown elements instead
            Assert.That(converted.Breakdown, Is.Not.EqualTo("No breakdown available"));
            Assert.That(converted.Breakdown.Length, Is.GreaterThan(10)); // Should have meaningful content
        }

        [Test]
        public void ComplexExpression_DiscordFormatted_ShouldBeReadable()
        {
            // Act
            var result = _evaluator.Evaluate("repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10");
            var converted = _converter.Convert(result.TypedResult);
            var context = new FormattingContext { SupportsEmojis = true, MaxLineLength = 2000 };
            var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Standard);
            var formatted = formatter.Visit(converted, "repeat(min(3d6!+2, 20), 4) + (4d10! > 30) * 10");
            
            // Assert
            Assert.That(formatted.PrimaryText, Does.Contain("🎲"));
            Assert.That(formatted.PrimaryText, Does.Contain("**"));
            Assert.That(formatted.PrimaryText, Does.Not.Contain("55 12 12 10 1 5 4 + 11 11 9 1 3 5"));
            Assert.That(formatted.PrimaryText, Does.Not.StartWith("55 12 12 10"));
            
            // Should be Discord formatted and readable
            Assert.That(result.Value, Is.GreaterThan(0));
        }
    }
}