using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller;
using DiceRoller.DragonQuest;
using DiceRoller.Core.Parser;
using DiscordRollerBot;
using Irony.Parsing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiceRollerCmd
{
    public class BotHost : IHostedService
    {
        private readonly ILogger<BotHost> _logger;
        private readonly IDiscordApi _discordInterface;

        public BotHost(ILogger<BotHost> logger, IHostApplicationLifetime appLifetime, IDiscordApi discord)
        {
            Console.WriteLine("BotHost CONSTRUCTOR called");
            _logger = logger;
            _discordInterface = discord;
            _logger.LogInformation("BotHost constructor called - Discord interface injected");
            Console.WriteLine("BotHost CONSTRUCTOR completed");
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BotHost.StartAsync called - Starting Discord interface...");
            try
            {
                await _discordInterface.Start();
                _logger.LogInformation("BotHost.StartAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Discord interface");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Stop();
        }
    }
}
