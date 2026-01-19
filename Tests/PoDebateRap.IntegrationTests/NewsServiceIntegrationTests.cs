using Xunit;
using System.Threading.Tasks;
using System.Linq;
using PoDebateRap.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Web.Services.News;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for NewsService.
    /// Uses CustomWebApplicationFactory with mocked News service.
    /// </summary>
    [Collection("IntegrationTests")]
    public class NewsServiceIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public NewsServiceIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetTopHeadlinesAsync_ReturnsHeadlines_WhenApiCallIsSuccessful()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();

            // Act
            var headlines = await newsService.GetTopHeadlinesAsync(1);

            // Assert
            Assert.NotNull(headlines);
            Assert.True(headlines.Any(), "Should return at least one headline.");
        }

        [Fact]
        public async Task GetTopHeadlinesAsync_ReturnsRequestedNumberOfHeadlines()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
            var numberOfHeadlines = 2;

            // Act
            var headlines = await newsService.GetTopHeadlinesAsync(numberOfHeadlines);

            // Assert
            Assert.NotNull(headlines);
            Assert.Equal(numberOfHeadlines, headlines.Count);
        }
    }
}



