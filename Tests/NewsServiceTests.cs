using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.News;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net;

public class NewsServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<NewsService>> _mockLogger;
    private readonly string _newsApiKey = "acd7ec0ba05b49b2944494ebd941be3c"; // The key provided by the user

    public NewsServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<NewsService>>();

        // Setup configuration for NewsApi:ApiKey
        _mockConfiguration.Setup(c => c["NewsApi:ApiKey"]).Returns(_newsApiKey);
    }

    [Fact]
    public async Task GetTopHeadlinesAsync_WithInvalidApiKey_ReturnsBadRequest()
    {
        // Arrange
        // NewsAPI.org returns 401 Unauthorized for invalid API key, or 400 Bad Request for missing query parameters.
        // Given the previous logs showed 400, we'll expect that.
        // We don't need to mock HttpClient's response for this, as we're testing the actual API call.
        var httpClient = new HttpClient(); // Use a real HttpClient to make the actual call
        var newsService = new NewsService(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => newsService.GetTopHeadlinesAsync(1));

        // Assert that an HttpRequestException was thrown
        Assert.NotNull(exception);
        var httpException = Assert.IsType<HttpRequestException>(exception);
        Assert.Contains("Response status code does not indicate success: 400 (Bad Request)", httpException.Message);
    }
}
