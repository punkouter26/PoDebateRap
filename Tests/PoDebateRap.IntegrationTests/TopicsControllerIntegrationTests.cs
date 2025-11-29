using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for TopicsController endpoints.
    /// </summary>
    public class TopicsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TopicsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTopics_ReturnsOkWithTopicsList()
        {
            // Act
            var response = await _client.GetAsync("/Topics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topics = await response.Content.ReadFromJsonAsync<List<Topic>>();
            Assert.NotNull(topics);
        }

        [Fact]
        public async Task GetLatestTopic_ReturnsOkOrNotFound()
        {
            // Act
            var response = await _client.GetAsync("/Topics/latest");

            // Assert - Either we get a topic or there are none available
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");
        }
    }
}
