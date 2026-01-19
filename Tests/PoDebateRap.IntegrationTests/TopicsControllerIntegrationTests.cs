using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;
using PoDebateRap.IntegrationTests.Infrastructure;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for Topics API endpoints.
    /// Uses CustomWebApplicationFactory with mocked dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class TopicsControllerIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public TopicsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetTopics_ReturnsOkWithTopicsList()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/topics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topics = await response.Content.ReadFromJsonAsync<List<Topic>>();
            Assert.NotNull(topics);
            Assert.NotEmpty(topics);
        }

        [Fact]
        public async Task GetLatestTopic_ReturnsOkWithTopic()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/topics/latest");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topic = await response.Content.ReadFromJsonAsync<Topic>();
            Assert.NotNull(topic);
            Assert.NotNull(topic.Title);
        }
    }
}



