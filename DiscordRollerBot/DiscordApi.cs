using DiceRoller;
using DiceRoller.Parser;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
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

            _client.MessageCreated += HandleMessage;
            _logger.LogInformation("Connecting");
            await _client.ConnectAsync();

            State = DiscordApiStatus.Started;
            _logger.LogInformation("Started...");

            return await Task.FromResult(true);
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
                    await e.Message.RespondAsync(response);
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
