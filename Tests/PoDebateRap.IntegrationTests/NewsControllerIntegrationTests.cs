using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;
using PoDebateRap.IntegrationTests.Infrastructure;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for News API endpoints.
    /// Uses CustomWebApplicationFactory with mocked dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class NewsControllerIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public NewsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetHeadlines_ReturnsOkWithHeadlinesList()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/news/headlines");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var headlines = await response.Content.ReadFromJsonAsync<List<NewsHeadline>>();
            Assert.NotNull(headlines);
            Assert.NotEmpty(headlines);
        }

        [Fact]
        public async Task GetTopics_ReturnsOkWithTopicsList()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/news/topics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topics = await response.Content.ReadFromJsonAsync<List<Topic>>();
            Assert.NotNull(topics);
        }

        [Fact]
        public async Task GetLatestTopic_ReturnsOkWithTopic()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/news/topics/latest");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topic = await response.Content.ReadFromJsonAsync<Topic>();
            Assert.NotNull(topic);
        }
    }
}



