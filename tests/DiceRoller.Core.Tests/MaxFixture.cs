using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class MaxFixture
    {
        [Test]
        [TestCase("max(d10+6,1)", 11)]  // max(5+6,1) = max(11,1) = 11
        [TestCase("max(d10-6,2)", 2)]   // max(5-6,2) = max(-1,2) = 2
        [TestCase("max(d10-4,2)", 2)]   // max(5-4,2) = max(1,2) = 2
        [TestCase("max(d10+5,4)", 10)]  // max(5+5,4) = max(10,4) = 10
        public void WillTakeMaxWhenApplicable(string roll, float expected)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(expected));
        }
    }
}