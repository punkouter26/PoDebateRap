using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            try
            {
                var headlines = await _newsService.GetTopHeadlinesAsync(1); // Call with count
                return Ok(headlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news headlines.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
