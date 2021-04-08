using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class MinFixture
    {
        [Test]
        [TestCase("(d10-6) min 1", 1)]
        [TestCase("(d10-66) min 2", 2)]
        [TestCase("(d10-4) min 2", 2)]
        [TestCase("((d10-5) min 1) min 4", 4)]
        public void WillTakeMinWhenDieIsLessThanMin(string roll, float expected)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 5);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(expected));
        }
    }
}