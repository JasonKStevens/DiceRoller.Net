using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class RepeatFixture
    {
        [Test]
        [TestCase("repeat(2d10,5)", 50)]
        [TestCase("repeat(min(d10-9,1),5)", 5)]
        public void RepeatsASimpleRoll(string roll, float expected)
        {
            // Arrange
            var generator = DiceHelper.GetMoqGeneratorForStaticResult(4);
            var evaluator = new Evaluator(generator);

            // Act
            var evaluation = evaluator.Evaluate(roll);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(expected));
        }
    }
}