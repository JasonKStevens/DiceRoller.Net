using NUnit.Framework;
using DiceRoller.DragonQuest;
using System;

namespace DiceRoller.Test
{
    [TestFixture]
    public class InjuryFixture
    {
        private GrievousInjuries _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new GrievousInjuries();
        }

        [TestCase(1, "Congratulations! It’s a bleeder")]
        [TestCase(5, "Congratulations! It’s a bleeder")]
        [TestCase(6, "Oh no! Your opponent’s weapon has")]
        [TestCase(11, "Your aorta is severed and you are quite dead.")]
        [TestCase(100, "Crushing blow to your pelvis")]
        public void InjuryRoll_WithinBounds_ReturnsCorrectResult(int diceRoll, string startOfText)
        {
            // Act
            var injury = _sut.GetInjury(diceRoll);

            // Assert
            Assert.That(injury.StartsWith(startOfText), Is.True);
        }

        [TestCase(0)]
        [TestCase(101)]
        public void InjuryRoll_OutsideOfBounds_Throws(int diceRoll)
        {
            // Assert
            Assert.That(() => _sut.GetInjury(diceRoll), Throws.TypeOf<ArgumentException>());
        }
    }
}