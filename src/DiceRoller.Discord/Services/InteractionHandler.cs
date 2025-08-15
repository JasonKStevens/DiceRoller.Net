using DiceRoller;
using DiceRoller.Core.Parser;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
using DiceRoller.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordRollerBot.Services
{
    public class InteractionHandler
    {
        private readonly IEnumerable<ICommandProcessor> _commandProcessors;
        private readonly IUserSettings _userSettings;
        private readonly IResultNodeConverter _resultNodeConverter;
        private readonly AdaptiveFormatterManager _adaptiveFormatterManager;
        private readonly ILogger<InteractionHandler> _logger;

        private const string ButtonPrefix = "button_";
        private const string ProgressiveDisclosurePrefix = "pd_";

        // Button definitions (copied from DiscordApi for consistency)
        private static readonly DiscordButtonComponent[] _diceButtons = new DiscordButtonComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Secondary, $"{ButtonPrefix}d100", "d100"),
            new DiscordButtonComponent(ButtonStyle.Secondary, $"{ButtonPrefix}d20", "d20"),
            new DiscordButtonComponent(ButtonStyle.Secondary, $"{ButtonPrefix}d10", "d10"),
            new DiscordButtonComponent(ButtonStyle.Secondary, $"{ButtonPrefix}d8", "d8"),
            new DiscordButtonComponent(ButtonStyle.Secondary, $"{ButtonPrefix}d6", "d6"),
        };

        private static readonly DiscordButtonComponent[] _specialsButtons = new DiscordButtonComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}specgrev", "Spec Griev"),
            new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}backfire", "Backfire"),
            new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}fear", "Fear"),
        };

        private static readonly List<DiscordSelectComponentOption> hitLocationOptions =
            new List<DiscordSelectComponentOption>()
            {
                new DiscordSelectComponentOption("Hit location", "hitloc_", isDefault: true),
                new DiscordSelectComponentOption("Human mid", "hitloc_humanoid_mid"),
                new DiscordSelectComponentOption("Quadruped mid", "hitloc_quadruped_mid"),
                new DiscordSelectComponentOption("Avian mid", "hitloc_avian_mid"),
                new DiscordSelectComponentOption("Serpent mid", "hitloc_serpent_mid"),

                new DiscordSelectComponentOption("Human high", "hitloc_humanoid_high"),
                new DiscordSelectComponentOption("Human arms", "hitloc_humanoid_arms"),
                new DiscordSelectComponentOption("Human legs", "hitloc_humanoid_legs"),

                new DiscordSelectComponentOption("Quadruped high", "hitloc_quadruped_high"),
                new DiscordSelectComponentOption("Quadruped low", "hitloc_quadruped_low"),

                new DiscordSelectComponentOption("Avian high", "hitloc_avian_high"),
                new DiscordSelectComponentOption("Avian low", "hitloc_avian_low"),

                new DiscordSelectComponentOption("Serpent high", "hitloc_serpent_high"),
                new DiscordSelectComponentOption("Serpent low", "hitloc_serpent_low"),
            };

        private static readonly DiscordComponent[] _hitLocDropdown = new DiscordComponent[]
        {
            new DiscordSelectComponent($"{ButtonPrefix}hitloc", null, hitLocationOptions, false, 1, 1),
        };

        public InteractionHandler(
            IEnumerable<ICommandProcessor> commandProcessors, 
            IUserSettings userSettings, 
            IResultNodeConverter resultNodeConverter,
            AdaptiveFormatterManager adaptiveFormatterManager,
            ILogger<InteractionHandler> logger)
        {
            _commandProcessors = commandProcessors;
            _userSettings = userSettings;
            _resultNodeConverter = resultNodeConverter;
            _adaptiveFormatterManager = adaptiveFormatterManager;
            _logger = logger;
        }

        public async Task HandleComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            try
            {
                // Handle Progressive Disclosure buttons
                if (e.Id.StartsWith("reroll_") || e.Id.StartsWith("details_") || e.Id.StartsWith("copy_"))
                {
                    await HandleProgressiveDisclosureInteraction(e);
                    return;
                }
                
                if (!e.Id.StartsWith(ButtonPrefix))
                    return;

                var id = e.Id.Substring(ButtonPrefix.Length);
                var buttonCommand = $"{id}";

                if (id == "hitloc")
                {
                    var loc = e.Values[0].Substring("hitloc_".Length);
                    buttonCommand += $" {loc}";
                }

                var user = new DiscordUserInfo()
                {
                    Id = e.User.Id.ToString(),
                    DisplayName = e.User.Username,
                    NickName = (e.User as DiscordMember)?.Nickname
                };

                if (e.User is DiscordMember member && !string.IsNullOrWhiteSpace(member.DisplayName))
                {
                    user.DisplayName = member.DisplayName;
                }

                var response = GetResponse(user, $"!roll {buttonCommand}");

                var builder = new DiscordMessageBuilder();

                if (response?.Length > 2000)
                {
                    var fileName = $"button_result_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                    var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
                    builder.AddFile(fileName, stream);
                    builder.WithContent($"{user.DisplayName}: !roll {buttonCommand}");
                    // Don't add buttons when sending files - Discord API doesn't allow components with files
                }
                else
                {
                    builder.WithContent($"{user.DisplayName}: !roll {buttonCommand} -> {response}");
                    // Only add buttons when not sending files
                    AddButtons(e.User.Id.ToString(), builder);
                }

                await builder.SendAsync(e.Channel);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling component interaction: {InteractionId}", e.Id);

                try
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("An error occurred while processing your interaction.")
                            .AsEphemeral(true));
                }
                catch (Exception responseEx)
                {
                    _logger.LogError(responseEx, "Failed to send error response");
                }
            }
        }

        public void AddButtons(string userId, DiscordMessageBuilder builder)
        {
            var showDiceButtons = true;
            var showHitLocations = true;
            var showSpecialButtons = true;

            if (!string.IsNullOrWhiteSpace(_userSettings.GetUserSetting(userId, "showDiceButtons")))
            {
                bool.TryParse(_userSettings.GetUserSetting(userId, "showDiceButtons"), out showDiceButtons);
            }

            if (!string.IsNullOrWhiteSpace(_userSettings.GetUserSetting(userId, "showHitLocations")))
            {
                bool.TryParse(_userSettings.GetUserSetting(userId, "showHitLocations"), out showHitLocations);
            }

            if (!string.IsNullOrWhiteSpace(_userSettings.GetUserSetting(userId, "showSpecialButtons")))
            {
                bool.TryParse(_userSettings.GetUserSetting(userId, "showSpecialButtons"), out showSpecialButtons);
            }

            if (showDiceButtons) builder.AddComponents(_diceButtons);
            if (showHitLocations) builder.AddComponents(_hitLocDropdown);
            if (showSpecialButtons) builder.AddComponents(_specialsButtons);
        }

        private string GetResponse(DiscordUserInfo user, string content)
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
                    return $"{content}: {ex.Message}";
                }
            }

            if (handled && rollResult == TypedResult.Null)
                return "Unrecognised command";

            if (rollResult == null)
                return null;

            // Visit the rollResult and build the output text
            var outputBuilder = new DiscordTypedResultOutputVisitor(10);
            var response = outputBuilder.Visit(rollResult, 1);

            // Check for lucky numbers, if applicable
            if (!response.Contains("```") && !string.IsNullOrWhiteSpace(user.NickName))
            {
                var luckyNums = new List<int>();
                var nickname = user.NickName;
                if (nickname.Contains('[') && nickname.Contains(']'))
                {
                    var luckynums = nickname.Substring(nickname.IndexOf('[') + 1, nickname.IndexOf(']') - nickname.IndexOf('[') - 1);
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
                    response = response.Replace($"[{num},", $"[:sparkles:{num},");
                    response = response.Replace($" {num},", $" :sparkles:{num},");
                    response = response.Replace($" {num}]", $" :sparkles:{num}]");
                    response = response.Replace($"[{num}]", $"[:sparkles:{num}]");
                }
            }

            return response;
        }
        
        private async Task HandleProgressiveDisclosureInteraction(ComponentInteractionCreateEventArgs e)
        {
            try
            {
                var user = new DiscordUserInfo()
                {
                    Id = e.User.Id.ToString(),
                    DisplayName = e.User.Username,
                    NickName = (e.User as DiscordMember)?.Nickname
                };

                if (e.User is DiscordMember member && !string.IsNullOrWhiteSpace(member.DisplayName))
                {
                    user.DisplayName = member.DisplayName;
                }

                if (e.Id.StartsWith("reroll_"))
                {
                    await HandleRerollInteraction(e, user);
                }
                else if (e.Id.StartsWith("details_"))
                {
                    await HandleDetailsInteraction(e, user);
                }
                else if (e.Id.StartsWith("copy_"))
                {
                    await HandleCopyInteraction(e, user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Progressive Disclosure interaction: {InteractionId}", e.Id);
                
                try
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("An error occurred while processing your interaction.")
                            .AsEphemeral(true));
                }
                catch (Exception responseEx)
                {
                    _logger.LogError(responseEx, "Failed to send error response for Progressive Disclosure");
                }
            }
        }
        
        private async Task HandleRerollInteraction(ComponentInteractionCreateEventArgs e, DiscordUserInfo user)
        {
            // Try to extract the original expression from the message
            var originalMessage = e.Message;
            var originalExpression = ExtractExpressionFromMessage(originalMessage.Content);
            
            if (string.IsNullOrEmpty(originalExpression))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not determine the original expression to re-roll.")
                        .AsEphemeral(true));
                return;
            }
            
            // Execute the roll again
            var response = GetResponse(user, $"!roll {originalExpression}");
            
            if (string.IsNullOrWhiteSpace(response))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not process the re-roll.")
                        .AsEphemeral(true));
                return;
            }
            
            // Send the new result
            if (response.Length > 2000)
            {
                var fileName = $"reroll_result_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
                
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"**Re-roll:** {user.DisplayName}: {originalExpression}")
                        .AddFile(fileName, stream));
            }
            else
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"**Re-roll:** {response}"));
            }
        }
        
        private async Task HandleDetailsInteraction(ComponentInteractionCreateEventArgs e, DiscordUserInfo user)
        {
            var originalMessage = e.Message;
            var originalExpression = ExtractExpressionFromMessage(originalMessage.Content);
            
            if (string.IsNullOrEmpty(originalExpression))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not determine the original expression for details.")
                        .AsEphemeral(true));
                return;
            }
            
            // Get the detailed result using Progressive Disclosure
            var rollResult = GetTypedResult(user, $"!roll {originalExpression}");
            if (rollResult != null)
            {
                var adapter = new ProgressiveDisclosureDiscordAdapter(_resultNodeConverter, _adaptiveFormatterManager, user.Id, true);
                var discordResponse = adapter.ProcessResult(rollResult, originalExpression, ResultFormat.Detailed);
                
                if (discordResponse.RequiresFile)
                {
                    using var stream = new MemoryStream(discordResponse.FileContent);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent(discordResponse.Content)
                            .AddFile(discordResponse.FileName, stream)
                            .AsEphemeral(true));
                }
                else
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent(discordResponse.Content)
                            .AsEphemeral(true));
                }
            }
            else
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not get detailed information for this result.")
                        .AsEphemeral(true));
            }
        }
        
        private async Task HandleCopyInteraction(ComponentInteractionCreateEventArgs e, DiscordUserInfo user)
        {
            var originalMessage = e.Message;
            var value = ExtractValueFromMessage(originalMessage.Content);
            
            if (string.IsNullOrEmpty(value))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not determine the value to copy.")
                        .AsEphemeral(true));
                return;
            }
            
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"Result copied: `{value}`")
                    .AsEphemeral(true));
        }
        
        private string ExtractExpressionFromMessage(string messageContent)
        {
            // Try to extract expression from various patterns
            // Pattern 1: "Expression: `expression`"
            var expressionMatch = System.Text.RegularExpressions.Regex.Match(messageContent, @"Expression:\s*`([^`]+)`");
            if (expressionMatch.Success)
            {
                return expressionMatch.Groups[1].Value;
            }
            
            // Pattern 2: "/roll expression:expression"
            var rollMatch = System.Text.RegularExpressions.Regex.Match(messageContent, @"/roll\s+expression:([^\s]+)");
            if (rollMatch.Success)
            {
                return rollMatch.Groups[1].Value;
            }
            
            // Pattern 3: "user: !roll expression"
            var userRollMatch = System.Text.RegularExpressions.Regex.Match(messageContent, @"!roll\s+([^\s]+)");
            if (userRollMatch.Success)
            {
                return userRollMatch.Groups[1].Value;
            }
            
            return null;
        }
        
        private string ExtractValueFromMessage(string messageContent)
        {
            // Try to extract numeric value from the message
            var valueMatch = System.Text.RegularExpressions.Regex.Match(messageContent, @"\*\*(\d+(?:\.\d+)?)\*\*");
            if (valueMatch.Success)
            {
                return valueMatch.Groups[1].Value;
            }
            
            // Fallback to any number
            var numberMatch = System.Text.RegularExpressions.Regex.Match(messageContent, @"(\d+(?:\.\d+)?)");
            if (numberMatch.Success)
            {
                return numberMatch.Groups[1].Value;
            }
            
            return null;
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
    }
}