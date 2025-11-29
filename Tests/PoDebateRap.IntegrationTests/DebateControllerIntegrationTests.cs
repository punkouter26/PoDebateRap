using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for DebateController endpoints.
    /// Uses WebApplicationFactory to create a test server.
    /// </summary>
    public class DebateControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DebateControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Override services with test doubles if needed
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetState_ReturnsOkWithDebateState()
        {
            // Act
            var response = await _client.GetAsync("/Debate/state");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            Assert.NotNull(state);
        }

        [Fact]
        public async Task StartDebate_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new StartDebateRequest
            {
                Rapper1 = new Rapper { Name = "Eminem", RowKey = "eminem" },
                Rapper2 = new Rapper { Name = "Snoop Dogg", RowKey = "snoop-dogg" },
                Topic = new Topic { Title = "Best Coast", Category = "Geography" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Debate/start", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            Assert.NotNull(state);
            Assert.True(state.IsDebateInProgress);
        }

        [Fact]
        public async Task StartDebate_WithMissingRapper_ReturnsBadRequest()
        {
            // Arrange
            var request = new StartDebateRequest
            {
                Rapper1 = null!,
                Rapper2 = new Rapper { Name = "Snoop Dogg", RowKey = "snoop-dogg" },
                Topic = new Topic { Title = "Test Topic", Category = "Test" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Debate/start", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ResetDebate_ReturnsOk()
        {
            // Act
            var response = await _client.PostAsync("/Debate/reset", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SignalAudioComplete_ReturnsOk()
        {
            // Act
            var response = await _client.PostAsync("/Debate/signal-audio-complete", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
