using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers
{
    /// <summary>
    /// Controller for retrieving news headlines and converting them to debate topics.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the latest news headlines.
        /// </summary>
        /// <returns>A list of current news headlines.</returns>
        /// <response code="200">Returns the list of headlines.</response>
        [HttpGet("headlines")]
        [ProducesResponseType(typeof(IEnumerable<NewsHeadline>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NewsHeadline>>> GetHeadlines()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(1);
            return Ok(headlines);
        }

        /// <summary>
        /// Retrieves topics derived from current news headlines.
        /// </summary>
        /// <returns>A list of topics suitable for rap debates.</returns>
        /// <response code="200">Returns the list of topics.</response>
        [HttpGet("topics")]
        [ProducesResponseType(typeof(IEnumerable<Topic>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(10);
            var topics = headlines.Select(h => new Topic { Title = h.Title ?? string.Empty, Category = "Current Events" }).ToList();
            _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
            return Ok(topics);
        }

        /// <summary>
        /// Retrieves the latest breaking news topic for a debate.
        /// </summary>
        /// <returns>The most recent topic from breaking news.</returns>
        /// <response code="200">Returns the latest topic.</response>
        /// <response code="404">No current news topics available.</response>
        [HttpGet("topics/latest")]
        [ProducesResponseType(typeof(Topic), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Topic>> GetLatestTopic()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(1);
            if (headlines.Any())
            {
                var latestTopic = new Topic { Title = headlines.First().Title ?? string.Empty, Category = "Breaking News" };
                _logger.LogInformation("Retrieved latest topic: {Title}", latestTopic.Title);
                return Ok(latestTopic);
            }
            _logger.LogWarning("No news headlines available for topic generation.");
            return NotFound("No current news topics available");
        }
    }
}
