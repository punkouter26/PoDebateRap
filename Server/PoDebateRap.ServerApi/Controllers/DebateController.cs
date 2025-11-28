using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Controller for managing rap debate sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DebateController : ControllerBase
    {
        private readonly IDebateOrchestrator _orchestrator;
        private readonly ILogger<DebateController> _logger;

        public DebateController(IDebateOrchestrator orchestrator, ILogger<DebateController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Initiates a new rap debate between two rappers.
        /// </summary>
        /// <param name="request">The debate configuration including rapper IDs and topic.</param>
        /// <returns>The initial debate state with session information.</returns>
        /// <response code="200">Debate started successfully.</response>
        /// <response code="400">Invalid rapper selection or topic.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DebateState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DebateState>> CreateDebate([FromBody] StartDebateRequest request)
        {
            await _orchestrator.StartNewDebateAsync(request.Rapper1, request.Rapper2, request.Topic);
            return Ok(_orchestrator.CurrentState);
        }

        /// <summary>
        /// Initiates a new rap debate between two rappers.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use POST /api/Debate instead.
        /// </remarks>
        [HttpPost("start")]
        [Obsolete("Use POST /api/Debate instead")]
        [ProducesResponseType(typeof(DebateState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DebateState>> StartDebate([FromBody] StartDebateRequest request)
        {
            return await CreateDebate(request);
        }

        /// <summary>
        /// Retrieves the current state of the active debate.
        /// </summary>
        /// <returns>The current debate state including turn information and transcripts.</returns>
        /// <response code="200">Returns the current debate state.</response>
        [HttpGet("current")]
        [ProducesResponseType(typeof(DebateState), StatusCodes.Status200OK)]
        public ActionResult<DebateState> GetCurrentDebate()
        {
            _logger.LogInformation("GetCurrentDebate: Returning state. IsDebateInProgress: {InProg}, IsGeneratingTurn: {Gen}, CurrentTurnText: '{Text}'",
                _orchestrator.CurrentState.IsDebateInProgress, _orchestrator.CurrentState.IsGeneratingTurn, _orchestrator.CurrentState.CurrentTurnText);
            return Ok(_orchestrator.CurrentState);
        }

        /// <summary>
        /// Retrieves the current state of the active debate.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use GET /api/Debate/current instead.
        /// </remarks>
        [HttpGet("state")]
        [Obsolete("Use GET /api/Debate/current instead")]
        [ProducesResponseType(typeof(DebateState), StatusCodes.Status200OK)]
        public ActionResult<DebateState> GetCurrentState()
        {
            return GetCurrentDebate();
        }

        /// <summary>
        /// Updates the audio playback status to signal completion.
        /// </summary>
        /// <returns>Acknowledgement of the status update.</returns>
        /// <response code="200">Audio status updated successfully.</response>
        [HttpPatch("current/audio-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAudioStatus()
        {
            await _orchestrator.SignalAudioPlaybackCompleteAsync();
            return Ok();
        }

        /// <summary>
        /// Signals that audio playback has completed.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use PATCH /api/Debate/current/audio-status instead.
        /// </remarks>
        [HttpPost("signal-audio-complete")]
        [Obsolete("Use PATCH /api/Debate/current/audio-status instead")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SignalAudioPlaybackComplete()
        {
            return await UpdateAudioStatus();
        }

        /// <summary>
        /// Cancels and resets the current debate session.
        /// </summary>
        /// <returns>Acknowledgement of the reset.</returns>
        /// <response code="204">Debate reset successfully.</response>
        [HttpDelete("current")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteCurrentDebate()
        {
            _orchestrator.ResetDebate();
            return NoContent();
        }

        /// <summary>
        /// Resets the current debate session.
        /// </summary>
        /// <remarks>
        /// **Deprecated**: Use DELETE /api/Debate/current instead.
        /// </remarks>
        [HttpPost("reset")]
        [Obsolete("Use DELETE /api/Debate/current instead")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ResetDebate()
        {
            _orchestrator.ResetDebate();
            return Ok();
        }
    }
}
