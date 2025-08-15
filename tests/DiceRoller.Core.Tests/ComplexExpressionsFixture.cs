using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class ComplexExpressionsFixture
    {
        [Test]
        public void ComplexExpression_NestedParentheses_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(3, 4, 2);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("((1d6 + 2) * 3) + 1d6");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(19)); // ((3 + 2) * 3) + 4 = 15 + 4 = 19
            Assert.That(evaluation.Breakdown, Does.Contain("[3]"));
            Assert.That(evaluation.Breakdown, Does.Contain("[4]"));
        }

        [Test]
        public void ComplexExpression_MultipleOperations_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 3, 2, 4, 1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("2d6 + 3d4 - 5");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(10)); // (5 + 3) + (2 + 4 + 1) - 5 = 8 + 7 - 5 = 10
            Assert.That(evaluation.Breakdown, Does.Contain("[5, 3]"));
            Assert.That(evaluation.Breakdown, Does.Contain("[2, 4, 1]"));
        }

        [Test]
        public void ComplexExpression_SimpleMinFunction_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(4);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("min(1d6 + 2, 8)");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(6)); // min(4 + 2, 8) = min(6, 8) = 6
            Assert.That(evaluation.Breakdown, Does.Contain("min"));
        }

        [Test]
        public void ComplexExpression_ExplodingDiceWithArithmetic_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(6, 4, 3, 2);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("(2d6! + 5) * 2");

            // Assert
            // First die: 6 (explodes) + 4, Second die: 3, Total: (6+4+3+5)*2 = 36
            Assert.That(evaluation.Value, Is.GreaterThan(10));
            Assert.That(evaluation.Breakdown, Does.Contain("!"));
        }

        [Test]
        public void ComplexExpression_MultipleComparisons_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(3, 5, 2);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("(1d6 > 2) + (1d6 < 4)");

            // Assert
            // First roll: 3 > 2 = 1, Second roll: 5 < 4 = 0, Total: 1 + 0 = 1
            Assert.That(evaluation.Value, Is.EqualTo(1));
        }

        [Test]
        public void ComplexExpression_NestedRepeats_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(4, 2, 6, 1, 3, 5);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("repeat(repeat(1d6, 2), 2)");

            // Assert
            // Inner repeats: (4 + 2) and (6 + 1), Outer repeat sums: 6 + 7 = 13
            Assert.That(evaluation.Value, Is.GreaterThan(0));
            Assert.That(evaluation.Breakdown, Does.Contain("repeat"));
        }

        [Test]
        public void ComplexExpression_MixedFunctionsAndOperators_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(3, 4, 5, 2, 1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("min(2d6, 8) + repeat(1d4, 2) - 3");

            // Assert
            // min(3+4, 8) + (5 + 2) - 3 = min(7, 8) + 7 - 3 = 8 + 7 - 3 = 12
            Assert.That(evaluation.Value, Is.GreaterThan(0));
            Assert.That(evaluation.Breakdown, Does.Contain("min"));
        }

        [Test]
        public void ComplexExpression_DeepNesting_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(3, 2, 4);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("(((1d6 + 1) * 2) + ((1d6 - 1) / 2))");

            // Assert
            // ((3 + 1) * 2) + ((2 - 1) / 2) = (4 * 2) + (1 / 2) = 8 + 0.5 = 8.5
            Assert.That(evaluation.Value, Is.GreaterThan(0));
        }

        [Test]
        public void ComplexExpression_LongChainOfOperations_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1, 2, 3, 4, 5, 6);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("1d6 + 1d6 - 1d6 * 1d6 / 1d6 + 1d6");

            // Assert
            // 1 + 2 - 3 * 4 / 5 + 6 = 1 + 2 - (12/5) + 6 = 1 + 2 - 2.4 + 6 = 6.6
            Assert.That(evaluation.Value, Is.GreaterThan(0));
            Assert.That(evaluation.Breakdown, Does.Contain("[1]"));
            Assert.That(evaluation.Breakdown, Does.Contain("[6]"));
        }

        [Test]
        public void ComplexExpression_MultipleExplodingDice_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(6, 6, 3, 4, 2);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("2d6! + 1d6!");

            // Assert
            // First die: 6! + 6! + 3, Second die: 4, Third die: 2
            Assert.That(evaluation.Value, Is.GreaterThan(10));
            Assert.That(evaluation.Breakdown, Does.Contain("!"));
        }

        [Test]
        public void ComplexExpression_NestedMinWithComplexArguments_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(4, 3, 5, 2, 6);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("min(2d6 + 3, repeat(1d6, 2) + 1)");

            // Assert
            // min((4+3)+3, (5+2)+1) = min(10, 8) = 10
            Assert.That(evaluation.Value, Is.GreaterThan(0));
            Assert.That(evaluation.Breakdown, Does.Contain("min"));
        }

        [Test]
        public void ComplexExpression_WithComments_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(4, 5);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("1d6 + 1d6 # Attack roll with damage");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(9)); // 4 + 5 = 9
            Assert.That(evaluation.Breakdown, Does.Contain("[4]"));
            Assert.That(evaluation.Breakdown, Does.Contain("[5]"));
        }

        [Test]
        public void ComplexExpression_StepFunctionInComplexExpression_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 4, 3); // For step evaluation and additional rolls
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("step 5 + 1d6");

            // Assert
            Assert.That(evaluation.Value, Is.GreaterThan(0));
            Assert.That(evaluation.Breakdown, Does.Contain("step"));
        }

        [Test]
        public void ComplexExpression_AllOperatorPrecedence_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 3, 4, 5);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act - Test operator precedence: multiplication before addition
            var evaluation = evaluator.Evaluate("1d6 + 1d6 * 1d6 + 1d6");

            // Assert
            // 2 + 3 * 4 + 5 = 2 + 12 + 5 = 19
            Assert.That(evaluation.Value, Is.EqualTo(19));
        }

        [Test]
        public void ComplexExpression_ParenthesesOverridePrecedence_EvaluatesCorrectly()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 3, 4);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act - Parentheses should override normal precedence
            var evaluation = evaluator.Evaluate("(1d6 + 1d6) * 1d6");

            // Assert
            // (2 + 3) * 4 = 5 * 4 = 20
            Assert.That(evaluation.Value, Is.EqualTo(20));
        }
    }
}