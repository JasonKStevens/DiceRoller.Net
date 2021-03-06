using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class DiceRollFixture
    {
        [TestCase("1d2")]
        [TestCase(" 1d2")]
        [TestCase("1 d2")]
        [TestCase("1d 2")]
        [TestCase("1d2 ")]
        public void DiceRoll_Whitespace_NoDifference(string roll)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(1));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[1]"));
        }

        [Test]
        public void DiceRoll_SingleDice_RollWithSum()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("1d2");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(1));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[1]"));
        }

        [Test]
        public void DiceRoll_NoDice_DefaultsTo1()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("d2");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(1));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[1]"));
        }

        [Test]
        public void DiceRoll_MultipleDice_RollWithSum()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1, 2, 1);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("3d2");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(4));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[1, 2, 1]"));
        }

        [Test]
        public void DiceRoll_LargeDice_RollWithSum()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(700, 300);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("2d1000");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(1000));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[700, 300]"));
        }

        [Test]
        public void Breakdown_1()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(85, 24, 99, 5, 56, 97, 71);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("repeat(d100,7)");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(437));
            Assert.That(evaluation.Breakdown, Is.EqualTo("([85], [24], [99], [5], [56], [97], [71])"));
        }

    }
}