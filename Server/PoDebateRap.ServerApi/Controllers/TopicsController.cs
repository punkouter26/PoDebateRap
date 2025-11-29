using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.ServerApi.Extensions;
using PoDebateRap.Shared.Models;
using System.Linq;

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
        public async Task<ActionResult<IEnumerable<Topic>>> Get()
        {
            var headlines = await _newsService.GetTopHeadlinesAsync(10);
            var topics = headlines.ToTopics().ToList();
            _logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
            return Ok(topics);
        }

        [HttpGet("latest")]
        public async Task<ActionResult<Topic>> GetLatest()
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
