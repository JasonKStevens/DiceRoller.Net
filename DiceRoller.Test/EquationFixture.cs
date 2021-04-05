using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class EquationFixture
    {
        [TestCase("2.5+(5-1)*5", 22.5f)]
        [TestCase("2.5+(5-1)*1d5", 22.5f)]
        [TestCase("2.5+(1d5-1)*5", 22.5f)]
        [TestCase("2.5+(1d5-1)*1d5", 22.5f)]
        public void DiceRoll_PartOfEquation_Solves(string equation, float answer)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(5, 5);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(equation);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(answer));
        }
    }
}