using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiceRoller.Bot.Services
{
    /// <summary>
    /// Hosted service that initializes plugin integration on startup
    /// </summary>
    public class PluginStartupService : IHostedService
    {
        private readonly ILogger<PluginStartupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PluginStartupService(
            ILogger<PluginStartupService> logger, 
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Plugin Startup Service");

            try
            {
                // Get the plugin integration service and initialize it
                var pluginService = _serviceProvider.GetRequiredService<IPluginIntegrationService>();
                await pluginService.InitializeAsync(cancellationToken);

                _logger.LogInformation("Plugin integration initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize plugin integration");
                // Don't throw - we want the bot to start even if plugins fail
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Plugin Startup Service");
            return Task.CompletedTask;
        }
    }
}