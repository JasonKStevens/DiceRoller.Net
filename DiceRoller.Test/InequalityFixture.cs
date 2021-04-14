using DiceRoller.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    public class InequalityFixture
    {
        [TestCase("1<2", 1f)]
        [TestCase("1<1", 0f)]
        [TestCase("1<=1", 1f)]
        [TestCase("2<=1", 0f)]
        [TestCase("1=1", 1f)]
        [TestCase("2=1", 0f)]
        [TestCase("1>=1", 1f)]
        [TestCase("1>=2", 0f)]
        [TestCase("2>1", 1f)]
        [TestCase("1>1", 0f)]
        public void Equation_Inquality_Solves(string equation, float answer)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator();
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(equation);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(answer));
        }

        [TestCase("d2<2", 1f)]
        [TestCase("repeat(d2<2, 2)", 1f)]
        public void Expression_Inequality_Solves(string equation, float answer)
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(1, 2);
            var evaluator = new Evaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate(equation);

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(answer));
        }
    }
}