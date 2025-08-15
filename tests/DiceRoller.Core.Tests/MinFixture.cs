using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class MinFixture
    {
        [Test]
        [TestCase("min(d10-6,1)", -1)]  // min(5-6,1) = min(-1,1) = -1
        [TestCase("min(d10-66,2)", -61)]  // min(5-66,2) = min(-61,2) = -61
        [TestCase("min(d10-4,2)", 1)]  // min(5-4,2) = min(1,2) = 1
        [TestCase("min(d10-5+1,4)", 1)]  // min(5-5+1,4) = min(1,4) = 1
        public void WillTakeMinWhenDieIsLessThanMin(string roll, float expected)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

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
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("d10 # test comment for testing tests");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(5));
        }        
    }
}