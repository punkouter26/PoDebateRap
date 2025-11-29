using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers.V2
{
    /// <summary>
    /// RESTful Debates API (v2) with proper resource naming conventions.
    /// Follows REST principles: nouns for resources, HTTP verbs for actions.
    /// </summary>
    [ApiController]
    [Route("api/v2/debates")]
    public class DebatesController : ControllerBase
    {
        private readonly IDebateOrchestrator _orchestrator;
        private readonly ILogger<DebatesController> _logger;

        public DebatesController(IDebateOrchestrator orchestrator, ILogger<DebatesController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Get the current debate state.
        /// </summary>
        [HttpGet("current")]
        public ActionResult<DebateState> GetCurrent()
        {
            return Ok(_orchestrator.CurrentState);
        }

        /// <summary>
        /// Start a new debate (creates a new debate resource).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DebateState>> Create([FromBody] StartDebateRequest request)
        {
            await _orchestrator.StartNewDebateAsync(request.Rapper1, request.Rapper2, request.Topic);
            return CreatedAtAction(nameof(GetCurrent), _orchestrator.CurrentState);
        }

        /// <summary>
        /// Delete (reset) the current debate.
        /// </summary>
        [HttpDelete("current")]
        public IActionResult DeleteCurrent()
        {
            _orchestrator.ResetDebate();
            return NoContent();
        }

        /// <summary>
        /// Update audio playback status for the current debate.
        /// </summary>
        [HttpPatch("current/audio-status")]
        public async Task<IActionResult> UpdateAudioStatus([FromBody] AudioStatusUpdate? update = null)
        {
            await _orchestrator.SignalAudioPlaybackCompleteAsync();
            return Ok();
        }
    }

    /// <summary>
    /// Audio status update request for PATCH endpoint.
    /// </summary>
    public record AudioStatusUpdate
    {
        public bool IsComplete { get; init; } = true;
    }
}
