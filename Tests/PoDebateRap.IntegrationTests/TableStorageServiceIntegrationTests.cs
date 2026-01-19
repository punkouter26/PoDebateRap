using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.IntegrationTests.Infrastructure;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for Table Storage through API endpoints.
    /// Uses CustomWebApplicationFactory with mocked Table Storage service.
    /// Tests the data layer through the rappers endpoint.
    /// </summary>
    [Collection("IntegrationTests")]
    public class TableStorageServiceIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public TableStorageServiceIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task RappersEndpoint_ReturnsRappers_FromMockedRepository()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act - Test data layer through API
            var response = await client.GetAsync("/api/rappers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rappers = await response.Content.ReadFromJsonAsync<List<Rapper>>();
            Assert.NotNull(rappers);
            Assert.NotEmpty(rappers);
        }

        [Fact]
        public async Task RappersEndpoint_ReturnsExpectedCount_FromMockedRepository()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/rappers");
            var rappers = await response.Content.ReadFromJsonAsync<List<Rapper>>();

            // Assert - Should match mock data (5 test rappers)
            Assert.NotNull(rappers);
            Assert.Equal(5, rappers.Count);
        }
    }
}



