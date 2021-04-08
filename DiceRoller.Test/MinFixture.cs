using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class MinFixture
    {
        [Test]
        [TestCase("min(d10-6,1)", 1)]
        [TestCase("min(d10-66,2)", 2)]
        [TestCase("min(d10-4,2)", 2)]
        [TestCase("min(d10-5+1,4)", 4)]
        public void WillTakeMinWhenDieIsLessThanMin(string roll, float expected)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(expected));
        }
    }

    public class RepeatFixture
    {
        [Test]
        public void RepeatsASimpleRoll()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1,2,3,4,5,6,7,8,9,10);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("repeat(2d10,5)");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(55));
        }
    }
}