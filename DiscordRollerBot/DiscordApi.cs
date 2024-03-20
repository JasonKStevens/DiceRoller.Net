using DiceRoller;
using DiceRoller.Parser;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRollerBot
{
    public class DiscordApi : IDiscordApi    
    {
        private readonly DiscordClient _client;
        private readonly DiscordApiConfiguration _config;
        private readonly ILogger<DiscordApi> _logger;
        private readonly IUserSettings _userSettings;

        private readonly IEnumerable<ICommandProcessor> _commandProcessors;

        private const string ButtonPrefix = "button_";
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

        private static readonly DiscordComponent[] _hitLocDropdown = new DiscordComponent[]
        {
            new DiscordSelectComponent($"{ButtonPrefix}hitloc", null, hitLocationOptions, false, 1, 1),
        };

        private static readonly List<DiscordComponent[]> buttons = new List<DiscordComponent[]>()
        {
            _diceButtons,
            _hitLocDropdown,
            _specialsButtons,
        };

        public DiscordApi(DiscordClient client, DiscordApiConfiguration config, IEnumerable<ICommandProcessor> commandProcessors, IUserSettings userSettings, ILogger<DiscordApi> logger = null)
        {
            _client = client;
            _config = config;
            _commandProcessors = commandProcessors;
            _userSettings = userSettings;
            _logger = logger ?? NullLogger<DiscordApi>.Instance;
        }

        public DiscordApiStatus State { get; private set; }

        public async Task<bool> Start()
        {
            _logger.LogInformation("Starting Discord Bot...");
            State = DiscordApiStatus.Starting;

            _client.MessageCreated += HandleTypedMessage;
            _logger.LogInformation("Connecting");
            await _client.ConnectAsync();

            State = DiscordApiStatus.Started;
            _logger.LogInformation("Started...");

            _client.ComponentInteractionCreated += HandleInteraction;

            return await Task.FromResult(true);
        }

        private static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        private async Task HandleTypedMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsCurrent)
                return;

            _logger.LogInformation($"[{e.Message.MessageType}] ({e.Author}) {e.Message.Content}");

            var content = e.Message.Content.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Message was not handled");
                return;
            }

            var response = GetResponse(e.Author, content);

            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogWarning("Message was not handled");
                return;
            }

            var builder = new DiscordMessageBuilder();
            AddButtons(e.Author.Id.ToString(), builder);

            if (response.Length < 2000)
            {
                builder.WithContent(response);
                await e.Message.RespondAsync(builder);
            }
            else
            {
                var fileName = Guid.NewGuid() + ".sav";

                //var data = new S
                StreamWriter outputFile = new StreamWriter(fileName);
                try
                {
                    await outputFile.WriteAsync(response);
                    outputFile.Close();
                    var fs = File.OpenRead(fileName);
                    try
                    {
                        builder.WithFile(fileName, fs);
                        await e.Message.RespondAsync(builder);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                finally
                {
                    File.Delete(fileName);
                }
            }
        }

        private void AddButtons(string userId, DiscordMessageBuilder builder)
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

        private string GetResponse(DiscordUser fromUser, string content)
        {
            string response;
            bool handled = false;
            TypedResult rollResult = null;

            string name = fromUser.Username;
            DiscordMember discordMember = (fromUser as DiscordMember);
            if (discordMember != null)
                name = discordMember.DisplayName;

            var user = new DiscordUserInfo()
            {
                Id = fromUser.Id.ToString(),
                DisplayName = name,
            };

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
                    _logger.LogError(ex, processor.Prefix);

                    response = content + ": " + ex.Message;
                    return user.DisplayName + ": " + response;
                }
            }

            if (handled && rollResult == TypedResult.Null)
                return "Unrecognised command prefix";

            if (rollResult == null)
                return null;

            //visit the rollResult and build the output text
            var outputBuilder = new DiscordTypedResultOutputVisitor(10);
            response = outputBuilder.Visit(rollResult, 1);

            //check for lucky numbers, if applicable

            if (!response.Contains("```"))
            {
                var luckyNums = new List<int>();
                var nickname = discordMember?.Nickname ?? name;
                if (nickname.Contains('[', ']'))
                {
                    var luckynums = nickname.Substring(nickname.IndexOf('[')+1, nickname.IndexOf(']') - nickname.IndexOf('[') - 1);
                    var nums = luckynums.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var num in nums)
                    {
                        if (Int32.TryParse(GetNumbers(num), out int value))
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

        private async Task HandleInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            var id = e.Id.Substring(ButtonPrefix.Length);
            var buttonCommand = $"{id}";

            if (id == "hitloc")
            {
                var loc = e.Values[0].Substring("hitloc_".Length);
                buttonCommand += $" {loc}";
            }

            var response = GetResponse(e.User, $"!roll {buttonCommand}");

            var userName = e.User.Username;
            if (e.User is DiscordMember)
            {
                if (!string.IsNullOrWhiteSpace((e.User as DiscordMember).Nickname))
                    userName = ( e.User as DiscordMember ).Nickname;
            }


            var builder = new DiscordMessageBuilder();

            AddButtons(e.User.Id.ToString(), builder);
            //foreach (var buttonList in buttons)
            //{
            //    builder.AddComponents(buttonList);
            //}
            builder.WithContent($"{userName}: !roll {buttonCommand}->{response}");

            await builder.SendAsync(e.Channel);
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            
            //await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }

        public async Task<bool> Stop()
        {
            _client.ComponentInteractionCreated -= HandleInteraction;
            _logger.LogInformation("Stopping Discord Bot...");

            State = DiscordApiStatus.Stopping;

            _logger.LogInformation("Disconnecting");
            await _client.DisconnectAsync();

            State = DiscordApiStatus.Stopped;
            _logger.LogInformation("Stopped");

            return await Task.FromResult(true);
        }
    }
}
