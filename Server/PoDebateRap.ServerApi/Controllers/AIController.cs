using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAzureOpenAIService _openAIService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAzureOpenAIService openAIService, ILogger<AIController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost("generate-debate-turn")]
        public async Task<ActionResult<string>> GenerateDebateTurn([FromBody] GenerateDebateTurnRequest request)
        {
            try
            {
                var response = await _openAIService.GenerateDebateTurnAsync(request.Prompt, request.MaxTokens, CancellationToken.None);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating debate turn.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("judge-debate")]
        public async Task<ActionResult<JudgeDebateResponse>> JudgeDebate([FromBody] JudgeDebateRequest request)
        {
            try
            {
                var response = await _openAIService.JudgeDebateAsync(request.DebateTranscript, request.Rapper1Name, request.Rapper2Name, request.Topic, CancellationToken.None);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error judging debate.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("generate-speech")]
        public async Task<ActionResult<byte[]>> GenerateSpeech([FromBody] GenerateSpeechRequest request)
        {
            try
            {
                var audioBytes = await _openAIService.GenerateSpeechAsync(request.Text, request.VoiceName, CancellationToken.None);
                return File(audioBytes, "audio/wav"); // Assuming WAV format
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating speech.");
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
