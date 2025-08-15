using DiceRoller.DragonQuest;
using DiceRoller.Heroes;
using NUnit.Framework;
using System;

namespace DiceRoller.Test
{
    [TestFixture]
    public class LocationTablesFixture
    {
        #region DragonQuest Humanoid Location Tables

        [TestFixture]
        public class HumanoidHighLocationTableTests
        {
            private HumanoidHighLocationsTable _table;

            [SetUp]
            public void SetUp()
            {
                _table = new HumanoidHighLocationsTable();
            }

            [TestCase(16, "Skull")]
            [TestCase(27, "Face/Eye*")]
            [TestCase(43, "Neck")]
            [TestCase(75, "Shoulder*")]
            [TestCase(79, "Upper Arm*")]
            [TestCase(81, "Elbow*")]
            [TestCase(85, "Forearm*")]
            [TestCase(89, "Hand/Wrist*")]
            [TestCase(94, "Wing*†")]
            [TestCase(97, "Thorax")]
            [TestCase(98, "Abdomen")]
            [TestCase(100, "Hip/Pelvis*")]
            public void HumanoidHighLocation_ValidRolls_ReturnsCorrectLocation(int roll, string expectedLocation)
            {
                // Act
                var result = _table.LookupResult(roll);

                // Assert
                Assert.That(result, Is.EqualTo(expectedLocation));
            }

            [Test]
            public void HumanoidHighLocation_GetRoll_ReturnsD100()
            {
                // Act
                var roll = _table.GetRoll();

                // Assert
                Assert.That(roll, Is.EqualTo("d100"));
            }

            [Test]
            public void HumanoidHighLocation_GetMaxRoll_Returns100()
            {
                // Act
                var maxRoll = _table.GetMaxRoll();

                // Assert
                Assert.That(maxRoll, Is.EqualTo(100));
            }

            [TestCase(0)]
            [TestCase(101)]
            public void HumanoidHighLocation_OutOfBounds_ThrowsException(int roll)
            {
                // Assert
                Assert.That(() => _table.LookupResult(roll), Throws.ArgumentException);
            }
        }

        [TestFixture]
        public class HumanoidArmsLocationTableTests
        {
            private HumanoidArmsLocationsTable _table;

            [SetUp]
            public void SetUp()
            {
                _table = new HumanoidArmsLocationsTable();
            }

            [Test]
            public void HumanoidArmsLocation_ValidRange_DoesNotThrow()
            {
                // Test that all values 1-100 are handled
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _table.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Humanoid Arms table");
                }
            }

            [Test]
            public void HumanoidArmsLocation_GetRoll_ReturnsD100()
            {
                // Act
                var roll = _table.GetRoll();

                // Assert
                Assert.That(roll, Is.EqualTo("d100"));
            }
        }

        [TestFixture]
        public class HumanoidMidLocationTableTests
        {
            private HumanoidMidLocationsTable _table;

            [SetUp]
            public void SetUp()
            {
                _table = new HumanoidMidLocationsTable();
            }

            [Test]
            public void HumanoidMidLocation_ValidRange_DoesNotThrow()
            {
                // Test that all values 1-100 are handled
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _table.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Humanoid Mid table");
                }
            }

