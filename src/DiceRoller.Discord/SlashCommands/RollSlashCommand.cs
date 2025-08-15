using DiceRoller;
using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordRollerBot.SlashCommands
{
    public class RollSlashCommand : ApplicationCommandModule
    {
        private readonly IEnumerable<ICommandProcessor> _commandProcessors;
        private readonly IUserSettings _userSettings;
        private readonly IResultNodeConverter _resultNodeConverter;
        private readonly AdaptiveFormatterManager _adaptiveFormatterManager;
        private readonly ILogger<RollSlashCommand> _logger;

        public RollSlashCommand(
            IEnumerable<ICommandProcessor> commandProcessors, 
            IUserSettings userSettings, 
            IResultNodeConverter resultNodeConverter,
            AdaptiveFormatterManager adaptiveFormatterManager,
            ILogger<RollSlashCommand> logger)
        {
            _commandProcessors = commandProcessors;
            _userSettings = userSettings;
            _resultNodeConverter = resultNodeConverter;
            _adaptiveFormatterManager = adaptiveFormatterManager;
            _logger = logger;
        }

        [SlashCommand("roll", "Roll dice with an expression")]
        public async Task RollCommand(InteractionContext ctx,
            [Autocomplete(typeof(DiceExpressionAutocompleteProvider))]
            [Option("dice", "e.g., d20, 3d6+2, 2d10")] string expression,
            [Option("ephemeral", "Make the result visible only to you")] bool ephemeral = false,
            [Option("format", "Choose output format: auto, terse, standard, detailed")] string format = "auto")
        {
            _logger.LogInformation($"=== RollCommand invoked by {ctx.User.Username} with expression: {expression} ===");
            
            try
            {
                var user = new DiscordUserInfo()
                {
                    Id = ctx.User.Id.ToString(),
                    DisplayName = ctx.Member?.DisplayName ?? ctx.User.Username,
                };

                var commandText = $"!roll {expression}";
                var typedResult = GetTypedResult(user, commandText);

                if (typedResult == null || typedResult == TypedResult.Null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Could not process the dice roll expression.")
                            .AsEphemeral(ephemeral));
                    return;
                }

                // Parse the format option
                var preferredFormat = ParseResultFormat(format);
                
                // Use Progressive Disclosure to format the result
                var adapter = new ProgressiveDisclosureDiscordAdapter(_resultNodeConverter, _adaptiveFormatterManager, user.Id, ephemeral);
                var discordResponse = adapter.ProcessResult(typedResult, expression, preferredFormat);

                // Apply lucky number formatting if not ephemeral
                if (!ephemeral)
                {
                    discordResponse.Content = ApplyLuckyNumberFormatting(discordResponse.Content, user.DisplayName);
                }

                // Send the response - no more file attachments needed with intelligent truncation
                var content = discordResponse.Content;
                var components = discordResponse.Components;
                
                // Log detailed information before sending to Discord
                _logger.LogInformation("About to send Discord response for expression: {Expression}", expression);
                _logger.LogInformation("Content length: {ContentLength}", content?.Length ?? 0);
                _logger.LogInformation("Number of components: {ComponentCount}", components?.Count ?? 0);
                
                if (components?.Any() == true)
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        var component = components[i];
                        _logger.LogInformation("Component {Index}: Type={Type}", i, component.GetType().Name);
                        
                        if (component is DiscordActionRowComponent actionRow)
                        {
                            _logger.LogInformation("  ActionRow with {ButtonCount} buttons", actionRow.Components?.Count ?? 0);
                            
                            if (actionRow.Components != null)
                            {
                                int buttonIndex = 0;
                                foreach (var button in actionRow.Components)
                                {
                                    if (button is DiscordButtonComponent btnComp)
                                    {
                                        _logger.LogInformation("    Button {ButtonIndex}: CustomId='{CustomId}' (length: {Length}), Label='{Label}', Style={Style}",
                                            buttonIndex, btnComp.CustomId, btnComp.CustomId?.Length ?? 0, btnComp.Label, btnComp.Style);
                                    }
                                    buttonIndex++;
                                }
                            }
                        }
                    }
                }
                
                var builder = new DiscordInteractionResponseBuilder()
                    .WithContent(content)
                    .AsEphemeral(ephemeral);
                
                // Add components if they exist
                if (components?.Any() == true)
                {
                    try
                    {
                        // Validate components before adding
                        var validComponents = ValidateComponents(components);
                        if (validComponents.Any())
                        {
                            builder.AddComponents(validComponents);
                            _logger.LogInformation("Added {ValidCount} valid components to builder", validComponents.Count);
                        }
                        else
                        {
                            _logger.LogWarning("No valid components to add after validation");
                        }
                    }
                    catch (Exception componentEx)
                    {
                        _logger.LogWarning(componentEx, "Failed to add components to response, sending without components");
                        // Continue without components rather than failing
                    }
                }
                
                try
                {
                    _logger.LogInformation("Attempting to send Discord response...");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    _logger.LogInformation("Successfully sent Discord response");
                }
                catch (Exception discordEx)
                {
                    _logger.LogError(discordEx, "Discord API error when sending response. Content length: {ContentLength}, Components: {ComponentCount}", 
                        content?.Length ?? 0, components?.Count ?? 0);
                    
                    // Try to send a simplified response without components
                    try
                    {
                        var fallbackBuilder = new DiscordInteractionResponseBuilder()
                            .WithContent(content?.Length > 2000 ? content.Substring(0, 1997) + "..." : content)
                            .AsEphemeral(ephemeral);
                        
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, fallbackBuilder);
                        _logger.LogInformation("Successfully sent fallback response without components");
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Failed to send even fallback response");
                        throw; // Re-throw the original exception
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing roll command: {Expression}", expression);
                
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Error processing roll: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("rollhelp", "Get help for dice rolling commands")]
        public async Task RollHelpCommand(InteractionContext ctx)
        {
            var helpText = @"**Dice Rolling Help**

**Basic Dice Expressions:**
• `d20` - Roll a 20-sided die
• `3d6` - Roll three 6-sided dice
• `2d10+5` - Roll two 10-sided dice and add 5
• `d100` - Roll percentile dice

**Special Commands:**
• `backfire` - Roll on backfire table
• `injury` or `specgrev` - Roll for grievous injury
• `fear` - Roll on fear table
• `speed` - Roll on speed table
• `hitloc [bodytype]` - Roll hit location (humanoid_mid, quadruped_mid, etc.)

**Aliases:**
• `addalias <name> <expression>` - Create a custom alias
• `removealias <name>` - Remove an alias
• `listalias` - List your aliases

**Examples:**
• `/roll expression:d20+5`
• `/roll expression:3d6`
• `/roll expression:backfire`";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent(helpText)
                    .AsEphemeral(true));
        }

        private TypedResult GetTypedResult(DiscordUserInfo user, string content)
        {
            bool handled = false;
            TypedResult rollResult = null;

            foreach (var processor in _commandProcessors)
            {
                try
                {
                    (handled, rollResult) = processor.ProcessTyped(user.Id, content);

                    if (handled)
                        break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in command processor {Prefix}", processor.Prefix);
                    return null;
                }
            }

            return rollResult;
        }
        
        private ResultFormat ParseResultFormat(string format)
        {
            return format?.ToLower() switch
            {
                "terse" => ResultFormat.Terse,
                "standard" => ResultFormat.Standard,
                "detailed" => ResultFormat.Detailed,
                "narrative" => ResultFormat.Narrative,
                "technical" => ResultFormat.Technical,
                _ => ResultFormat.Auto
            };
        }
        
        private List<DiscordComponent> ValidateComponents(List<DiscordComponent> components)
        {
            var validComponents = new List<DiscordComponent>();
            
            if (components == null || !components.Any())
                return validComponents;
                
            foreach (var component in components)
            {
                try
                {
                    if (component is DiscordActionRowComponent actionRow)
                    {
                        var validButtons = new List<DiscordButtonComponent>();
                        
                        if (actionRow.Components != null)
                        {
                            foreach (var button in actionRow.Components)
                            {
                                if (button is DiscordButtonComponent btnComp)
                                {
                                    // Validate button properties
                                    if (IsValidButton(btnComp))
                                    {
                                        validButtons.Add(btnComp);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Invalid button detected: CustomId='{CustomId}', Label='{Label}'", 
                                            btnComp.CustomId, btnComp.Label);
                                    }
                                }
                            }
                        }
                        
                        // Only add action row if it has valid buttons
                        if (validButtons.Any())
                        {
                            validComponents.Add(new DiscordActionRowComponent(validButtons));
                        }
                    }
                    else
                    {
                        validComponents.Add(component);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error validating component: {ComponentType}", component.GetType().Name);
                }
            }
            
            return validComponents;
        }
        
        private bool IsValidButton(DiscordButtonComponent button)
        {
            // Check custom ID length (Discord limit is 100 characters)
            if (string.IsNullOrEmpty(button.CustomId) || button.CustomId.Length > 100)
            {
                _logger.LogWarning("Button has invalid CustomId length: {Length}", button.CustomId?.Length ?? 0);
                return false;
            }
            
            // Check label length (Discord limit is 80 characters)
            if (string.IsNullOrEmpty(button.Label) || button.Label.Length > 80)
            {
                _logger.LogWarning("Button has invalid Label length: {Length}", button.Label?.Length ?? 0);
                return false;
            }
            
            // Check that custom ID doesn't contain invalid characters
            if (button.CustomId.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
            {
                _logger.LogWarning("Button CustomId contains invalid control characters");
                return false;
            }
            
            return true;
        }
        
        private string ApplyLuckyNumberFormatting(string content, string displayName)
        {
            // Check for lucky numbers in display name, if applicable
            if (content.Contains("```") || string.IsNullOrWhiteSpace(displayName))
                return content;

            var luckyNums = new List<int>();
            if (displayName.Contains('[') && displayName.Contains(']'))
            {
                var luckynums = displayName.Substring(displayName.IndexOf('[') + 1, displayName.IndexOf(']') - displayName.IndexOf('[') - 1);
                var nums = luckynums.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var num in nums)
                {
                    var numStr = new string(num.Where(c => char.IsDigit(c)).ToArray());
                    if (int.TryParse(numStr, out int value))
                    {
                        luckyNums.Add(value);
                    }
                }
            }

            foreach (var num in luckyNums)
            {
                content = content.Replace($"[{num},", $"[:sparkles:{num},");
                content = content.Replace($" {num},", $" :sparkles:{num},");
                content = content.Replace($" {num}]", $" :sparkles:{num}]");
                content = content.Replace($"[{num}]", $"[:sparkles:{num}]");
            }

            return content;
        }
    }

    public class DiceExpressionAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            var input = ctx.FocusedOption.Value?.ToString() ?? "";
            var inputLower = input.ToLower();

            // Always preserve the user's exact input as the first choice if it's not empty
            // This prevents Discord from replacing complex expressions
            if (!string.IsNullOrWhiteSpace(input))
            {
                choices.Add(new DiscordAutoCompleteChoice(input, input));
            }

            // Only suggest simple expressions for short, simple inputs
            // Don't interfere with complex expressions that contain operators, functions, or parentheses
            if (!string.IsNullOrWhiteSpace(input) && 
                input.Length <= 10 && 
                !ContainsComplexSyntax(input))
            {
                // Common dice expressions
                var commonExpressions = new[]
                {
                    "d4", "d6", "d8", "d10", "d12", "d20", "d100",
                    "2d6", "3d6", "4d6", "2d8", "2d10", "2d20",
                    "3d6+3", "d20+5", "2d8+2", "d10+1",
                    "backfire", "injury", "fear", "speed", "hitloc"
                };

                foreach (var expr in commonExpressions)
                {
                    // Only add suggestions that start with the input and aren't already the user's input
                    if (expr.StartsWith(inputLower, StringComparison.OrdinalIgnoreCase) && 
                        !expr.Equals(input, StringComparison.OrdinalIgnoreCase) && 
                        choices.Count < 25)
                    {
                        choices.Add(new DiscordAutoCompleteChoice(expr, expr));
                    }
                }
            }

            // For empty input, provide some basic suggestions
            if (string.IsNullOrWhiteSpace(input))
            {
                choices.AddRange(new[]
                {
                    new DiscordAutoCompleteChoice("d20", "d20"),
                    new DiscordAutoCompleteChoice("d100", "d100"),
                    new DiscordAutoCompleteChoice("3d6", "3d6"),
                    new DiscordAutoCompleteChoice("2d10", "2d10"),
                    new DiscordAutoCompleteChoice("backfire", "backfire"),
                    new DiscordAutoCompleteChoice("injury", "injury")
                });
            }

            return await Task.FromResult(choices);
        }

        /// <summary>
        /// Checks if the input contains complex syntax that shouldn't be interfered with
        /// </summary>
        private static bool ContainsComplexSyntax(string input)
        {
            // Check for operators, functions, parentheses, or other complex syntax
            return input.Contains('(') || input.Contains(')') ||
                   input.Contains("max") || input.Contains("min") ||
                   input.Contains("repeat") || input.Contains(',') ||
                   input.Contains('*') || input.Contains('/') ||
                   input.Contains('!') || // exploding dice
                   (input.Contains('+') && input.Length > 6) || // complex addition
                   (input.Contains('-') && input.Length > 6);   // complex subtraction
        }
    }
}