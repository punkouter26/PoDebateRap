using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            try
            {
                // Get latest news headlines and convert them to debate topics
                var headlines = await _newsService.GetTopHeadlinesAsync(10); // Get top 10 latest news
                
                var topics = headlines.Select(headline => new Topic 
                { 
                    Title = headline.Title,
                    Category = "Current Events" // Since these are from news
                }).ToList();

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
        public async Task<ActionResult<Topic>> GetLatest()
        {
            try
            {
                // Get the most recent news headline as the debate topic
                var headlines = await _newsService.GetTopHeadlinesAsync(1); // Get only the latest
                
                if (headlines.Any())
                {
                    var latestTopic = new Topic 
                    { 
                        Title = headlines.First().Title,
                        Category = "Breaking News"
                    };

                    _logger.LogInformation("Retrieved latest topic: {Title}", latestTopic.Title);
                    return Ok(latestTopic);
                }
                else
                {
                    _logger.LogWarning("No news headlines available for topic generation.");
                    return NotFound("No current news topics available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest topic from news.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
