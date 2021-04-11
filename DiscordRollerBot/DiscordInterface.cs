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
        private readonly DiscordInterfaceConfiguration _config;
        private readonly ILogger<DiscordApi> _logger;

        private readonly List<CommandRegistration> _commandHandlers = new List<CommandRegistration>();
        public DiscordApi(DiscordClient client, DiscordInterfaceConfiguration config, ILogger<DiscordApi> logger = null)
        {
            _client = client;
            _config = config;
            _logger = logger ?? NullLogger<DiscordApi>.Instance;
        }

        public DiscordInterfaceStatus State { get; private set; }

        public void AddHandler(string commandPrefix, Func<string, string> handler)
        {
            _commandHandlers.Add(new CommandRegistration(commandPrefix, handler));
        }

        public async Task<bool> Start()
        {
            _logger.LogInformation("Starting Discord Bot...");
            State = DiscordInterfaceStatus.Starting;

            _client.MessageCreated += HandleMessage;
            _logger.LogInformation("Connecting");
            await _client.ConnectAsync();

            State = DiscordInterfaceStatus.Started;
            _logger.LogInformation("Started...");

            return await Task.FromResult(true);
        }

        private async Task HandleMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsCurrent)
                return;

            _logger.LogInformation($"[{e.Message.MessageType}] ({e.Author}) {e.Message.Content}");
            
            var content = e.Message.Content.Trim();
            var tokens = content.Split(' ', StringSplitOptions.None);

            if (tokens.Length > 0)
            {
                string response = null;
                var instructions = String.Join(' ', tokens, 1, tokens.Length-1);
                bool handled = false;

                string name = e.Author.Username;
                DiscordMember discordMember = (e.Author as DiscordMember);
                if (discordMember != null)
                    name = discordMember.DisplayName;



                foreach (var handler in _commandHandlers)
                {
                    try
                    {
                        (handled, response) = handler.Handle(tokens[0], instructions);
                        if (handled)
                        {
                            response = name + "l: " + response;
                            break;
                        } 
                    } catch (InvalidOperationException iex)
                    {
                        response = name + ": " + iex.Message;
                        break;
                    }
                }

                if (handled && response==null)
                    response  = "Unrecognised command prefix";

                await e.Message.RespondAsync(response);
                return;
            }

            _logger.LogWarning("Message was not handled");

            return;
        } 

        public async Task<bool> Stop()
        {
            _logger.LogInformation("Starting Discord Bot...");

            State = DiscordInterfaceStatus.Stopping;

            _logger.LogInformation("Disconnecting");
            await _client.DisconnectAsync();

            State = DiscordInterfaceStatus.Stopped;
            _logger.LogInformation("Stopped");

            return await Task.FromResult(true);
        }
    }
}
