using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DebateController : ControllerBase
    {
        private readonly IDebateOrchestrator _orchestrator;
        private readonly ILogger<DebateController> _logger;

        public DebateController(IDebateOrchestrator orchestrator, ILogger<DebateController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<ActionResult<DebateState>> StartDebate([FromBody] StartDebateRequest request)
        {
            if (request?.Rapper1 == null || request.Rapper2 == null || request.Topic == null)
            {
                return BadRequest("Invalid request: Rappers and Topic must be provided.");
            }

            if (string.IsNullOrWhiteSpace(request.Rapper1.Name) || string.IsNullOrWhiteSpace(request.Rapper2.Name))
            {
                return BadRequest("Invalid request: Rapper names cannot be empty.");
            }

            if (request.Rapper1.Name == request.Rapper2.Name)
            {
                return BadRequest("Invalid request: Rappers cannot be the same.");
            }

            if (string.IsNullOrWhiteSpace(request.Topic.Title))
            {
                return BadRequest("Invalid request: Topic title cannot be empty.");
            }

            try
            {
                await _orchestrator.StartNewDebateAsync(request.Rapper1, request.Rapper2, request.Topic);
                return Ok(_orchestrator.CurrentState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting debate.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("state")]
        public ActionResult<DebateState> GetCurrentState()
        {
            _logger.LogInformation("GetCurrentState: Returning state. IsDebateInProgress: {InProg}, IsGeneratingTurn: {Gen}, CurrentTurnText: '{Text}'",
                _orchestrator.CurrentState.IsDebateInProgress, _orchestrator.CurrentState.IsGeneratingTurn, _orchestrator.CurrentState.CurrentTurnText);
            return Ok(_orchestrator.CurrentState);
        }

        [HttpPost("signal-audio-complete")]
        public async Task<IActionResult> SignalAudioPlaybackComplete()
        {
            try
            {
                await _orchestrator.SignalAudioPlaybackCompleteAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signaling audio playback complete.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reset")]
        public IActionResult ResetDebate()
        {
            try
            {
                _orchestrator.ResetDebate();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting debate.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
