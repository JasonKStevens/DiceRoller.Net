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

        [Test]
        public void sdfst()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("d10 # test comment for testing tests");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(5));
        }        
    }
}