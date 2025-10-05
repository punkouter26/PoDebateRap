using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.News;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net;
using Moq.Protected; // Add this for Protected() and ItExpr

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
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"status\":\"error\",\"code\":\"apiKeyInvalid\",\"message\":\"Your API key is invalid or incorrect. Check your key, or go to https://newsapi.org to create a free API key.\"}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/") // Base address doesn't matter for mocked response
        };

        // Setup configuration to return an invalid key for this specific test
        _mockConfiguration.Setup(c => c["NewsApi:ApiKey"]).Returns("invalid-api-key");

        var newsService = new NewsService(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await newsService.GetTopHeadlinesAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Expecting one fallback headline
        // Since fallback topics are randomized, just verify we got a valid fallback topic
        var fallbackTitles = new[]
        {
            "Artificial Intelligence vs Human Creativity",
            "Social Media Impact on Society",
            "Climate Change Solutions",
            "Future of Remote Work",
            "Electric Cars vs Gas Cars",
            "Space Exploration Priorities",
            "Healthy Lifestyle Choices",
            "Education System Reform"
        };
        Assert.Contains(result[0].Title, fallbackTitles);
        Assert.StartsWith("https://example.com/", result[0].Url);
    }
}
