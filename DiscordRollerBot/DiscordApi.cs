using DiceRoller;
using DiceRoller.Parser;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
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

        private readonly IEnumerable<ICommandProcessor> _commandProcessors;

        private const string ButtonPrefix = "button_";
        private static readonly List<DiscordButtonComponent[]> buttons = new List<DiscordButtonComponent[]>()
        {
            new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, $"{ButtonPrefix}d100", "d100"),
                new DiscordButtonComponent(ButtonStyle.Success, $"{ButtonPrefix}d20",  "d20"),
                new DiscordButtonComponent(ButtonStyle.Success, $"{ButtonPrefix}d10",  "d10"),
                new DiscordButtonComponent(ButtonStyle.Success, $"{ButtonPrefix}d6",   "d6"),
                new DiscordButtonComponent(ButtonStyle.Success, $"{ButtonPrefix}d4",   "d4"),
            },
            new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ButtonPrefix}hitloc", "HitLoc Humanoid"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ButtonPrefix}hitloc quadruped_mid", "HitLoc Quadruped"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ButtonPrefix}hitloc avian_mid", "HitLoc Avian"),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{ButtonPrefix}hitloc serpent_mid", "HitLoc Serpentine"),
            },
            new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}specgrev", "Spec Griev"),
                new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}backfire", "Backfire"),
                new DiscordButtonComponent(ButtonStyle.Danger, $"{ButtonPrefix}fear", "Fear"),
            },
        };

        public DiscordApi(DiscordClient client, DiscordApiConfiguration config, IEnumerable<ICommandProcessor> commandProcessors, ILogger<DiscordApi> logger = null)
        {
            _client = client;
            _config = config;
            _commandProcessors = commandProcessors;
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
            builder.WithContent(response);

            foreach (var buttonList in buttons)
            {
                builder.AddComponents(buttonList);
            }

            await e.Message.RespondAsync(builder);
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
            var buttonCommand = $"!roll {e.Id.Substring(ButtonPrefix.Length)}"; 

            var response = GetResponse(e.User, buttonCommand);

            var builder = new DiscordInteractionResponseBuilder();
            builder.WithContent(e.User.Username + ": " + e.Id.Substring(ButtonPrefix.Length) + Environment.NewLine + response);

            foreach (var buttonList in buttons)
            {
                builder.AddComponents(buttonList);
            }

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
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
