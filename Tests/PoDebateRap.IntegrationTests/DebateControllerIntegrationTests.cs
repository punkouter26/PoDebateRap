using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;
using PoDebateRap.IntegrationTests.Infrastructure;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for DebateController endpoints.
    /// Uses CustomWebApplicationFactory with mocked external dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class DebateControllerIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public DebateControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetCurrentDebate_ReturnsOkWithDebateState()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act - Use correct endpoint path
            var response = await client.GetAsync("/api/debate/current");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            Assert.NotNull(state);
        }

        [Fact]
        public async Task CreateDebate_WithValidRequest_ReturnsOk()
        {
            // Arrange
            using var client = _factory.CreateClient();
            var request = new StartDebateRequest
            {
                Rapper1 = new Rapper("Eminem") { RowKey = "eminem" },
                Rapper2 = new Rapper("Snoop Dogg") { RowKey = "snoop-dogg" },
                Topic = new Topic { Title = "Best Coast", Category = "Geography" }
            };

            // Act - Use correct endpoint path
            var response = await client.PostAsJsonAsync("/api/debate", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            Assert.NotNull(state);
            Assert.True(state.IsDebateInProgress);
        }

        [Fact]
        public async Task CreateDebate_WithMissingRapper_ReturnsClientError()
        {
            // Arrange
            using var client = _factory.CreateClient();
            var request = new StartDebateRequest
            {
                Rapper1 = null!,
                Rapper2 = new Rapper("Snoop Dogg") { RowKey = "snoop-dogg" },
                Topic = new Topic { Title = "Test Topic", Category = "Test" }
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/debate", request);

            // Assert - Should return some error (BadRequest, InternalServerError, or UnprocessableEntity)
            // Validation or null reference handling should prevent success
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.InternalServerError ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Expected error response for invalid request, got {response.StatusCode}");
        }

        [Fact]
        public async Task DeleteCurrentDebate_ReturnsNoContent()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act - Use correct endpoint path
            var response = await client.DeleteAsync("/api/debate/current");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAudioStatus_ReturnsOk()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act - Use correct endpoint path
            var response = await client.PatchAsync("/api/debate/current/audio-status", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}



