using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RappersController : ControllerBase
    {
        private readonly IRapperRepository _rapperRepository;
        private readonly ILogger<RappersController> _logger;

        public RappersController(IRapperRepository rapperRepository, ILogger<RappersController> logger)
        {
            _rapperRepository = rapperRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rapper>>> Get()
        {
            var rappers = await _rapperRepository.GetAllRappersAsync();
            return Ok(rappers);
        }

        [HttpPost("update-win-loss")]
        public async Task<IActionResult> UpdateWinLossRecord([FromQuery] string winnerName, [FromQuery] string loserName)
        {
            await _rapperRepository.UpdateWinLossRecordAsync(winnerName, loserName);
            return Ok();
        }
    }
}
