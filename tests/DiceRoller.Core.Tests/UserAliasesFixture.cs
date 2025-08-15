using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;
using System.Linq;

namespace DiceRoller.Test
{
    [TestFixture]
    public class UserAliasesFixture
    {
        private UserAliases _userAliases;
        private DiceRollEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _userAliases = new UserAliases();
            var sequenceGenerator = DiceHelper.GetSequenceGenerator(10, 5, 15);
            _evaluator = new DiceRollEvaluator(sequenceGenerator);
        }

        [Test]
        public void AddUpdate_NewAlias_StoresCorrectly()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "attack";
            var expression = "d20+5";
            var parseTree = _evaluator.Parse(expression);

            // Act
            _userAliases.AddUpdate(userId, aliasName, parseTree, expression);

            // Assert
            var retrievedTree = _userAliases.Get(userId, aliasName);
            Assert.That(retrievedTree, Is.Not.Null);
            Assert.That(retrievedTree.Root, Is.Not.Null);
        }

        [Test]
        public void AddUpdate_UpdateExistingAlias_OverwritesCorrectly()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "damage";
            var originalExpression = "d6";
            var updatedExpression = "2d6+3";
            var originalTree = _evaluator.Parse(originalExpression);
            var updatedTree = _evaluator.Parse(updatedExpression);

            // Act
            _userAliases.AddUpdate(userId, aliasName, originalTree, originalExpression);
            _userAliases.AddUpdate(userId, aliasName, updatedTree, updatedExpression);

            // Assert
            var retrievedTree = _userAliases.Get(userId, aliasName);
            Assert.That(retrievedTree, Is.Not.Null);
            
            var aliasList = _userAliases.GetAliasList(userId);
            Assert.That(aliasList.Count, Is.EqualTo(1));
            Assert.That(aliasList.First(), Does.Contain(updatedExpression));
        }

        [Test]
        public void Get_NonExistentAlias_ReturnsNull()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "nonexistent";

            // Act
            var result = _userAliases.Get(userId, aliasName);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Get_DifferentUser_ReturnsNull()
        {
            // Arrange
            var userId1 = "user1";
            var userId2 = "user2";
            var aliasName = "skill";
            var expression = "d100";
            var parseTree = _evaluator.Parse(expression);

            _userAliases.AddUpdate(userId1, aliasName, parseTree, expression);

            // Act
            var result = _userAliases.Get(userId2, aliasName);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Remove_ExistingAlias_RemovesSuccessfully()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "initiative";
            var expression = "d20+2";
            var parseTree = _evaluator.Parse(expression);

            _userAliases.AddUpdate(userId, aliasName, parseTree, expression);

            // Act
            _userAliases.Remove(userId, aliasName);

            // Assert
            var result = _userAliases.Get(userId, aliasName);
            Assert.That(result, Is.Null);
            
            var aliasList = _userAliases.GetAliasList(userId);
            Assert.That(aliasList.Count, Is.EqualTo(0));
        }

        [Test]
        public void Remove_NonExistentAlias_DoesNotThrow()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "nonexistent";

            // Act & Assert
            Assert.That(() => _userAliases.Remove(userId, aliasName), Throws.Nothing);
        }

        [Test]
        public void GetAliasList_MultipleAliases_ReturnsAllAliases()
        {
            // Arrange
            var userId = "testuser";
            var aliases = new[]
            {
                ("attack", "d20+8"),
                ("damage", "2d6+4"),
                ("save", "d20+3")
            };

            foreach (var (name, expression) in aliases)
            {
                var parseTree = _evaluator.Parse(expression);
                _userAliases.AddUpdate(userId, name, parseTree, expression);
            }

            // Act
            var result = _userAliases.GetAliasList(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            
            foreach (var (name, expression) in aliases)
            {
                Assert.That(result.Any(alias => alias.Contains(name) && alias.Contains(expression)), Is.True,
                    $"Expected to find alias '{name}: {expression}' in result");
            }
        }

        [Test]
        public void GetAliasList_EmptyUser_ReturnsEmptyList()
        {
            // Arrange
            var userId = "newuser";

            // Act
            var result = _userAliases.GetAliasList(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void UserIds_CaseInsensitive_WorksCorrectly()
        {
            // Arrange
            var aliasName = "test";
            var expression = "d20";
            var parseTree = _evaluator.Parse(expression);

            // Act
            _userAliases.AddUpdate("TestUser", aliasName, parseTree, expression);
            var result = _userAliases.Get("testuser", aliasName);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Serialize_WithAliases_ReturnsValidJson()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "heal";
            var expression = "2d4+2";
            var parseTree = _evaluator.Parse(expression);

            _userAliases.AddUpdate(userId, aliasName, parseTree, expression);

            // Act
            var serialized = _userAliases.Serialize();

            // Assert
            Assert.That(serialized, Is.Not.Null);
            Assert.That(serialized, Does.Contain(userId));
            Assert.That(serialized, Does.Contain(aliasName));
            // The JSON serializer escapes + as \u002B, so check for the literal values or escaped version
            var containsUserId = serialized.Contains(userId);
            var containsAliasName = serialized.Contains(aliasName);
            var containsExpression = serialized.Contains(expression) || serialized.Contains("2d4\\u002B2");
            
            Assert.That(containsUserId, Is.True, $"Expected serialized JSON to contain user ID '{userId}'");
            Assert.That(containsAliasName, Is.True, $"Expected serialized JSON to contain alias name '{aliasName}'");
            Assert.That(containsExpression, Is.True, $"Expected serialized JSON to contain expression '{expression}' (either literal or escaped)");
        }

        [Test]
        public void Hydrate_WithValidJson_RestoresAliases()
        {
            // Arrange
            var userId = "testuser";
            var aliasName = "spell";
            var expression = "3d6";
            var parseTree = _evaluator.Parse(expression);

            _userAliases.AddUpdate(userId, aliasName, parseTree, expression);
            var serialized = _userAliases.Serialize();

            var newUserAliases = new UserAliases();

            // Act
            newUserAliases.Hydrate(serialized, _evaluator);

            // Assert
            var result = newUserAliases.Get(userId, aliasName);
            Assert.That(result, Is.Not.Null);
            
            var aliasList = newUserAliases.GetAliasList(userId);
            Assert.That(aliasList.Count, Is.EqualTo(1));
            Assert.That(aliasList.First(), Does.Contain(expression));
        }

        [Test]
        public void UserSeparation_MultipleUsers_KeepsAliasesSeparate()
        {
            // Arrange
            var user1 = "user1";
            var user2 = "user2";
            var aliasName = "roll";
            var expression1 = "d20";
            var expression2 = "d100";
            
            var tree1 = _evaluator.Parse(expression1);
            var tree2 = _evaluator.Parse(expression2);

            // Act
            _userAliases.AddUpdate(user1, aliasName, tree1, expression1);
            _userAliases.AddUpdate(user2, aliasName, tree2, expression2);

            // Assert
            var result1 = _userAliases.Get(user1, aliasName);
            var result2 = _userAliases.Get(user2, aliasName);
            
            Assert.That(result1, Is.Not.Null);
            Assert.That(result2, Is.Not.Null);
            Assert.That(result1, Is.Not.EqualTo(result2));
            
            var list1 = _userAliases.GetAliasList(user1);
            var list2 = _userAliases.GetAliasList(user2);
            
            Assert.That(list1.First(), Does.Contain(expression1));
            Assert.That(list2.First(), Does.Contain(expression2));
        }
    }
}