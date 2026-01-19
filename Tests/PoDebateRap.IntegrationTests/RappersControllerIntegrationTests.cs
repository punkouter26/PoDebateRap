using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PoDebateRap.Shared.Models;
using PoDebateRap.IntegrationTests.Infrastructure;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for Rapper API endpoints.
    /// Uses CustomWebApplicationFactory with mocked dependencies.
    /// </summary>
    [Collection("IntegrationTests")]
    public class RappersControllerIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public RappersControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetRappers_ReturnsOkWithRappersList()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/rappers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rappers = await response.Content.ReadFromJsonAsync<List<Rapper>>();
            Assert.NotNull(rappers);
            Assert.NotEmpty(rappers);
        }

        [Fact]
        public async Task GetRappers_ReturnsExpectedRapperCount()
        {
            // Arrange
            using var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/rappers");
            var rappers = await response.Content.ReadFromJsonAsync<List<Rapper>>();

            // Assert
            Assert.NotNull(rappers);
            Assert.Equal(5, rappers.Count); // 5 test rappers from mock
        }
    }
}



