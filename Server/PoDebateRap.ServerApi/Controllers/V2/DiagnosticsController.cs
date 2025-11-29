using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers.V2
{
    /// <summary>
    /// RESTful Diagnostics API (v2) with proper resource naming conventions.
    /// </summary>
    [ApiController]
    [Route("api/v2/diagnostics")]
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
        /// Get all diagnostic results (RESTful: GET /diagnostics instead of GET /diagnostics/all).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DiagnosticResult>>> GetAll()
        {
            var results = await _diagnosticsService.RunAllChecksAsync();
            return Ok(results);
        }

        /// <summary>
        /// Get a specific diagnostic check by name.
        /// </summary>
        [HttpGet("{checkName}")]
        public async Task<ActionResult<DiagnosticResult>> GetByName(string checkName)
        {
            var results = await _diagnosticsService.RunAllChecksAsync();
            var result = results.FirstOrDefault(r => 
                r.CheckName.Equals(checkName, StringComparison.OrdinalIgnoreCase));
            
            if (result is null)
            {
                return NotFound($"Diagnostic check '{checkName}' not found");
            }
            
            return Ok(result);
        }
    }
}
