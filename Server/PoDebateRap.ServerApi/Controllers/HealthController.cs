using Microsoft.AspNetCore.Mvc;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Legacy health controller - redirects to proper health check endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Redirects to the main health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            _logger.LogInformation("Legacy health endpoint called, redirecting to /api/health");
            return Redirect("/api/health");
        }

        /// <summary>
        /// Redirects to liveness probe
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLiveness()
        {
            return Redirect("/api/health/live");
        }

        /// <summary>
        /// Redirects to readiness probe
        /// </summary>
        [HttpGet("ready")]
        public IActionResult GetReadiness()
        {
            return Redirect("/api/health/ready");
        }
    }
}
