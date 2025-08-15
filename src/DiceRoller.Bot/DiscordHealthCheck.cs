using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordRollerBot;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DiceRollerCmd
{
    public class DiscordHealthCheck : IHealthCheck
    {
        private readonly IDiscordApi _discordApi;

        public DiscordHealthCheck(IDiscordApi discordApi)
        {
            _discordApi = discordApi;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var status = _discordApi.State;
                
                return status switch
                {
                    DiscordApiStatus.Started => Task.FromResult(HealthCheckResult.Healthy("Discord bot is connected and running")),
                    DiscordApiStatus.Starting => Task.FromResult(HealthCheckResult.Degraded("Discord bot is starting")),
                    DiscordApiStatus.Stopping => Task.FromResult(HealthCheckResult.Degraded("Discord bot is stopping")),
                    DiscordApiStatus.Stopped => Task.FromResult(HealthCheckResult.Unhealthy("Discord bot is stopped")),
                    _ => Task.FromResult(HealthCheckResult.Unhealthy("Discord bot status unknown"))
                };
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Failed to check Discord bot status", ex));
            }
        }
    }
}