using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.News;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace PoDebateRap.IntegrationTests
{
    public class NewsServiceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewsService> _logger;

        public NewsServiceIntegrationTests()
        {
            // Build configuration for tests, including appsettings.json
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true) // For test-specific settings
                .AddUserSecrets<NewsServiceIntegrationTests>() // For sensitive keys
                .AddEnvironmentVariables()
                .Build();

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<NewsService>();
        }

        [Fact(Skip = "Requires a valid NewsAPI key and external API call. Run manually or with specific CI setup.")]
        public async Task GetTopHeadlinesAsync_ReturnsHeadlines_WhenApiCallIsSuccessful()
        {
            // Arrange
            var httpClient = new HttpClient();
            var newsService = new NewsService(httpClient, _configuration, _logger);

            // Act
            var headlines = await newsService.GetTopHeadlinesAsync(1);

            // Assert
            Assert.NotNull(headlines);
            Assert.True(headlines.Any(), "Should return at least one headline.");
            Assert.False(headlines.Any(h => h.Title == "End of the world is coming"), "Should not return fallback headline.");
        }

        [Fact(Skip = "Requires a valid NewsAPI key and external API call. Run manually or with specific CI setup.")]
        public async Task GetTopHeadlinesAsync_ReturnsCorrectNumberOfHeadlines()
        {
            // Arrange
            var httpClient = new HttpClient();
            var newsService = new NewsService(httpClient, _configuration, _logger);
            var numberOfHeadlines = 3;

            // Act
            var headlines = await newsService.GetTopHeadlinesAsync(numberOfHeadlines);

            // Assert
            Assert.NotNull(headlines);
            Assert.Equal(numberOfHeadlines, headlines.Count());
        }
    }
}
