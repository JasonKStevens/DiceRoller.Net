using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;
using System;

namespace DiceRoller.Test
{
    [TestFixture]
    public class ErrorHandlingFixture
    {
        private DiceRollEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(10, 5, 15, 3, 8);
            _evaluator = new DiceRollEvaluator(sequenceGenerator);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        [TestCase("\n")]
        public void Evaluate_EmptyOrWhitespaceInput_ThrowsArgumentException(string input)
        {
            // Act & Assert
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<ArgumentException>());
        }

        [TestCase("invalid")]
        [TestCase("xyz")]
        [TestCase("1d")]
        [TestCase("d")]
        [TestCase("++")]
        [TestCase("--")]
        [TestCase("**")]
        [TestCase("//")]
        public void Evaluate_InvalidExpressions_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert  
            // The evaluator should throw InvalidOperationException for invalid syntax
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("1/0")]
        [TestCase("5/0")]
        [TestCase("d20/0")]
        public void Evaluate_DivisionByZero_ReturnsInfinity(string input)
        {
            // Act
            var result = _evaluator.Evaluate(input);

            // Assert
            // Division by zero results in positive infinity
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(double.PositiveInfinity));
        }

        [TestCase("1d0")]
        [TestCase("2d0")]
        [TestCase("d0")]
        public void Evaluate_ZeroSidedDice_ReturnsValues(string input)
        {
            // Act
            var result = _evaluator.Evaluate(input);

            // Assert
            // Zero-sided dice are handled and return actual values from the RNG
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.GreaterThan(0));
        }

        [TestCase("1d-6")]
        public void Evaluate_NegativeDiceSides_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Negative number of dice sides should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("-1d6")]
        [TestCase("-d20")]
        public void Evaluate_NegativeOfDiceRoll_EvaluatesSuccessfully(string input)
        {
            // Act & Assert
            // These are valid expressions (negative of a dice roll)
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [Ignore("These tests take too long to run and can cause timeouts")]
        [TestCase("1000000d1000000")]
        [TestCase("999999d999999")]
        public void Evaluate_ExtremelyLargeDiceValues_HandlesGracefully(string input)
        {
            // Act & Assert
            // Should handle large values without crashing, though might be slow
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [TestCase("((((1d6))))")]
        public void Evaluate_MatchedParentheses_EvaluatesSuccessfully(string input)
        {
            // Act & Assert
            // Properly matched parentheses should work fine
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [TestCase("(1d6")]
        [TestCase("1d6)")]
        [TestCase("((1d6)")]
        [TestCase("(1d6))")]
        public void Evaluate_MismatchedParentheses_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Mismatched parentheses should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("min(")]
        [TestCase("min(1")]
        [TestCase("min(1,")]
        [TestCase("min(,2)")]
        [TestCase("min()")]
        public void Evaluate_IncompleteMinFunction_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Incomplete function syntax should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("repeat(")]
        [TestCase("repeat(d6")]
        [TestCase("repeat(d6,")]
        [TestCase("repeat(,5)")]
        [TestCase("repeat()")]
        public void Evaluate_IncompleteRepeatFunction_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Incomplete function syntax should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("step")]
        [TestCase("step ")]
        public void Evaluate_IncompleteStepFunction_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Incomplete function syntax should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("1d6 * / 2")]
        [TestCase("1d6 < > 2")]
        public void Evaluate_InvalidOperatorSequences_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Invalid operator sequences should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("1d6 + + 2")]
        public void Evaluate_DoubleOperators_HandlesGracefully(string input)
        {
            // Act & Assert
            // Some double operators are handled gracefully by the parser
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [TestCase("repeat(d6, 0)")]
        public void Evaluate_ZeroRepeatCount_HandlesGracefully(string input)
        {
            // Act & Assert
            // Zero repeat count is handled gracefully
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [TestCase("repeat(d6, -1)")]
        public void Evaluate_NegativeRepeatCount_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Negative repeat count throws InvalidOperationException due to parser syntax error
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("step 0")]
        [TestCase("step -1")]
        public void Evaluate_InvalidStepValues_ThrowsException(string input)
        {
            // Act & Assert
            // Zero and negative step values throw ArgumentOutOfRangeException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase("step 1000")]
        public void Evaluate_LargeStepValue_HandlesGracefully(string input)
        {
            // Act & Assert
            // Large step values are handled gracefully
            Assert.That(() => _evaluator.Evaluate(input), Throws.Nothing);
        }

        [Test]
        public void Evaluate_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.That(() => _evaluator.Evaluate((string)null), Throws.ArgumentException.Or.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Parse_ValidExpression_ReturnsParseTree()
        {
            // Act
            var result = _evaluator.Parse("1d6");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Root, Is.Not.Null);
        }

        [Test]
        public void Parse_InvalidExpression_ThrowsInvalidOperationException()
        {
            // Act & Assert
            // Parser should throw InvalidOperationException for invalid expressions
            Assert.That(() => _evaluator.Parse("invalid expression"), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Evaluate_ComplexInvalidExpression_ThrowsInvalidOperationException()
        {
            // Arrange
            var complexInvalidExpression = "1d6 + * - / ( ) min repeat step ! @ # $ % ^ & * ( ) } { [ ]";

            // Act & Assert
            // Complex invalid syntax should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(complexInvalidExpression), Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("1d6 + abc")]
        [TestCase("xyz * 2d8")]
        [TestCase("min(abc, def)")]
        public void Evaluate_ExpressionsWithInvalidTokens_ThrowsInvalidOperationException(string input)
        {
            // Act & Assert
            // Invalid tokens should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Evaluate_VeryLongExpression_HandlesGracefully()
        {
            // Arrange
            var longExpression = string.Join(" + ", new string[100]).Replace(" + ", "1d6 + ") + "1d6";

            // Act & Assert
            Assert.That(() => _evaluator.Evaluate(longExpression), Throws.Nothing);
        }

        [Test]
        public void Evaluate_ExpressionWithSpecialCharacters_ThrowsInvalidOperationException()
        {
            // Arrange
            var specialChars = "1d6 + 2 @ # $ % ^ & * ( ) } { [ ] ; : ' \" < > ? / | \\ ~ `";

            // Act & Assert
            // Special characters should throw InvalidOperationException
            Assert.That(() => _evaluator.Evaluate(specialChars), Throws.TypeOf<InvalidOperationException>());
        }
    }
}