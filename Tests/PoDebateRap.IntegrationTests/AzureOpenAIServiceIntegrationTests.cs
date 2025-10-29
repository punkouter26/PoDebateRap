using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.AI;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for AzureOpenAIService (verse generation only).
    /// Speech synthesis tests are in TextToSpeechServiceIntegrationTests.
    /// </summary>
    public class AzureOpenAIServiceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIServiceIntegrationTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddUserSecrets<AzureOpenAIServiceIntegrationTests>()
                .AddEnvironmentVariables()
                .Build();

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AzureOpenAIService>();
        }

        [Fact]
        public async Task GenerateDebateTurnAsync_ReturnsResponse_WhenApiCallIsSuccessful()
        {
            // Arrange
            var service = new AzureOpenAIService(_configuration, _logger);
            var prompt = "Test prompt for debate turn generation.";
            var maxTokens = 50;

            // Act
            var response = await service.GenerateDebateTurnAsync(prompt, maxTokens, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrEmpty(response));
            Assert.True(response.Length > 0);
        }
    }
}
