using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers.V2
{
    /// <summary>
    /// RESTful Rappers API (v2) with proper resource naming conventions.
    /// </summary>
    [ApiController]
    [Route("api/v2/rappers")]
    public class RappersController : ControllerBase
    {
        private readonly IRapperRepository _rapperRepository;
        private readonly ILogger<RappersController> _logger;

        public RappersController(IRapperRepository rapperRepository, ILogger<RappersController> logger)
        {
            _rapperRepository = rapperRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all rappers.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rapper>>> GetAll()
        {
            var rappers = await _rapperRepository.GetAllRappersAsync();
            return Ok(rappers);
        }

        /// <summary>
        /// Get a rapper by name.
        /// </summary>
        [HttpGet("{name}")]
        public async Task<ActionResult<Rapper>> GetByName(string name)
        {
            var rappers = await _rapperRepository.GetAllRappersAsync();
            var rapper = rappers.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (rapper is null)
            {
                return NotFound($"Rapper '{name}' not found");
            }
            
            return Ok(rapper);
        }

        /// <summary>
        /// Update a rapper's statistics (wins/losses).
        /// </summary>
        [HttpPatch("{name}/stats")]
        public async Task<IActionResult> UpdateStats(string name, [FromBody] RapperStatsUpdate update)
        {
            var rappers = await _rapperRepository.GetAllRappersAsync();
            var rapper = rappers.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (rapper is null)
            {
                return NotFound($"Rapper '{name}' not found");
            }

            if (update.AddWin && !string.IsNullOrEmpty(update.OpponentName))
            {
                await _rapperRepository.UpdateWinLossRecordAsync(name, update.OpponentName);
            }
            else if (update.AddLoss && !string.IsNullOrEmpty(update.OpponentName))
            {
                await _rapperRepository.UpdateWinLossRecordAsync(update.OpponentName, name);
            }

            return Ok();
        }
    }

    /// <summary>
    /// Rapper statistics update request.
    /// </summary>
    public record RapperStatsUpdate
    {
        public bool AddWin { get; init; }
        public bool AddLoss { get; init; }
        public string? OpponentName { get; init; }
    }
}
