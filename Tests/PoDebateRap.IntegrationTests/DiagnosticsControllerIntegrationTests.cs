using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for DiagnosticsController endpoints.
    /// </summary>
    public class DiagnosticsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DiagnosticsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllDiagnostics_ReturnsOkWithDiagnosticResults()
        {
            // Act
            var response = await _client.GetAsync("/Diagnostics/all");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = await response.Content.ReadFromJsonAsync<List<DiagnosticResult>>();
            Assert.NotNull(results);
        }

        [Fact]
        public async Task GetAllDiagnostics_ReturnsMultipleChecks()
        {
            // Act
            var response = await _client.GetAsync("/Diagnostics/all");
            var results = await response.Content.ReadFromJsonAsync<List<DiagnosticResult>>();

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Count >= 1, "Expected at least one diagnostic check");
        }
    }
}