            [Test]
            public void HumanoidMidLocation_GetRoll_ReturnsD100()
            {
                // Act
                var roll = _table.GetRoll();

                // Assert
                Assert.That(roll, Is.EqualTo("d100"));
            }
        }

        [TestFixture]
        public class HumanoidLegsLocationTableTests
        {
            private HumanoidLegsLocationsTable _table;

            [SetUp]
            public void SetUp()
            {
                _table = new HumanoidLegsLocationsTable();
            }

            [Test]
            public void HumanoidLegsLocation_ValidRange_DoesNotThrow()
            {
                // Test that all values 1-100 are handled
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _table.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Humanoid Legs table");
                }
            }

            [Test]
            public void HumanoidLegsLocation_GetRoll_ReturnsD100()
            {
                // Act
                var roll = _table.GetRoll();

                // Assert
                Assert.That(roll, Is.EqualTo("d100"));
            }
        }

        #endregion

        #region DragonQuest Quadruped Location Tables

        [TestFixture]
        public class QuadrupedLocationTableTests
        {
            private QuadrupedHighLocationsTable _highTable;
            private QuadrupedMidLocationsTable _midTable;
            private QuadrupedLowLocationsTable _lowTable;

            [SetUp]
            public void SetUp()
            {
                _highTable = new QuadrupedHighLocationsTable();
                _midTable = new QuadrupedMidLocationsTable();
                _lowTable = new QuadrupedLowLocationsTable();
            }

            [Test]
            public void QuadrupedHighLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _highTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Quadruped High table");
                }
            }

            [Test]
            public void QuadrupedMidLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _midTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Quadruped Mid table");
                }
            }

            [Test]
            public void QuadrupedLowLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _lowTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Quadruped Low table");
                }
            }

            [Test]
            public void QuadrupedTables_GetRoll_ReturnsD100()
            {
                Assert.That(_highTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_midTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_lowTable.GetRoll(), Is.EqualTo("d100"));
            }
        }

        #endregion

        #region DragonQuest Avian Location Tables

        [TestFixture]
        public class AvianLocationTableTests
        {
            private AvianHighLocationsTable _highTable;
            private AvianMidLocationsTable _midTable;
            private AvianLowLocationsTable _lowTable;

            [SetUp]
            public void SetUp()
            {
                _highTable = new AvianHighLocationsTable();
                _midTable = new AvianMidLocationsTable();
                _lowTable = new AvianLowLocationsTable();
            }

            [Test]
            public void AvianHighLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _highTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Avian High table");
                }
            }

            [Test]
            public void AvianMidLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _midTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Avian Mid table");
                }
            }

            [Test]
            public void AvianLowLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _lowTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Avian Low table");
                }
            }

            [Test]
            public void AvianTables_GetRoll_ReturnsD100()
            {
                Assert.That(_highTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_midTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_lowTable.GetRoll(), Is.EqualTo("d100"));
            }
        }

        #endregion

        #region DragonQuest Serpentine Location Tables

        [TestFixture]
        public class SerpentineLocationTableTests
        {
            private SerpentineHighLocationsTable _highTable;
            private SerpentineMidLocationsTable _midTable;
            private SerpentineLowLocationsTable _lowTable;

            [SetUp]
            public void SetUp()
            {
                _highTable = new SerpentineHighLocationsTable();
                _midTable = new SerpentineMidLocationsTable();
                _lowTable = new SerpentineLowLocationsTable();
            }

            [Test]
            public void SerpentineHighLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _highTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Serpentine High table");
                }
            }

            [Test]
            public void SerpentineMidLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _midTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Serpentine Mid table");
                }
            }

            [Test]
            public void SerpentineLowLocation_ValidRange_DoesNotThrow()
            {
                for (int i = 1; i <= 100; i++)
                {
                    Assert.That(() => _lowTable.LookupResult(i), Throws.Nothing,
                        $"Roll {i} should be valid for Serpentine Low table");
                }
            }

            [Test]
            public void SerpentineTables_GetRoll_ReturnsD100()
            {
                Assert.That(_highTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_midTable.GetRoll(), Is.EqualTo("d100"));
                Assert.That(_lowTable.GetRoll(), Is.EqualTo("d100"));
            }
        }

        #endregion

        #region Heroes Location Table

        [TestFixture]
        public class HeroesLocationTableTests
        {
            private LocationTable _table;

            [SetUp]
            public void SetUp()
            {
                _table = new LocationTable();
            }

            [TestCase(3, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8")]
            [TestCase(4, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8")]
            [TestCase(5, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8")]
            [TestCase(6, "Hands - Stun:x1 NStun:x½ Body:x½ ToHit:-6")]
            [TestCase(7, "Arms - Stun:x2 NStun:x½ Body:x½ ToHit:-5")]
            [TestCase(8, "Arms - Stun:x2 NStun:x½ Body:x½ ToHit:-5")]
            [TestCase(9, "Shoulders - Stun:x3 NStun:x1 Body:x1 ToHit:-5")]
            [TestCase(10, "Chest - Stun:x3 NStun:x1 Body:x1 ToHit:-3")]
            [TestCase(11, "Chest - Stun:x3 NStun:x1 Body:x1 ToHit:-3")]
            [TestCase(12, "Stomach - Stun:x4 NStun:x1½ Body:x1 ToHit:-7")]
            [TestCase(13, "Vitals - Stun:x4 NStun:x1½ Body:x2 ToHit:-8")]
            [TestCase(14, "Thighs - Stun:x2 NStun:x1 Body:x1 ToHit:-4")]
            [TestCase(15, "Legs - Stun:x2 NStun:x½ Body:x½ ToHit:-6")]
            [TestCase(16, "Legs - Stun:x2 NStun:x½ Body:x½ ToHit:-6")]
            [TestCase(17, "Feet - Stun:x1 NStun:x½ Body:x½ ToHit:-8")]
            [TestCase(18, "Feet - Stun:x1 NStun:x½ Body:x½ ToHit:-8")]
            public void HeroesLocation_ValidRolls_ReturnsCorrectLocation(int roll, string expectedLocation)
            {
                // Act
                var result = _table.LookupResult(roll);

                // Assert
                Assert.That(result, Is.EqualTo(expectedLocation));
            }

            [Test]
            public void HeroesLocation_GetRoll_Returns3D6()
            {
                // Act
                var roll = _table.GetRoll();

                // Assert
                Assert.That(roll, Is.EqualTo("3d6"));
            }

            [Test]
            public void HeroesLocation_GetMaxRoll_Returns18()
            {
                // Act
                var maxRoll = _table.GetMaxRoll();

                // Assert
                Assert.That(maxRoll, Is.EqualTo(18));
            }

            [TestCase(0)]
            [TestCase(19)]
            public void HeroesLocation_OutOfBounds_ThrowsException(int roll)
            {
                // Assert
                Assert.That(() => _table.LookupResult(roll), Throws.ArgumentException);
            }

            [Test]
            public void HeroesLocation_EdgeCases_HandledCorrectly()
            {
                // Test minimum and maximum valid values
                Assert.That(() => _table.LookupResult(3), Throws.Nothing);
                Assert.That(() => _table.LookupResult(18), Throws.Nothing);
            }

            [Test]
            public void HeroesLocation_HeadHits_HaveCorrectModifiers()
            {
                // Test that head hits (3-5) all have the same modifiers
                var result3 = _table.LookupResult(3);
                var result4 = _table.LookupResult(4);
                var result5 = _table.LookupResult(5);

                Assert.That(result3, Is.EqualTo(result4));
                Assert.That(result4, Is.EqualTo(result5));
                Assert.That(result3, Does.Contain("Stun:x5"));
                Assert.That(result3, Does.Contain("ToHit:-8"));
            }

            [Test]
            public void HeroesLocation_ChestHits_HaveCorrectModifiers()
            {
                // Test that chest hits (10-11) have the same modifiers
                var result10 = _table.LookupResult(10);
                var result11 = _table.LookupResult(11);

                Assert.That(result10, Is.EqualTo(result11));
                Assert.That(result10, Does.Contain("Stun:x3"));
                Assert.That(result10, Does.Contain("ToHit:-3"));
            }
        }

        #endregion

        #region General Location Table Tests

        [TestFixture]
        public class GeneralLocationTableTests
        {
            [Test]
            public void AllDragonQuestLocationTables_UseD100_AsExpected()
            {
                var tables = new LookupTable[]
                {
                    new HumanoidHighLocationsTable(),
                    new HumanoidArmsLocationsTable(),
                    new HumanoidMidLocationsTable(),
                    new HumanoidLegsLocationsTable(),
                    new QuadrupedHighLocationsTable(),
                    new QuadrupedMidLocationsTable(),
                    new QuadrupedLowLocationsTable(),
                    new AvianHighLocationsTable(),
                    new AvianMidLocationsTable(),
                    new AvianLowLocationsTable(),
                    new SerpentineHighLocationsTable(),
                    new SerpentineMidLocationsTable(),
                    new SerpentineLowLocationsTable()
                };

                foreach (var table in tables)
                {
                    Assert.That(table.GetRoll(), Is.EqualTo("d100"), 
                        $"{table.GetType().Name} should use d100");
                    Assert.That(table.GetMaxRoll(), Is.EqualTo(100),
                        $"{table.GetType().Name} should have max roll of 100");
                }
            }

            [Test]
            public void AllLocationTables_HandleBoundaryValues_Correctly()
            {
                var allTables = new LookupTable[]
                {
                    new HumanoidHighLocationsTable(),
                    new HumanoidArmsLocationsTable(),
                    new HumanoidMidLocationsTable(),
                    new HumanoidLegsLocationsTable(),
                    new QuadrupedHighLocationsTable(),
                    new QuadrupedMidLocationsTable(),
                    new QuadrupedLowLocationsTable(),
                    new AvianHighLocationsTable(),
                    new AvianMidLocationsTable(),
                    new AvianLowLocationsTable(),
                    new SerpentineHighLocationsTable(),
                    new SerpentineMidLocationsTable(),
                    new SerpentineLowLocationsTable(),
                    new LocationTable() // Heroes table
                };

                foreach (var table in allTables)
                {
                    var maxRoll = table.GetMaxRoll();
                    var minRoll = table is LocationTable ? 3 : 1; // Heroes table starts at 3

                    // Test valid boundaries
                    Assert.That(() => table.LookupResult(minRoll), Throws.Nothing,
                        $"{table.GetType().Name} should handle minimum roll {minRoll}");
                    Assert.That(() => table.LookupResult(maxRoll), Throws.Nothing,
                        $"{table.GetType().Name} should handle maximum roll {maxRoll}");

                    // Test invalid boundaries
                    // Heroes table has special behavior - it doesn't throw for values 1-2, but returns the value for 3
                    if (table is LocationTable)
                    {
                        // Heroes table only throws for 0 and negative values due to base class check
                        Assert.That(() => table.LookupResult(0), Throws.ArgumentException,
                            $"{table.GetType().Name} should throw for roll 0");
                    }
                    else
                    {
                        // Standard DragonQuest tables throw for values below 1
                        Assert.That(() => table.LookupResult(minRoll - 1), Throws.ArgumentException,
                            $"{table.GetType().Name} should throw for roll {minRoll - 1}");
                    }
                    
                    Assert.That(() => table.LookupResult(maxRoll + 1), Throws.ArgumentException,
                        $"{table.GetType().Name} should throw for roll {maxRoll + 1}");
                }
            }
        }

        #endregion
    }
}