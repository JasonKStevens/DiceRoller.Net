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
            _logger = logger;
            _discordInterface = discord;
            _logger.LogInformation("BotHost constructor called - Discord interface injected");
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("BotHost.StartAsync called - Starting Discord interface...");
            _logger.LogInformation("BotHost.StartAsync called - Starting Discord interface...");
            try
            {
                var result = await _discordInterface.Start();
                Console.WriteLine($"Discord interface started: {result}");
                _logger.LogInformation("BotHost.StartAsync completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR starting Discord: {ex.Message}");
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
