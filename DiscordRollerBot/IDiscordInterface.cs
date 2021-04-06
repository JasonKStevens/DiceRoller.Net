using DiceRoller.Parser;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordRollerBot
{
    public interface IDiscordInterface
    {
        Task<bool> Start();
        Task<bool> Stop();

        DiscordInterfaceStatus State {get; }
    }

    public class DiscordInterface : IDiscordInterface    
    {
        private readonly DiscordClient _client;
        private readonly DiscordInterfaceConfiguration _config;
        private readonly Evaluator _evaluator;
        private readonly ILogger<DiscordInterface> _logger;

        public DiscordInterface(DiscordClient client, DiscordInterfaceConfiguration config, Evaluator evaluator, ILogger<DiscordInterface> logger = null)
        {
            _client = client;
            _config = config;
            _evaluator = evaluator;
            _logger = logger ?? NullLogger<DiscordInterface>.Instance;
        }

        public DiscordInterfaceStatus State { get; private set; }

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
            _logger.LogInformation(e.Message.Content);
            
            var content = e.Message.Content.Trim();
            var tokens = content.Split(' ', StringSplitOptions.None);

            if (tokens.Length > 0)
            {
                string response = "Unrecognised command prefix";

                if (tokens[0].ToLower().Contains(_config.CommandPrefix))
                {
                    var instructions = String.Join(' ', tokens, 1, tokens.Length-1);

                    try
                    {
                        var result = _evaluator.Evaluate(instructions);
                        response = result.Value + " Reason: " + result.Breakdown;
                    } catch (InvalidOperationException iex)
                    {
                        response = iex.Message;
                    }
                }

                await e.Message.RespondAsync(response);

                return;
            }

            _logger.LogWarning("Message was not handled");
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
