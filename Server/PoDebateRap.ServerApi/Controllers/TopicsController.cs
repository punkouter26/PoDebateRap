using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.ServerApi.Extensions;
using PoDebateRap.Shared.Models;
using System.Linq;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Controller for managing debate topics derived from news headlines.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TopicsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<TopicsController> _logger;

        public TopicsController(INewsService newsService, ILogger<TopicsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of available debate topics from current news headlines.
        /// </summary>
        /// <returns>A list of topics suitable for rap debates.</returns>
        /// <response code="200">Returns the list of topics.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Topic>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Topic>>> GetAllTopics()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(10);
            var topics = headlines.ToTopics().ToList();
            _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
            return Ok(topics);
        }

        /// <summary>
        /// Retrieves the latest breaking news topic for a debate.
        /// </summary>
        /// <returns>The most recent topic from breaking news.</returns>
        /// <response code="200">Returns the latest topic.</response>
        /// <response code="404">No current news topics available.</response>
        [HttpGet("latest")]
        [ProducesResponseType(typeof(Topic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Topic>> GetLatestTopic()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(1);
            var latestTopic = headlines.ToLatestTopic();
            
            if (latestTopic is not null)
            {
                _logger.LogInformation("Retrieved latest topic: {Title}", latestTopic.Title);
                return Ok(latestTopic);
            }
            
            _logger.LogWarning("No news headlines available for topic generation.");
            return NotFound("No current news topics available");
        }
    }
}
