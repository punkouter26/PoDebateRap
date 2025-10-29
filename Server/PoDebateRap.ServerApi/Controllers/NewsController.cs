using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
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

        [HttpGet("topics")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(10);
            var topics = headlines.Select(h => new Topic { Title = h.Title ?? string.Empty, Category = "Current Events" }).ToList();
            _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
            return Ok(topics);
        }

        [HttpGet("topics/latest")]
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
