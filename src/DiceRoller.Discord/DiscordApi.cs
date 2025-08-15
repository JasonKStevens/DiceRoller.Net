using DiceRoller;
using DiceRoller.Core.Parser;
using DiscordRollerBot.Services;
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
        private readonly InteractionHandler _interactionHandler;

        private readonly IEnumerable<ICommandProcessor> _commandProcessors;



        public DiscordApi(DiscordClient client, DiscordApiConfiguration config, IEnumerable<ICommandProcessor> commandProcessors, IUserSettings userSettings, InteractionHandler interactionHandler, ILogger<DiscordApi> logger = null)
        {
            Console.WriteLine("DiscordApi CONSTRUCTOR called");
            _client = client;
            _config = config;
            _commandProcessors = commandProcessors;
            _userSettings = userSettings;
            _interactionHandler = interactionHandler;
            _logger = logger ?? NullLogger<DiscordApi>.Instance;
            Console.WriteLine("DiscordApi CONSTRUCTOR completed");
        }

        public DiscordApiStatus State { get; private set; }

        public async Task<bool> Start()
        {
            _logger.LogInformation("Starting Discord Bot...");
            _logger.LogInformation($"Token present: {!string.IsNullOrEmpty(_config.Token)}");
            _logger.LogInformation($"EnableSlashCommands: {_config.EnableSlashCommands}");
            _logger.LogInformation($"EnablePrefixCommands: {_config.EnablePrefixCommands}");
            _logger.LogInformation($"EnableButtonInteractions: {_config.EnableButtonInteractions}");
            
            State = DiscordApiStatus.Starting;

            // Register event handlers based on configuration
            if (_config.EnablePrefixCommands)
            {
                _client.MessageCreated += HandleTypedMessage;
                _logger.LogInformation("Prefix commands enabled - MessageCreated handler attached");
            }

            if (_config.EnableButtonInteractions)
            {
                _client.ComponentInteractionCreated += _interactionHandler.HandleComponentInteraction;
                _logger.LogInformation("Button interactions enabled - ComponentInteractionCreated handler attached");
            }

            _logger.LogInformation("Connecting to Discord...");
            try
            {
                await _client.ConnectAsync();
                _logger.LogInformation($"Connected successfully! Bot user: {_client.CurrentUser?.Username}#{_client.CurrentUser?.Discriminator}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Discord");
                throw;
            }

            State = DiscordApiStatus.Started;
            _logger.LogInformation("Discord Bot Started successfully!");

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
                        builder.AddFile(fileName, fs);
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
            if (_config.EnableButtonInteractions)
            {
                _interactionHandler.AddButtons(userId, builder);
            }
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



        public async Task<bool> Stop()
        {
            // Unregister event handlers
            if (_config.EnablePrefixCommands)
            {
                _client.MessageCreated -= HandleTypedMessage;
            }

            if (_config.EnableButtonInteractions)
            {
                _client.ComponentInteractionCreated -= _interactionHandler.HandleComponentInteraction;
            }

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
