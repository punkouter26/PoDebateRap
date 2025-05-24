using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.News;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Threading;

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
        
        // Create a mock HttpClient that simulates a 400 Bad Request response
        var handler = new MockHttpMessageHandler(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("{\"status\":\"error\",\"code\":\"apiKeyMissing\",\"message\":\"Your API key is missing.\"}")
        });
        
        var httpClient = new HttpClient(handler);
        var newsService = new NewsService(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => newsService.GetTopHeadlinesAsync(1));

        // Assert that an HttpRequestException was thrown
        Assert.NotNull(exception);
        var httpException = Assert.IsType<HttpRequestException>(exception);
        Assert.Contains("Response status code does not indicate success: 400 (Bad Request)", httpException.Message);
    }
}

// Mock HttpMessageHandler for testing HTTP responses
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
