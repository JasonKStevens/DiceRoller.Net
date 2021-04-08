using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class ExplodingDiceFixture
    {
        [Test]
        public void Exploding_SingleDice_SingleExplosion_NoInitialNumber()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 1);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("d2!");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(3));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[2!, 1]"));
        }

        [Test]
        public void Exploding_SingleDice_SingleExplosion()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 1);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("1d2!");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(3));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[2!, 1]"));
        }

        [Test]
        public void DiceRoll_SingleDice_MultipleExplosions()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 2, 1);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("1d2!");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(5));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[2!, 2!, 1]"));
        }

        [Test]
        public void DiceRoll_MultipleDice_MultipleExplosions()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(2, 2, 1, 2, 1);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("2d2!");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(8));
            Assert.That(evaluation.Breakdown, Is.EqualTo("[2!, 2!, 1, 2!, 1]"));
        }
    }
}