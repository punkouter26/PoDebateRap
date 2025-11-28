using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Controller for managing rapper profiles and records.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        /// Retrieves all available rappers.
        /// </summary>
        /// <returns>A list of all rappers with their profiles and statistics.</returns>
        /// <response code="200">Returns the list of rappers.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Rapper>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Rapper>>> GetAllRappers()
        {
            var rappers = await _rapperRepository.GetAllRappersAsync();
            return Ok(rappers);
        }

        /// <summary>
        /// Updates the win/loss record for a specific rapper after a battle.
        /// </summary>
        /// <param name="id">The rapper ID to update.</param>
        /// <param name="request">The win/loss update request containing result information.</param>
        /// <returns>Acknowledgement of the update.</returns>
        /// <response code="200">Record updated successfully.</response>
        /// <response code="404">Rapper not found.</response>
        [HttpPatch("{id}/record")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRapperRecord(string id, [FromBody] UpdateRecordRequest request)
        {
            await _rapperRepository.UpdateWinLossRecordAsync(request.WinnerName, request.LoserName);
            return Ok();
        }

        /// <summary>
        /// Updates the win/loss record for rappers after a battle.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use PATCH /api/Rappers/{id}/record instead.
        /// </remarks>
        [HttpPost("update-win-loss")]
        [Obsolete("Use PATCH /api/Rappers/{id}/record instead")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateWinLossRecord([FromQuery] string winnerName, [FromQuery] string loserName)
        {
            await _rapperRepository.UpdateWinLossRecordAsync(winnerName, loserName);
            return Ok();
        }
    }

    /// <summary>
    /// Request model for updating rapper win/loss records.
    /// </summary>
    public class UpdateRecordRequest
    {
        /// <summary>
        /// Name of the winning rapper.
        /// </summary>
        public string WinnerName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the losing rapper.
        /// </summary>
        public string LoserName { get; set; } = string.Empty;
    }
}
