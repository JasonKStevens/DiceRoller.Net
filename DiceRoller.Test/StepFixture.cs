using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class StepFixture
    {
        [Test]
        [TestCase("step 1", 3)]
        [TestCase("step 2", 4)]
        [TestCase("step 3", 5)]
        [TestCase("step 4", 5)]
        [TestCase("step 5", 5)]
        [TestCase("step 6", 5)]
        [TestCase("step 7", 5)]
        [TestCase("step 8", 10)]
        [TestCase("step 9", 10)]
        [TestCase("step 10", 10)]
        [TestCase("step 11", 10)]
        [TestCase("step 12", 10)]
        [TestCase("step 13", 10)]
        [TestCase("step 14", 10)]
        [TestCase("step 15", 15)]
        [TestCase("step 16", 15)]
        [TestCase("step 17", 15)]
        [TestCase("step 18", 15)]
        [TestCase("step 19", 15)]
        [TestCase("step 20", 15)]
        [TestCase("step 30", 20)]
        [TestCase("step 41", 25)]
        [TestCase("step 52", 30)]
        [TestCase("step 63", 35)]
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