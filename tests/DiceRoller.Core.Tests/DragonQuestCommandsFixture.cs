using DiceRoller.DragonQuest;
using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class DragonQuestCommandsFixture
    {
        private GrievousInjuries _injuries;
        private Backfires _backfires;
        private FearResult _fears;

        [SetUp]
        public void SetUp()
        {
            _injuries = new GrievousInjuries();
            _backfires = new Backfires();
            _fears = new FearResult();
        }

        [TestCase(1, "Congratulations! It's a bleeder in your primary arm!")]
        [TestCase(5, "Congratulations! It's a bleeder in your primary arm!")]
        [TestCase(6, "Oh no! Your opponent's weapon has entered your secondary arm's elbow joint")]
        [TestCase(11, "Your aorta is severed and you are quite dead.")]
        [TestCase(50, "A slash along one arm, and it's a bleeder!")]
        [TestCase(100, "Crushing blow to your pelvis")]
        public void InjuryCommand_ValidRolls_ReturnsCorrectResults(int roll, string expectedStartOfText)
        {
            // Act
            var result = _injuries.LookupResult(roll);

            // Assert - normalize quotes to handle Unicode apostrophes
            var normalizedResult = result.Replace('\u2019', '\'').Replace('\u2018', '\'');
            var normalizedExpected = expectedStartOfText.Replace('\u2019', '\'').Replace('\u2018', '\'');
            Assert.That(normalizedResult.StartsWith(normalizedExpected), Is.True, 
                $"Expected result to start with '{expectedStartOfText}' but was '{result}'");
        }

        [TestCase(9, "Fatigue loss equal to that already expended.")]
        [TestCase(16, "Fatigue loss equal to twice that already expended.")]
        [TestCase(25, "Fatigue loss equal to 5 times that already expended.")]
        [TestCase(30, "The Adept becomes the target of the spell.")]
        [TestCase(58, "The spell works as normal.")]
        [TestCase(61, "Blind for D10 pulses.")]
        [TestCase(100, "Roll two more times and apply both effects.")]
        public void BackfireCommand_ValidRolls_ReturnsCorrectResults(int roll, string expectedResult)
        {
            // Act
            var result = _backfires.LookupResult(roll);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase(20, "[Wary] - The target will not voluntarily approach")]
        [TestCase(25, "[Berserk]- They immediately charge to attack")]
        [TestCase(75, "[Panic]- They will attempt to maximise their safety")]
        [TestCase(90, "[Frozen]- They may take no action until snapped out")]
        [TestCase(95, "[Hysterical]- They stand and scream")]
        [TestCase(100, "[Catatonic]- Target becomes catatonic")]
        [TestCase(110, "[Faints]- The target faints into unconsciousness")]
        [TestCase(115, "[Collapses]- The target collapses into unconsciousness")]
        [TestCase(200, "[Heart Attack]- The target suffers a heart attack")]
        public void FearCommand_ValidRolls_ReturnsCorrectResults(int roll, string expectedStartOfText)
        {
            // Act
            var result = _fears.LookupResult(roll);

            // Assert
            Assert.That(result.StartsWith(expectedStartOfText), Is.True,
                $"Expected result to start with '{expectedStartOfText}' but was '{result}'");
        }

        [Test]
        public void InjuryCommand_WithDiceRollEvaluator_ReturnsValidResult()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(50);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("injury");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(50));
            Assert.That(evaluation.Breakdown, Does.Contain("slash along one arm"));
        }

        [Test]
        public void BackfireCommand_WithDiceRollEvaluator_ReturnsValidResult()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(58);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("backfire");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(58));
            Assert.That(evaluation.Breakdown, Is.EqualTo("The spell works as normal."));
        }

        [Test]
        public void FearCommand_WithDiceRollEvaluator_ReturnsValidResult()
        {
            // Arrange
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(75);
            var evaluator = new DiceRollEvaluator(sequenceGenerator);

            // Act
            var evaluation = evaluator.Evaluate("fear");

            // Assert
            Assert.That(evaluation.Value, Is.EqualTo(75));
            Assert.That(evaluation.Breakdown, Does.Contain("[Panic]"));
        }

        [TestCase(0)]
        [TestCase(101)]
        public void InjuryCommand_OutOfBounds_ThrowsException(int roll)
        {
            // Assert
            Assert.That(() => _injuries.LookupResult(roll), Throws.ArgumentException);
        }

        [TestCase(0)]
        [TestCase(101)]
        public void BackfireCommand_OutOfBounds_ThrowsException(int roll)
        {
            // Assert
            Assert.That(() => _backfires.LookupResult(roll), Throws.ArgumentException);
        }

        [TestCase(0)]
        [TestCase(201)]
        public void FearCommand_OutOfBounds_ThrowsException(int roll)
        {
            // Assert
            Assert.That(() => _fears.LookupResult(roll), Throws.ArgumentException);
        }

        [Test]
        public void InjuryCommand_EdgeCases_HandledCorrectly()
        {
            // Test minimum and maximum valid values
            Assert.That(() => _injuries.LookupResult(1), Throws.Nothing);
            Assert.That(() => _injuries.LookupResult(100), Throws.Nothing);
        }

        [Test]
        public void BackfireCommand_EdgeCases_HandledCorrectly()
        {
            // Test minimum and maximum valid values
            Assert.That(() => _backfires.LookupResult(1), Throws.Nothing);
            Assert.That(() => _backfires.LookupResult(100), Throws.Nothing);
        }

        [Test]
        public void FearCommand_EdgeCases_HandledCorrectly()
        {
            // Test minimum and maximum valid values
            Assert.That(() => _fears.LookupResult(1), Throws.Nothing);
            Assert.That(() => _fears.LookupResult(200), Throws.Nothing);
        }
    }
}