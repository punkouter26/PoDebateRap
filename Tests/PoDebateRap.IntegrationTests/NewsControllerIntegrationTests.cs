using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for NewsController endpoints.
    /// </summary>
    public class NewsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public NewsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHeadlines_ReturnsOkWithHeadlinesList()
        {
            // Act
            var response = await _client.GetAsync("/News/headlines");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var headlines = await response.Content.ReadFromJsonAsync<List<NewsHeadline>>();
            Assert.NotNull(headlines);
        }

        [Fact]
        public async Task GetTopics_ReturnsOkWithTopicsList()
        {
            // Act
            var response = await _client.GetAsync("/News/topics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topics = await response.Content.ReadFromJsonAsync<List<Topic>>();
            Assert.NotNull(topics);
        }

        [Fact]
        public async Task GetLatestTopic_ReturnsOkOrNotFound()
        {
            // Act
            var response = await _client.GetAsync("/News/topics/latest");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");
        }
    }
}
