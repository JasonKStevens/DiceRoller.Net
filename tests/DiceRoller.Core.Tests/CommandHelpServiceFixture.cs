using DiceRoller.Core.Services.Help;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class CommandHelpServiceFixture
    {
        private ICommandHelpService _helpService;

        [SetUp]
        public void Setup()
        {
            _helpService = new CommandHelpService();
        }

        [Test]
        public void When_GetCommandHelp_WithValidCommand_Should_ReturnHelpContent()
        {
            // Arrange
            var commandName = "roll";

            // Act
            var result = _helpService.GetCommandHelp(commandName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("Dice Rolling Command Help"));
        }

        [Test]
        public void When_GetCommandHelp_WithInvalidCommand_Should_ReturnNull()
        {
            // Arrange
            var commandName = "nonexistent";

            // Act
            var result = _helpService.GetCommandHelp(commandName);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void When_RegisterPluginHelp_WithValidData_Should_StoreHelpContent()
        {
            // Arrange
            var pluginId = "TestPlugin";
            var commandName = "testcommand";
            var helpContent = "Test command help content";

            // Act
            _helpService.RegisterPluginHelp(commandName, helpContent, pluginId);
            var result = _helpService.GetCommandHelp(commandName);

            // Assert
            Assert.That(result, Is.EqualTo(helpContent));
        }

        [Test]
        public void When_UnregisterPluginHelp_WithValidPluginId_Should_RemoveHelpContent()
        {
            // Arrange
            var pluginId = "TestPlugin";
            var commandName = "testcommand";
            var helpContent = "Test command help content";
            _helpService.RegisterPluginHelp(commandName, helpContent, pluginId);

            // Act
            _helpService.UnregisterPluginHelp(pluginId);
            var result = _helpService.GetCommandHelp(commandName);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void When_HasHelpFor_WithExistingCommand_Should_ReturnTrue()
        {
            // Arrange
            var commandName = "roll";

            // Act
            var result = _helpService.HasHelpFor(commandName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void When_HasHelpFor_WithNonExistingCommand_Should_ReturnFalse()
        {
            // Arrange
            var commandName = "nonexistent";

            // Act
            var result = _helpService.HasHelpFor(commandName);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void When_TruncateForDiscord_WithLongContent_Should_TruncateAppropriately()
        {
            // Arrange
            var longContent = new string('a', 2500);

            // Act
            var result = CommandHelpService.TruncateForDiscord(longContent);

            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(2000));
            Assert.That(result, Does.Contain("content truncated"));
        }

        [Test]
        public void When_TruncateForDiscord_WithShortContent_Should_ReturnUnchanged()
        {
            // Arrange
            var shortContent = "Short content";

            // Act
            var result = CommandHelpService.TruncateForDiscord(shortContent);

            // Assert
            Assert.That(result, Is.EqualTo(shortContent));
        }
    }
}