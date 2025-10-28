using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopicsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<TopicsController> _logger;

        public TopicsController(INewsService newsService, ILogger<TopicsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicDto>>> Get()
        {
            try
            {
                var headlines = await _newsService.GetTopHeadlinesAsync(10);
                var topics = headlines.Select(h => new TopicDto { Title = h.Title ?? string.Empty, Category = "Current Events" }).ToList();
                _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topics from latest news.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("latest")]
        public async Task<ActionResult<TopicDto>> GetLatest()
        {
            try
            {
                var headlines = await _newsService.GetTopHeadlinesAsync(1);
                if (headlines.Any())
                {
                    var latestTopic = new TopicDto { Title = headlines.First().Title ?? string.Empty, Category = "Breaking News" };
                    _logger.LogInformation("Retrieved latest topic: {Title}", latestTopic.Title);
                    return Ok(latestTopic);
                }
                _logger.LogWarning("No news headlines available for topic generation.");
                return NotFound("No current news topics available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest topic from news.");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public record TopicDto
    {
        public string Title { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }
}
