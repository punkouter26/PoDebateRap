using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Diagnostics;
using System.Threading.Tasks;

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

        [HttpGet("api-health")]
        public async Task<ActionResult<string>> GetApiHealth()
        {
            return Ok(await _diagnosticsService.CheckApiHealthAsync());
        }

        [HttpGet("data-connection")]
        public async Task<ActionResult<string>> GetDataConnection()
        {
            return Ok(await _diagnosticsService.CheckDataConnectionAsync());
        }

        [HttpGet("internet-connection")]
        public async Task<ActionResult<string>> GetInternetConnection()
        {
            return Ok(await _diagnosticsService.CheckInternetConnectionAsync());
        }

        [HttpGet("authentication-service")]
        public async Task<ActionResult<string>> GetAuthenticationService()
        {
            return Ok(await _diagnosticsService.CheckAuthenticationServiceAsync());
        }
    }
}
