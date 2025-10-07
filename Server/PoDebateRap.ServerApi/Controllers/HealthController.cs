using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IDiagnosticsService diagnosticsService, ILogger<HealthController> logger)
        {
            _diagnosticsService = diagnosticsService;
            _logger = logger;
        }

        /// <summary>
        /// Comprehensive health check for all external dependencies
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<HealthCheckResult>> GetHealth()
        {
            _logger.LogInformation("Health check requested");

            try
            {
                var results = await _diagnosticsService.RunAllChecksAsync();
                var isHealthy = results.All(r => r.Success);

                var healthResult = new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Timestamp = DateTime.UtcNow,
                    Checks = results.ToDictionary(r => r.CheckName, r => r.Message)
                };

                _logger.LogInformation("Health check completed. Overall health: {IsHealthy}", isHealthy);
                return Ok(healthResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                return StatusCode(500, new HealthCheckResult
                {
                    IsHealthy = false,
                    Timestamp = DateTime.UtcNow,
                    Checks = new Dictionary<string, string> { ["Error"] = $"Health check failed: {ex.Message}" }
                });
            }
        }

        /// <summary>
        /// Quick health check (liveness probe)
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLiveness()
        {
            return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Readiness check
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                var results = await _diagnosticsService.RunAllChecksAsync();
                var isReady = results.Any() && results.First().Success;

                if (isReady)
                {
                    return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
                }
                else
                {
                    return StatusCode(503, new { status = "not ready", timestamp = DateTime.UtcNow });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { status = "not ready", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }

    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Checks { get; set; } = new();
    }
}
