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
        public async Task<ActionResult<DiagnosticResult>> GetHealth()
        {
            _logger.LogInformation("Health check requested");

            var diagnosticResult = new DiagnosticResult
            {
                Timestamp = DateTime.UtcNow,
                Checks = new Dictionary<string, string>()
            };

            try
            {
                // API Health
                diagnosticResult.Checks["API"] = await _diagnosticsService.CheckApiHealthAsync();

                // Storage Connection
                diagnosticResult.Checks["Storage"] = await _diagnosticsService.CheckDataConnectionAsync();

                // Azure OpenAI
                diagnosticResult.Checks["AzureOpenAI"] = await _diagnosticsService.CheckAzureOpenAIServiceAsync();

                // Text-to-Speech
                diagnosticResult.Checks["TextToSpeech"] = await _diagnosticsService.CheckTextToSpeechServiceAsync();

                // Internet Connection
                diagnosticResult.Checks["Internet"] = await _diagnosticsService.CheckInternetConnectionAsync();

                // News API
                diagnosticResult.Checks["NewsAPI"] = await _diagnosticsService.CheckNewsServiceAsync();

                // Determine overall health
                diagnosticResult.IsHealthy = diagnosticResult.Checks.Values.All(v => 
                    v.Contains("OK") || 
                    v.Contains("working") || 
                    v.Contains("healthy") || 
                    v.Contains("available"));

                _logger.LogInformation("Health check completed. Overall health: {IsHealthy}", diagnosticResult.IsHealthy);

                return Ok(diagnosticResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                diagnosticResult.IsHealthy = false;
                diagnosticResult.Checks["Error"] = $"Health check failed: {ex.Message}";
                return StatusCode(500, diagnosticResult);
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
                // Quick check of critical services
                var apiCheck = await _diagnosticsService.CheckApiHealthAsync();
                var isReady = apiCheck.Contains("healthy") || apiCheck.Contains("OK");

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
}
