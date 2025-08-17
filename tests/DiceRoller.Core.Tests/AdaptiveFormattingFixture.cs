using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class AdaptiveFormattingFixture
    {
        private DiceRollEvaluator _evaluator;
        private ResultNodeConverter _converter;
        private ComplexityAnalyzer _complexityAnalyzer;
        private EnhancedTextFormatter _enhancedFormatter;
        private AdaptiveFormatterManager _adaptiveManager;
        
        [SetUp]
        public void SetUp()
        {
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 4, 1, 6, 3, 3, 3, 3, 2, 6, 11, 11, 9, 1, 3, 5, 16, 16, 14, 6, 1, 3, 4);
            _evaluator = new DiceRollEvaluator(sequenceGenerator);
            _converter = new ResultNodeConverter();
            _complexityAnalyzer = new ComplexityAnalyzer();
            _enhancedFormatter = new EnhancedTextFormatter();
            _adaptiveManager = new AdaptiveFormatterManager(_complexityAnalyzer, _enhancedFormatter);
        }

        [Test]
        public void ComplexityAnalyzer_SimpleExpression_ShouldReturnSimpleOrModerate()
        {
            // Arrange
            var result = _evaluator.Evaluate("d20");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var complexity = _complexityAnalyzer.AnalyzeComplexity(converted.TypedResult);
            
            // Assert
            Assert.That(complexity, Is.LessThanOrEqualTo(ComplexityLevel.Moderate));
        }

        [Test]
        public void ComplexityAnalyzer_ModerateExpression_ShouldReturnReasonableComplexity()
        {
            // Arrange
            var result = _evaluator.Evaluate("min(3d6+2, 15)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var complexity = _complexityAnalyzer.AnalyzeComplexity(converted.TypedResult);
            
            // Assert - Should be at least moderate but not extreme
            Assert.That(complexity, Is.GreaterThanOrEqualTo(ComplexityLevel.Moderate));
            Assert.That(complexity, Is.LessThan(ComplexityLevel.Extreme));
        }

        [Test]
        public void ComplexityAnalyzer_ComplexExpression_ShouldReturnComplexOrHigher()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(3d6, 3)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var complexity = _complexityAnalyzer.AnalyzeComplexity(converted.TypedResult);
            
            // Assert
            Assert.That(complexity, Is.GreaterThanOrEqualTo(ComplexityLevel.Complex));
        }

        [Test]
        public void ComplexityAnalyzer_VeryComplexExpression_ShouldReturnVeryComplexOrExtreme()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(min(3d6!+2, 20), 4)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var complexity = _complexityAnalyzer.AnalyzeComplexity(converted.TypedResult);
            
            // Assert
            Assert.That(complexity, Is.GreaterThanOrEqualTo(ComplexityLevel.VeryComplex));
        }

        [Test]
        public void EnhancedTextFormatter_RepeatExpression_ShouldCreateTreeStructure()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(d6, 3)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _enhancedFormatter.FormatBreakdown(converted.TypedResult, converted.Breakdown, "repeat(d6, 3)");
            
            // Assert
            Assert.That(formatted, Does.Contain("🎲"));
            Assert.That(formatted, Does.Contain("├─"));  // Tree branch symbol
            Assert.That(formatted, Does.Contain("Repeat"));
            Assert.That(formatted, Does.Contain("Iteration"));
        }

        [Test]
        public void EnhancedTextFormatter_MinExpression_ShouldShowHierarchy()
        {
            // Arrange
            var result = _evaluator.Evaluate("min(3d6, 10)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _enhancedFormatter.FormatBreakdown(converted.TypedResult, converted.Breakdown, "min(3d6, 10)");
            
            // Assert
            Assert.That(formatted, Does.Contain("🎲"));
            Assert.That(formatted, Does.Contain("min("));
            Assert.That(formatted, Does.Not.Contain("raw unformatted text"));
        }

        [Test]
        public void AdaptiveFormatterManager_SimpleExpression_ShouldUseSimpleFormatting()
        {
            // Arrange
            var result = _evaluator.Evaluate("d20");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(converted.TypedResult, converted.Breakdown, "d20");
            
            // Assert
            Assert.That(formatted, Does.Contain("`"));  // Simple expressions use code formatting
            Assert.That(formatted, Does.Not.Contain("├─"));  // Should not have tree symbols for simple expressions
        }

        [Test]
        public void AdaptiveFormatterManager_ComplexExpression_ShouldUseEnhancedFormatting()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(min(3d6, 15), 2)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(converted.TypedResult, converted.Breakdown, "repeat(min(3d6, 15), 2)");
            
            // Assert
            Assert.That(formatted, Does.Contain("🎲"));
            Assert.That(formatted, Does.Contain("├─").Or.Contain("└─"));  // Should have tree symbols for complex expressions
            Assert.That(formatted, Does.Contain("Repeat"));
        }

        [Test]
        public void DiscordFormatter_WithAdaptiveFormatting_ShouldProduceReadableOutput()
        {
            // Arrange
            var result = _evaluator.Evaluate("repeat(min(3d6!+2, 20), 3)");
            var converted = _converter.Convert(result.TypedResult);
            var context = new FormattingContext { SupportsEmojis = true, MaxLineLength = 2000 };
            var formatter = DiceHelper.CreateTestDiscordFormatter(context, ResultFormat.Detailed);
            
            // Act
            var formatted = formatter.Visit(converted, "repeat(min(3d6!+2, 20), 3)");
            
            // Assert
            Assert.That(formatted.PrimaryText, Does.Contain("🎲"));
            Assert.That(formatted.PrimaryText, Does.Contain("**Breakdown:**"));
            Assert.That(formatted.PrimaryText, Does.Not.Contain("55 12 12 10"));  // Should not contain raw unreadable output
            
            // Should use adaptive formatting for complex breakdown
            Assert.That(formatted.PrimaryText, Does.Contain("Repeat").Or.Contain("├─").Or.Contain("repeat("));
        }

        [Test]
        public void When_ExpressionIsNull_Should_ReturnFallbackFormatting()
        {
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(null, "some breakdown", "d20");
            
            // Assert
            Assert.That(formatted, Is.Not.Null);
            Assert.That(formatted, Does.Contain("some breakdown"));
        }

        [Test]
        public void When_BreakdownIsEmpty_Should_ReturnNoBreakdownMessage()
        {
            // Arrange
            var result = _evaluator.Evaluate("d20");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(converted.TypedResult, "", "d20");
            
            // Assert
            Assert.That(formatted, Does.Contain("No breakdown available"));
        }

        [Test]
        public void When_ComplexExpressionWithExplodingDice_Should_FormatProperly()
        {
            // Arrange - Use sequence that will create exploding dice
            var explodingSequenceGenerator = DiceHelper.GetSequenceGenerator(6, 6, 4, 2, 3, 5, 1, 6, 2);
            var explodingEvaluator = new DiceRollEvaluator(explodingSequenceGenerator);
            var result = explodingEvaluator.Evaluate("repeat(3d6!, 2)");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(converted.TypedResult, converted.Breakdown, "repeat(3d6!, 2)");
            
            // Assert - Adaptive manager correctly classifies as Moderate and uses code block formatting
            Assert.That(formatted, Does.Contain("repeat("));
            Assert.That(formatted, Does.Contain("iterations"));
            Assert.That(formatted, Does.Contain("!"));  // Should preserve exploding dice notation
            Assert.That(formatted, Does.Not.Contain("55 12 12 10"));  // Should not contain raw output
            Assert.That(result.Value, Is.GreaterThan(0));
        }

        [Test]
        public void When_ComparisonExpression_Should_ShowBooleanResult()
        {
            // Arrange
            var result = _evaluator.Evaluate("3d6 > 12");
            var converted = _converter.Convert(result.TypedResult);
            
            // Act
            var formatted = _adaptiveManager.FormatBreakdown(converted.TypedResult, converted.Breakdown, "3d6 > 12");
            
            // Assert
            Assert.That(formatted, Does.Contain(">"));
            Assert.That(formatted, Does.Contain("Comparison:").Or.Contain("True").Or.Contain("False"));
            Assert.That(result.Value, Is.EqualTo(0).Or.EqualTo(1)); // Boolean result
        }
    }
}