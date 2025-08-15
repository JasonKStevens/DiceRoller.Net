using DiscordRollerBot.SlashCommands;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordRollerBot.Services
{
    public class SlashCommandRegistrationService : IHostedService
    {
        private readonly DiscordClient _discordClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SlashCommandRegistrationService> _logger;
        private SlashCommandsExtension _slashCommands;
        private TaskCompletionSource<bool> _readyTcs = new TaskCompletionSource<bool>();

        public SlashCommandRegistrationService(
            DiscordClient discordClient, 
            IServiceProvider serviceProvider,
            ILogger<SlashCommandRegistrationService> logger)
        {
            Console.WriteLine("SlashCommandRegistrationService CONSTRUCTOR called");
            _discordClient = discordClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _logger.LogInformation("SlashCommandRegistrationService constructor called");
            Console.WriteLine("SlashCommandRegistrationService CONSTRUCTOR completed");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("=== SlashCommandRegistrationService.StartAsync ===");
                
                // Set up Ready event handler to know when bot is connected
                _discordClient.Ready += OnClientReady;
                
                _logger.LogInformation($"Discord client state: {_discordClient?.CurrentUser?.Username ?? "Not connected yet"}");

                _slashCommands = _discordClient.UseSlashCommands(new SlashCommandsConfiguration()
                {
                    Services = _serviceProvider
                });
                _logger.LogInformation("SlashCommands extension initialized");

                // Handle slash command errors
                _slashCommands.SlashCommandErrored += OnSlashCommandError;
                _slashCommands.SlashCommandExecuted += OnSlashCommandExecuted;

                // Register commands when Ready event fires (non-blocking)
                _ = Task.Run(async () =>
                {
                    _logger.LogInformation("Waiting for Discord Ready event to register commands...");
                    
                    // Wait for Ready event
                    await _readyTcs.Task;
                    
                    _logger.LogInformation("Discord is ready, registering slash commands now...");
                    
                    _slashCommands.RegisterCommands<RollSlashCommand>();
                    _logger.LogInformation("RollSlashCommand registered globally (may take up to 1 hour to appear)");
                    
                    _slashCommands.RegisterCommands<GameSystemSlashCommands>();
                    _logger.LogInformation("GameSystemSlashCommands registered globally (may take up to 1 hour to appear)");
                    
                    _logger.LogInformation("All slash commands have been registered!");
                });

                _logger.LogInformation("SlashCommandRegistrationService started (waiting for Discord connection)");

                return Task.CompletedTask; // Return immediately, don't block
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register slash commands");
                throw;
            }
        }
        
        private async Task OnSlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs e)
        {
            _logger.LogInformation($"Slash command executed: {e.Context.CommandName} by {e.Context.User.Username}");
        }
        
        private Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            _logger.LogInformation($"Discord client Ready event fired - Bot user: {sender.CurrentUser.Username}#{sender.CurrentUser.Discriminator}");
            _readyTcs.TrySetResult(true);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.Ready -= OnClientReady;
            
            if (_slashCommands != null)
            {
                _slashCommands.SlashCommandErrored -= OnSlashCommandError;
                _slashCommands.SlashCommandExecuted -= OnSlashCommandExecuted;
            }
            
            return Task.CompletedTask;
        }

        private async Task OnSlashCommandError(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Slash command error occurred. Command: {CommandName}", e.Context?.CommandName ?? "Unknown");

            if (e.Context != null)
            {
                try
                {
                    await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                            .WithContent("An error occurred while processing your command. Please try again later.")
                            .AsEphemeral(true));
                }
                catch (Exception responseEx)
                {
                    _logger.LogError(responseEx, "Failed to send error response to user");
                }
            }
        }
    }
}