using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(IDiagnosticsService diagnosticsService, ILogger<DiagnosticsController> logger)
        {
            _diagnosticsService = diagnosticsService;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<DiagnosticResult>>> GetAllDiagnostics()
        {
            try
            {
                var results = await _diagnosticsService.RunAllChecksAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running diagnostics");
                return StatusCode(500, "Error running diagnostics");
            }
        }
    }
}
