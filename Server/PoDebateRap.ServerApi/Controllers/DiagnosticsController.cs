using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Controller for running system diagnostics and health checks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(IDiagnosticsService diagnosticsService, ILogger<DiagnosticsController> logger)
        {
            _diagnosticsService = diagnosticsService;
            _logger = logger;
        }

        /// <summary>
        /// Runs all diagnostic checks and returns detailed results.
        /// </summary>
        /// <returns>A list of diagnostic results for each checked service.</returns>
        /// <response code="200">Returns the diagnostic results.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<DiagnosticResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DiagnosticResult>>> GetAllDiagnostics()
        {
            var results = await _diagnosticsService.RunAllChecksAsync();
            return Ok(results);
        }

        /// <summary>
        /// Runs all diagnostic checks and returns detailed results.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use GET /api/Diagnostics instead.
        /// </remarks>
        [HttpGet("all")]
        [Obsolete("Use GET /api/Diagnostics instead")]
        [ProducesResponseType(typeof(List<DiagnosticResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DiagnosticResult>>> GetAllDiagnosticsLegacy()
        {
            return await GetAllDiagnostics();
        }
    }
}
