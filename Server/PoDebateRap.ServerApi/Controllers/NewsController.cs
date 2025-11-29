using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.ServerApi.Extensions;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        [HttpGet("headlines")]
        public async Task<ActionResult<IEnumerable<NewsHeadline>>> GetHeadlines()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(1);
            return Ok(headlines);
        }

        /// <summary>
        /// Gets topics from news headlines.
        /// Note: Consider using TopicsController.Get() instead - this endpoint is kept for backwards compatibility.
        /// </summary>
        [HttpGet("topics")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(10);
            var topics = headlines.ToTopics().ToList();
            _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
            return Ok(topics);
        }

        /// <summary>
        /// Gets the latest topic from news headlines.
        /// Note: Consider using TopicsController.GetLatest() instead - this endpoint is kept for backwards compatibility.
        /// </summary>
        [HttpGet("topics/latest")]
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
