using DiceRoller.Heroes;
using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class HeroesCommandsFixture
    {
        private SpeedTable _speedTable;

        [SetUp]
        public void SetUp()
        {
            _speedTable = new SpeedTable();
        }

        [TestCase(1, "7")]
        [TestCase(2, "6, 12")]
        [TestCase(3, "4, 8, 12")]
        [TestCase(4, "3, 6, 9, 12")]
        [TestCase(5, "3, 5, 8, 10, 12")]
        [TestCase(6, "2, 4, 6, 8, 10, 12")]
        [TestCase(7, "2, 4, 6, 7, 9, 11, 12")]
        [TestCase(8, "2, 3, 5, 6, 8, 9, 11, 12")]
        [TestCase(9, "2, 3, 4, 6, 7, 8, 10, 11, 12")]
        [TestCase(10, "2, 3, 4, 5, 6, 8, 9, 10, 11, 12")]
        [TestCase(11, "2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12")]
        [TestCase(12, "1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12")]
        public void SpeedCommand_ValidSpeeds_ReturnsCorrectResult(int speed, string expectedResult)
        {
            // Act
            var result = _speedTable.LookupResult(speed);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void SpeedTable_WithCustomInput_ReturnsValidResult()
        {
            // Arrange - Test the table directly since "speed" command is not part of core parser
            var speed = 6;

            // Act
            var result = _speedTable.LookupResult(speed);

            // Assert
            Assert.That(result, Is.EqualTo("2, 4, 6, 8, 10, 12"));
        }

        [Test]
        public void SpeedTable_GetRoll_ReturnsCorrectDefaultRoll()
        {
            // Act
            var defaultRoll = _speedTable.GetRoll();

            // Assert
            Assert.That(defaultRoll, Is.EqualTo("1"));
        }

        [Test]
        public void SpeedTable_GetMaxRoll_ReturnsCorrectMaximum()
        {
            // Act
            var maxRoll = _speedTable.GetMaxRoll();

            // Assert
            Assert.That(maxRoll, Is.EqualTo(12));
        }

        [TestCase(0)]
        [TestCase(13)]
        public void SpeedCommand_OutOfBounds_ThrowsException(int speed)
        {
            // Assert
            Assert.That(() => _speedTable.LookupResult(speed), Throws.ArgumentException);
        }

        [Test]
        public void SpeedCommand_EdgeCases_HandledCorrectly()
        {
            // Test minimum and maximum valid values
            Assert.That(() => _speedTable.LookupResult(1), Throws.Nothing);
            Assert.That(() => _speedTable.LookupResult(12), Throws.Nothing);
        }

        [Test]
        public void SpeedCommand_Speed1_ReturnsOnlySegment7()
        {
            // Act
            var result = _speedTable.LookupResult(1);

            // Assert
            Assert.That(result, Is.EqualTo("7"));
            Assert.That(result.Split(',').Length, Is.EqualTo(1));
        }

        [Test]
        public void SpeedCommand_Speed12_ReturnsAllSegments()
        {
            // Act
            var result = _speedTable.LookupResult(12);

            // Assert
            Assert.That(result, Is.EqualTo("1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12"));
            Assert.That(result.Split(',').Length, Is.EqualTo(12));
        }

        [Test]
        public void SpeedCommand_Speed6_ContainsEvenSegments()
        {
            // Act
            var result = _speedTable.LookupResult(6);

            // Assert
            Assert.That(result, Is.EqualTo("2, 4, 6, 8, 10, 12"));
            
            var segments = result.Split(',');
            Assert.That(segments.Length, Is.EqualTo(6));
            
            // Verify all segments are even
            foreach (var segment in segments)
            {
                var segmentNumber = int.Parse(segment.Trim());
                Assert.That(segmentNumber % 2, Is.EqualTo(0), $"Segment {segmentNumber} should be even");
            }
        }
    }
}