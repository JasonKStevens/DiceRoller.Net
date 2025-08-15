using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Core.Parser;
using DiceRoller.Test.Helpers;
using NUnit.Framework;

namespace DiceRoller.Test
{
    [TestFixture]
    public class ProgressiveDisclosureFixture
    {
        private IResultNodeConverter _converter;
        private DiscordFormatter _formatter;
        private FormattingContext _context;
        
        [SetUp]
        public void SetUp()
        {
            _converter = new ResultNodeConverter();
            _context = new FormattingContext
            {
                Platform = "discord",
                IsInteractive = true,
                SupportsEmojis = true,
                SupportsMarkdown = true,
                MaxLineLength = 2000
            };
            _formatter = DiceHelper.CreateTestDiscordFormatter(_context, ResultFormat.Standard);
        }
        
        [Test]
        public void DiscordFormatter_SimpleTypedResult_ReturnsFormattedResponse()
        {
            // Given - A simple typed result converted to DiceResultNode
            var typedResult = TypedResult.NewSimpleResult("15");
            var diceNode = _converter.Convert(typedResult);
            
            // When - Formatting the result
            var response = _formatter.Visit(diceNode, "d20");
            
            // Then - Should get a formatted Discord response
            Assert.That(response, Is.Not.Null);
            Assert.That(response.PrimaryText, Is.Not.Empty);
            Assert.That(response.PrimaryText, Does.Contain("🎲"));
            Assert.That(response.PrimaryText, Does.Contain("**15**"));
            Assert.That(response.UsedFormat, Is.EqualTo(ResultFormat.Standard));
            Assert.That(response.InteractiveElements, Is.Not.Empty);
        }
        
        [Test]
        public void DiscordFormatter_TerseFormat_ReturnsMinimalOutput()
        {
            // Given - A simple typed result and terse formatter
            var typedResult = TypedResult.NewSimpleResult("8");
            var diceNode = _converter.Convert(typedResult);
            var terseFormatter = DiceHelper.CreateTestDiscordFormatter(_context, ResultFormat.Terse);
            
            // When - Formatting with terse format
            var response = terseFormatter.Visit(diceNode, "d10");
            
            // Then - Should get minimal output
            Assert.That(response, Is.Not.Null);
            Assert.That(response.PrimaryText, Is.Not.Empty);
            Assert.That(response.PrimaryText.Length, Is.LessThan(50));
            Assert.That(response.UsedFormat, Is.EqualTo(ResultFormat.Terse));
        }
        
        [Test]
        public void DiscordFormatter_DetailedFormat_ReturnsFullBreakdown()
        {
            // Given - A complex typed result and detailed formatter
            var typedResult = TypedResult.NewSimpleResult("18");
            var diceNode = _converter.Convert(typedResult);
            var detailedFormatter = DiceHelper.CreateTestDiscordFormatter(_context, ResultFormat.Detailed);
            
            // When - Formatting with detailed format
            var response = detailedFormatter.Visit(diceNode, "3d6+2");
            
            // Then - Should get detailed breakdown
            Assert.That(response, Is.Not.Null);
            Assert.That(response.PrimaryText, Is.Not.Empty);
            Assert.That(response.PrimaryText, Does.Contain("**Dice Roll Result**"));
            Assert.That(response.PrimaryText, Does.Contain("**Expression:**"));
            Assert.That(response.PrimaryText, Does.Contain("**Result:**"));
            Assert.That(response.UsedFormat, Is.EqualTo(ResultFormat.Detailed));
            Assert.That(response.IsExpandable, Is.True);
        }
        
        [Test]
        public void DiscordFormatter_AlwaysIncludesInteractiveElements()
        {
            // Given - Any valid typed result
            var typedResult = TypedResult.NewSimpleResult("12");
            var diceNode = _converter.Convert(typedResult);
            
            // When - Formatting the result
            var response = _formatter.Visit(diceNode, "d20");
            
            // Then - Should include interactive elements
            Assert.That(response.InteractiveElements, Is.Not.Empty);
            var actions = response.InteractiveElements.Select(e => e.Action).ToList();
            Assert.That(actions, Does.Contain("reroll"));
            Assert.That(actions, Does.Contain("copy"));
        }
        
        [Test]
        public void ResultNodeConverter_SimpleResult_ConvertsCorrectly()
        {
            // Given - A simple typed result
            var typedResult = TypedResult.NewSimpleResult("15");
            
            // When - Converting to DiceResultNode
            var diceNode = _converter.Convert(typedResult);
            
            // Then - Should convert properly
            Assert.That(diceNode, Is.Not.Null);
            Assert.That(diceNode.Value, Is.EqualTo(15));
            Assert.That(diceNode.Breakdown, Is.Not.Empty);
            Assert.That(diceNode.TypedResult, Is.EqualTo(typedResult));
        }
        
        [Test]
        public void ResultNodeConverter_NullResult_HandlesGracefully()
        {
            // Given - A null typed result
            TypedResult typedResult = null;
            
            // When - Converting to DiceResultNode
            var diceNode = _converter.Convert(typedResult);
            
            // Then - Should handle gracefully
            Assert.That(diceNode, Is.Not.Null);
            Assert.That(diceNode.Value, Is.EqualTo(0));
            Assert.That(diceNode.Breakdown, Does.Contain("No result"));
        }
    }
}