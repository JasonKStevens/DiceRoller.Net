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

            return await Task.FromResult(true);
        }

        private static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        private async Task HandleMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsCurrent)
                return;

            _logger.LogInformation($"[{e.Message.MessageType}] ({e.Author}) {e.Message.Content}");
            
            var content = e.Message.Content.Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                string response = null;
                bool handled = false;

                string name = e.Author.Username;
                DiscordMember discordMember = (e.Author as DiscordMember);
                if (discordMember != null)
                    name = discordMember.DisplayName;

                var user = new DiscordUserInfo(){
                    Id = e.Author.Id.ToString(),
                    DisplayName = name
                };

                foreach (var processor in _commandProcessors)
                {
                    try
                    {
                        (handled, response) = processor.Process(user, content);

                        if (handled)
                            break;
                    } catch (Exception ex)
                    {
                        response = content + ": " + ex.Message;
                        _logger.LogError(ex, processor.Prefix);
                        break;
                    }
                }

                if (handled && response==null)
                    response  = "Unrecognised command prefix";

                if (response != null)
                {
                    if (!response.Contains("```"))
                    {
                        var luckyNums = new List<int>();
                        var nickname = discordMember?.Nickname ?? e.Author.Username;
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

                    await e.Message.RespondAsync(user.DisplayName + ": " + response);
                }

                return;
            }

            _logger.LogWarning("Message was not handled");

            return;
        }

        private async Task HandleTypedMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsCurrent)
                return;

            _logger.LogInformation($"[{e.Message.MessageType}] ({e.Author}) {e.Message.Content}");

            var content = e.Message.Content.Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                string response = null;
                bool handled = false;
                TypedResult rollResult = null;

                string name = e.Author.Username;
                DiscordMember discordMember = (e.Author as DiscordMember);
                if (discordMember != null)
                    name = discordMember.DisplayName;

                var user = new DiscordUserInfo()
                {
                    Id = e.Author.Id.ToString(),
                    DisplayName = name
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
                        response = content + ": " + ex.Message;
                        _logger.LogError(ex, processor.Prefix);
                        await e.Message.RespondAsync(user.DisplayName + ": " + response);

                        break;
                    }
                }

                if (handled && rollResult == TypedResult.Null)
                    response = "Unrecognised command prefix";

                if (rollResult != null)
                {
                    //visit the rollResult and build the output text
                    var outputBuilder = new DiscordTypedResultOutputVisitor(10);
                    response = outputBuilder.Visit(rollResult,1);

                    //check for lucky numbers, if applicable

                    if (!response.Contains("```"))
                    {
                        var luckyNums = new List<int>();
                        var nickname = discordMember?.Nickname ?? e.Author.Username;
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

                    await e.Message.RespondAsync(user.DisplayName + ": " + response);
                }

                return;
            }

            _logger.LogWarning("Message was not handled");

            return;
        }

        public async Task<bool> Stop()
        {
            _logger.LogInformation("Starting Discord Bot...");

            State = DiscordApiStatus.Stopping;

            _logger.LogInformation("Disconnecting");
            await _client.DisconnectAsync();

            State = DiscordApiStatus.Stopped;
            _logger.LogInformation("Stopped");

            return await Task.FromResult(true);
        }
    }
}
