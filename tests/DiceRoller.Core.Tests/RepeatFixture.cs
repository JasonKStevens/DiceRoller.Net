using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class RepeatFixture
    {
        [Test]
        [TestCase("repeat(2d10,5)", 50)]
        [TestCase("repeat(min(d10-9,1),5)", -20)]  // repeat(min(5-9,1),5) = repeat(-4,5) = -20
        public void RepeatsASimpleRoll(string roll, float expected)
        {
            // Arrange
            var generator = DiceHelper.GetMoqGeneratorForStaticResult(4);
            var evaluator = new DiceRollEvaluator(generator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(expected));
        }
    }
}