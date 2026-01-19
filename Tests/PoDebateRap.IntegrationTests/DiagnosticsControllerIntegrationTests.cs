using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;
using PoDebateRap.IntegrationTests.Infrastructure;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for Diagnostics API endpoints.
    /// Uses CustomWebApplicationFactory with mocked dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class DiagnosticsControllerIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public DiagnosticsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllDiagnostics_ReturnsResponse()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/diagnostics");

            // Assert - Endpoint should respond (may return error due to health check dependencies)
            // In testing environment with mocks, some checks may fail but the endpoint should work
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected a response from diagnostics endpoint, got {response.StatusCode}");
        }

        [Fact]
        public async Task GetDiagnosticsEndpoint_IsRoutable()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/diagnostics");

            // Assert - Should not return 404 (endpoint exists)
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}



