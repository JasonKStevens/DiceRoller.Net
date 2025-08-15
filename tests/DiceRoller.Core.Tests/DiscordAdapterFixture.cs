using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Core.Parser;
using DiceRoller.Discord;
using DiceRoller.Test.Helpers;
using DSharpPlus.Entities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DiceRoller.Test
{
    [TestFixture]
    public class DiscordAdapterFixture
    {
        private ProgressiveDisclosureDiscordAdapter _adapter;
        private IResultNodeConverter _converter;
        
        [SetUp]
        public void SetUp()
        {
            _converter = new ResultNodeConverter();
            var adaptiveFormatterManager = DiceHelper.CreateTestAdaptiveFormatterManager();
            _adapter = new ProgressiveDisclosureDiscordAdapter(_converter, adaptiveFormatterManager, "test_user_123", false);
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_LargeExpression_UsesTruncation()
        {
            // Given - A large expression that generates lots of output (like 20d100!)
            var typedResult = CreateLargeTypedResult();
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "20d100!");
            
            // Then - Should use intelligent truncation instead of files
            Assert.That(response.RequiresFile, Is.False);
            Assert.That(response.FileName, Is.Null);
            Assert.That(response.FileContent, Is.Null);
            Assert.That(response.Content.Length, Is.LessThan(2000)); // Should be truncated to fit Discord limits
            Assert.That(response.Content, Does.Contain("20000")); // Should contain the result value
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_LargeExpression_IncludesComponents()
        {
            // Given - A large expression that would previously require file attachment
            var typedResult = CreateLargeTypedResult();
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "20d100!");
            
            // Then - Should now include components since we use truncation instead of files
            Assert.That(response.RequiresFile, Is.False);
            Assert.That(response.Components, Is.Not.Empty, 
                "Components should be included when using intelligent truncation");
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_SmallExpression_IncludesComponents()
        {
            // Given - A small expression that doesn't require file
            var typedResult = TypedResult.NewSimpleResult("15");
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "d20");
            
            // Then - Should include components for interactive behavior
            Assert.That(response.RequiresFile, Is.False);
            Assert.That(response.Components, Is.Not.Empty);
            Assert.That(response.Components.Count, Is.GreaterThan(0));
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_NullResult_HandlesGracefully()
        {
            // Given - A null result
            TypedResult typedResult = null;
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "invalid");
            
            // Then - Should handle gracefully without components
            Assert.That(response.Content, Does.Contain("Could not process"));
            Assert.That(response.RequiresFile, Is.False);
            Assert.That(response.Components, Is.Empty);
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_ComponentStructure_IsValidActionRow()
        {
            // Given - A valid result that should produce components
            var typedResult = TypedResult.NewSimpleResult("15");
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "d20");
            
            // Then - Components should be properly structured as ActionRows
            Assert.That(response.Components, Is.Not.Empty);
            Assert.That(response.Components.Count, Is.GreaterThan(0));
            
            // All components should be DiscordActionRowComponent (proper Discord structure)
            foreach (var component in response.Components)
            {
                Assert.That(component, Is.InstanceOf<DiscordActionRowComponent>(), 
                    "All components should be ActionRow components for proper Discord API structure");
                
                var actionRow = (DiscordActionRowComponent)component;
                Assert.That(actionRow.Components, Is.Not.Null);
                Assert.That(actionRow.Components.Count, Is.GreaterThan(0));
                Assert.That(actionRow.Components.Count, Is.LessThanOrEqualTo(5), 
                    "Discord ActionRow cannot have more than 5 components");
                
                // Verify buttons have valid properties
                foreach (var button in actionRow.Components)
                {
                    if (button is DiscordButtonComponent btnComp)
                    {
                        Assert.That(btnComp.CustomId, Is.Not.Null.And.Not.Empty);
                        Assert.That(btnComp.CustomId.Length, Is.LessThanOrEqualTo(100), 
                            "Discord CustomId cannot exceed 100 characters");
                        Assert.That(btnComp.Label, Is.Not.Null.And.Not.Empty);
                        Assert.That(btnComp.Label.Length, Is.LessThanOrEqualTo(80), 
                            "Discord Label cannot exceed 80 characters");
                    }
                }
            }
        }
        
        [Test]
        public void ProgressiveDisclosureDiscordAdapter_ExplodingDice_ComponentsWorkCorrectly()
        {
            // Given - An exploding dice result (like 20d100!) that should produce components
            var typedResult = CreateExplodingDiceResult();
            
            // When - Processing the result
            var response = _adapter.ProcessResult(typedResult, "20d100!");
            
            // Then - Should have proper component structure even for complex results
            Assert.That(response.RequiresFile, Is.False, "Should use intelligent truncation, not files");
            Assert.That(response.Components, Is.Not.Empty, "Should include components for interactivity");
            
            // Verify the first component is an ActionRow with proper structure
            var firstComponent = response.Components.First();
            Assert.That(firstComponent, Is.InstanceOf<DiscordActionRowComponent>());
            
            var actionRow = (DiscordActionRowComponent)firstComponent;
            Assert.That(actionRow.Components, Is.Not.Null);
            Assert.That(actionRow.Components.Count, Is.GreaterThan(0));
            
            // Should have re-roll button at minimum
            var hasRerollButton = actionRow.Components.Any(c => 
                c is DiscordButtonComponent btn && btn.Label.Contains("Re-roll"));
            Assert.That(hasRerollButton, Is.True, "Should include re-roll button for dice expressions");
        }
        
        private TypedResult CreateLargeTypedResult()
        {
            // Create a TypedResult that would generate a very long breakdown
            // Make sure it's long enough to trigger file requirement (> 1900 chars after formatting)
            var longText = string.Join(", ", Enumerable.Range(1, 200).Select(i => $"[{i}]"));
            var resultText = $"20000";
            
            var result = TypedResult.NewSimpleResult(resultText);
            // Add multiple sub-results to make it complex like an exploding dice result
            // Make each sub-result long enough to guarantee > 1900 chars after Discord formatting
            result.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, longText));
            result.SubText.Add(TypedResult.NewSimpleResult(NodeType.Comment, "Exploding dice result with many rolls: " + longText));
            result.SubText.Add(TypedResult.NewSimpleResult(NodeType.Comment, "Very detailed breakdown: " + longText));
            
            return result;
        }
        
        private TypedResult CreateExplodingDiceResult()
        {
            // Create a realistic exploding dice result
            var result = TypedResult.NewSimpleResult("2000");
            
            // Add exploding dice breakdown
            var explodingBreakdown = "[100!, 100!, 100!, 95, 88, 76, 100!, 100!, 91, 83, 72, 100!, 94, 87, 81, 100!, 99, 92, 85, 100!]";
            result.SubText.Add(TypedResult.NewSimpleResult(NodeType.DiceRoll, explodingBreakdown));
            result.SubText.Add(TypedResult.NewSimpleResult(NodeType.Comment, "Exploding dice: 20d100!"));
            
            return result;
        }
    }
}