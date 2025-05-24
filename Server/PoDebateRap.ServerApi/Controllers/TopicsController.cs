using Microsoft.AspNetCore.Mvc;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicRepository _topicRepository;
        private readonly ILogger<TopicsController> _logger;

        public TopicsController(ITopicRepository topicRepository, ILogger<TopicsController> logger)
        {
            _topicRepository = topicRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topic>>> Get()
        {
            try
            {
                var topics = await _topicRepository.GetAllTopicsAsync();
                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all topics.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
