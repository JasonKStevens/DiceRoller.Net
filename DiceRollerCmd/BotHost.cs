using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceRoller;
using DiceRoller.DragonQuest;
using DiceRoller.Parser;
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
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordInterface.Stop();
        }
    }

    //public class ServiceStateHost : BackgroundService
    //{
    //    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    //    {

    //    }
    //}
}
