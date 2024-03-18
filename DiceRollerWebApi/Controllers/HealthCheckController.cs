using DiscordRollerBot;
using Microsoft.AspNetCore.Mvc;

namespace DiceRollerWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase 
    {
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(ILogger<HealthCheckController> logger = null)
        {
            _logger = logger;
        }

        [Route("liveness")]
        public IActionResult Liveness()
        {
            return Ok();
        }

        [Route("readiness")]
        public IActionResult Readiness()
        {
            return Ok();
        }
    }
}