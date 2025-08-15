using DiceRoller;
using DiceRoller.Core.Parser;
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
    public class GameSystemSlashCommands : ApplicationCommandModule
    {
        private readonly IEnumerable<ICommandProcessor> _commandProcessors;
        private readonly IUserSettings _userSettings;
        private readonly ILogger<GameSystemSlashCommands> _logger;

        public GameSystemSlashCommands(IEnumerable<ICommandProcessor> commandProcessors, IUserSettings userSettings, ILogger<GameSystemSlashCommands> logger)
        {
            _commandProcessors = commandProcessors;
            _userSettings = userSettings;
            _logger = logger;
        }

        [SlashCommand("dq", "DragonQuest system commands")]
        public async Task DragonQuestCommand(InteractionContext ctx,
            [Autocomplete(typeof(DragonQuestAutocompleteProvider))]
            [Option("command", "DragonQuest command (backfire, injury, fear, etc.)")] string command,
            [Option("modifier", "Optional modifier or additional parameter")] string modifier = "",
            [Option("ephemeral", "Make the result visible only to you")] bool ephemeral = false)
        {
            await ProcessGameSystemCommand(ctx, "!dq", command, modifier, ephemeral);
        }

        [SlashCommand("hero", "Heroes system commands")]
        public async Task HeroesCommand(InteractionContext ctx,
            [Autocomplete(typeof(HeroesAutocompleteProvider))]
            [Option("command", "Heroes command (speed, etc.)")] string command,
            [Option("modifier", "Optional modifier or additional parameter")] string modifier = "",
            [Option("ephemeral", "Make the result visible only to you")] bool ephemeral = false)
        {
            await ProcessGameSystemCommand(ctx, "!hero", command, modifier, ephemeral);
        }

        [SlashCommand("party", "Party management commands")]
        public async Task PartyCommand(InteractionContext ctx,
            [Autocomplete(typeof(PartyAutocompleteProvider))]
            [Option("action", "Party action (create, join, leave, list, etc.)")] string action,
            [Option("parameter", "Optional parameter for the action")] string parameter = "",
            [Option("ephemeral", "Make the result visible only to you")] bool ephemeral = false)
        {
            await ProcessGameSystemCommand(ctx, "!party", action, parameter, ephemeral);
        }

        [SlashCommand("settings", "User settings management")]
        public async Task SettingsCommand(InteractionContext ctx,
            [Autocomplete(typeof(SettingsAutocompleteProvider))]
            [Option("action", "Settings action (set, get, delete, list)")] string action,
            [Option("setting", "Setting name")] string setting = "",
            [Option("value", "Setting value (for set command)")] string value = "",
            [Option("ephemeral", "Make the result visible only to you")] bool ephemeral = true)
        {
            var commandText = string.IsNullOrWhiteSpace(value) 
                ? $"!settings {action} {setting}".Trim()
                : $"!settings {action} {setting} {value}".Trim();
            
            await ProcessGameSystemCommand(ctx, "!settings", $"{action} {setting} {value}".Trim(), "", ephemeral);
        }

        private async Task ProcessGameSystemCommand(InteractionContext ctx, string prefix, string command, string modifier, bool ephemeral)
        {
            try
            {
                var user = new DiscordUserInfo()
                {
                    Id = ctx.User.Id.ToString(),
                    DisplayName = ctx.Member?.DisplayName ?? ctx.User.Username,
                    NickName = ctx.Member?.Nickname
                };

                var commandText = string.IsNullOrWhiteSpace(modifier) 
                    ? $"{prefix} {command}".Trim()
                    : $"{prefix} {command} {modifier}".Trim();

                var response = GetResponse(user, commandText);

                if (string.IsNullOrWhiteSpace(response))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Could not process the command.")
                            .AsEphemeral(ephemeral));
                    return;
                }

                // Handle long responses
                if (response.Length > 2000)
                {
                    var fileName = $"result_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                    var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
                    
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"{user.DisplayName}: {commandText}")
                            .AddFile(fileName, stream)
                            .AsEphemeral(ephemeral));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent(response)
                            .AsEphemeral(ephemeral));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing game system command: {Command}", command);
                
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Error processing command: {ex.Message}")
                        .AsEphemeral(true));
            }
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
    }

    public class DragonQuestAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            var input = ctx.FocusedOption.Value?.ToString()?.ToLower() ?? "";

            var dqCommands = new[]
            {
                "backfire", "injury", "specgrev", "fear", "hitloc", "hitlocation"
            };

            foreach (var cmd in dqCommands)
            {
                if (cmd.Contains(input))
                {
                    choices.Add(new DiscordAutoCompleteChoice(cmd, cmd));
                }
            }

            if (choices.Count == 0)
            {
                choices.AddRange(new[]
                {
                    new DiscordAutoCompleteChoice("backfire", "backfire"),
                    new DiscordAutoCompleteChoice("injury", "injury"),
                    new DiscordAutoCompleteChoice("fear", "fear"),
                    new DiscordAutoCompleteChoice("hitloc", "hitloc")
                });
            }

            return await Task.FromResult(choices);
        }
    }

    public class HeroesAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            var input = ctx.FocusedOption.Value?.ToString()?.ToLower() ?? "";

            var heroCommands = new[]
            {
                "speed"
            };

            foreach (var cmd in heroCommands)
            {
                if (cmd.Contains(input))
                {
                    choices.Add(new DiscordAutoCompleteChoice(cmd, cmd));
                }
            }

            if (choices.Count == 0)
            {
                choices.Add(new DiscordAutoCompleteChoice("speed", "speed"));
            }

            return await Task.FromResult(choices);
        }
    }

    public class PartyAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            var input = ctx.FocusedOption.Value?.ToString()?.ToLower() ?? "";

            var partyCommands = new[]
            {
                "create", "join", "leave", "list", "members", "delete", "invite", "kick"
            };

            foreach (var cmd in partyCommands)
            {
                if (cmd.Contains(input))
                {
                    choices.Add(new DiscordAutoCompleteChoice(cmd, cmd));
                }
            }

            if (choices.Count == 0)
            {
                choices.AddRange(new[]
                {
                    new DiscordAutoCompleteChoice("create", "create"),
                    new DiscordAutoCompleteChoice("join", "join"),
                    new DiscordAutoCompleteChoice("list", "list"),
                    new DiscordAutoCompleteChoice("members", "members")
                });
            }

            return await Task.FromResult(choices);
        }
    }

    public class SettingsAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var choices = new List<DiscordAutoCompleteChoice>();
            var input = ctx.FocusedOption.Value?.ToString()?.ToLower() ?? "";

            if (ctx.FocusedOption.Name == "action")
            {
                var settingsActions = new[]
                {
                    "set", "get", "delete", "list"
                };

                foreach (var action in settingsActions)
                {
                    if (action.Contains(input))
                    {
                        choices.Add(new DiscordAutoCompleteChoice(action, action));
                    }
                }
            }
            else if (ctx.FocusedOption.Name == "setting")
            {
                var commonSettings = new[]
                {
                    "showDiceButtons", "showHitLocations", "showSpecialButtons"
                };

                foreach (var setting in commonSettings)
                {
                    if (setting.ToLower().Contains(input))
                    {
                        choices.Add(new DiscordAutoCompleteChoice(setting, setting));
                    }
                }
            }

            return await Task.FromResult(choices);
        }
    }
}