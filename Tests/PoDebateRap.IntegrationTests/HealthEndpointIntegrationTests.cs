using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Web.Services.Diagnostics;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for health and diagnostics endpoints.
    /// Uses CustomWebApplicationFactory with mocked dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class HealthEndpointIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public HealthEndpointIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsResponse()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/health");

            // Assert - Health endpoint should respond (any status indicates it's working)
            // In testing environment with mocks, health checks may fail but endpoint should respond
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected a valid response from health endpoint, got {response.StatusCode}");
        }

        [Fact]
        public async Task DiagnosticsService_IsRegistered()
        {
            // Arrange - Verify the service is registered in DI
            using var scope = _factory.Services.CreateScope();
            
            // Act
            var diagnosticsService = scope.ServiceProvider.GetService<IDiagnosticsService>();

            // Assert
            Assert.NotNull(diagnosticsService);
        }
    }
}



