using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for DebateController endpoints.
    /// Tests the new RESTful endpoints and verifies backward compatibility with deprecated endpoints.
    /// </summary>
    public class DebateControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DebateControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region GET /api/Debate/current Tests

        [Fact]
        public async Task GetCurrentDebate_WhenNoDebateActive_ReturnsEmptyState()
        {
            // Act
            var response = await _client.GetAsync("/api/Debate/current");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
            state!.IsDebateInProgress.Should().BeFalse();
        }

        [Fact]
        public async Task GetCurrentDebate_ReturnsValidDebateState()
        {
            // Act
            var response = await _client.GetAsync("/api/Debate/current");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
            
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
        }

        #endregion

        #region DELETE /api/Debate/current Tests

        [Fact]
        public async Task DeleteCurrentDebate_ReturnsNoContent()
        {
            // Act
            var response = await _client.DeleteAsync("/api/Debate/current");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteCurrentDebate_ResetsDebateState()
        {
            // Arrange - First reset any existing state
            await _client.DeleteAsync("/api/Debate/current");

            // Act
            var response = await _client.GetAsync("/api/Debate/current");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
            state!.IsDebateInProgress.Should().BeFalse();
            state.CurrentTurn.Should().Be(0);
        }

        #endregion

        #region PATCH /api/Debate/current/audio-status Tests

        [Fact]
        public async Task UpdateAudioStatus_ReturnsOk()
        {
            // Act
            var response = await _client.PatchAsync("/api/Debate/current/audio-status", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Backward Compatibility Tests for Deprecated Endpoints

        [Fact]
        public async Task GetState_LegacyEndpoint_StillWorks()
        {
            // Act
            var response = await _client.GetAsync("/api/Debate/state");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
        }

        [Fact]
        public async Task Reset_LegacyEndpoint_StillWorks()
        {
            // Act
            var response = await _client.PostAsync("/api/Debate/reset", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SignalAudioComplete_LegacyEndpoint_StillWorks()
        {
            // Act
            var response = await _client.PostAsync("/api/Debate/signal-audio-complete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region POST /api/Debate (Create) Tests

        [Fact]
        public async Task CreateDebate_WithValidRequest_ReturnsDebateState()
        {
            // Arrange
            var request = new StartDebateRequest
            {
                Rapper1 = new Rapper { Name = "Test Rapper 1", RowKey = "test-rapper-1" },
                Rapper2 = new Rapper { Name = "Test Rapper 2", RowKey = "test-rapper-2" },
                Topic = new Topic { Title = "Test Topic", Category = "Test" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Debate", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
            state!.Rapper1.Name.Should().Be("Test Rapper 1");
            state.Rapper2.Name.Should().Be("Test Rapper 2");
            state.Topic.Title.Should().Be("Test Topic");

            // Cleanup
            await _client.DeleteAsync("/api/Debate/current");
        }

        [Fact]
        public async Task StartDebate_LegacyEndpoint_StillWorks()
        {
            // Arrange
            var request = new StartDebateRequest
            {
                Rapper1 = new Rapper { Name = "Legacy Rapper 1", RowKey = "legacy-rapper-1" },
                Rapper2 = new Rapper { Name = "Legacy Rapper 2", RowKey = "legacy-rapper-2" },
                Topic = new Topic { Title = "Legacy Topic", Category = "Test" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Debate/start", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var state = await response.Content.ReadFromJsonAsync<DebateState>();
            state.Should().NotBeNull();
            state!.Rapper1.Name.Should().Be("Legacy Rapper 1");

            // Cleanup
            await _client.DeleteAsync("/api/Debate/current");
        }

        #endregion
    }
}
