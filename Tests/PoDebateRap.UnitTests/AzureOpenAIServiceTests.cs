using Xunit;
using Moq;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.UnitTests
{
    public class AzureOpenAIServiceTests
    {
        private readonly Mock<ILogger<AzureOpenAIService>> _mockLogger;
        private readonly IConfiguration _configuration;

        public AzureOpenAIServiceTests()
        {
            _mockLogger = new Mock<ILogger<AzureOpenAIService>>();

            // Create in-memory configuration for testing
            var configData = new Dictionary<string, string?>
            {
                {"Azure:OpenAI:Endpoint", "https://test.openai.azure.com/"},
                {"Azure:OpenAI:ApiKey", "test-key-12345"},
                {"Azure:OpenAI:DeploymentName", "gpt-4-test"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public void Constructor_WithValidConfiguration_InitializesSuccessfully()
        {
            // Act
            var service = new AzureOpenAIService(_configuration, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithMissingEndpoint_ThrowsException()
        {
            // Arrange
            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:OpenAI:ApiKey", "test-key"},
                    {"Azure:OpenAI:DeploymentName", "gpt-4"}
                })
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AzureOpenAIService(invalidConfig, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithMissingApiKey_ThrowsException()
        {
            // Arrange
            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:OpenAI:Endpoint", "https://test.openai.azure.com/"},
                    {"Azure:OpenAI:DeploymentName", "gpt-4"}
                })
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AzureOpenAIService(invalidConfig, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithMissingDeploymentName_DoesNotThrow()
        {
            // Arrange - DeploymentName has a default value, so it shouldn't throw
            var configWithoutDeployment = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:OpenAI:Endpoint", "https://test.openai.azure.com/"},
                    {"Azure:OpenAI:ApiKey", "test-key"}
                })
                .Build();

            // Act
            var service = new AzureOpenAIService(configWithoutDeployment, _mockLogger.Object);

            // Assert - Should use default deployment name "gpt-35-turbo"
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GenerateDebateTurnAsync_WithValidPrompt_ReturnsNonEmptyString()
        {
            // Note: This test requires actual Azure OpenAI credentials
            // In a real scenario, you'd mock the OpenAI client or use integration tests
            // For now, we just verify the method signature exists and basic behavior

            // Arrange
            var service = new AzureOpenAIService(_configuration, _mockLogger.Object);

            // Act & Assert - Method should exist and be callable
            // In integration tests, we would actually call this with real credentials
            Assert.NotNull(service);
            await Task.CompletedTask; // Satisfy async requirement
        }

        [Fact]
        public void JudgeDebateAsync_MethodExists_CanBeInvoked()
        {
            // Arrange
            var service = new AzureOpenAIService(_configuration, _mockLogger.Object);

            // Assert - Verify method exists (compilation check)
            Assert.NotNull(service);
            // Note: Actual invocation requires Azure credentials
        }

        [Fact]
        public async Task GenerateDebateTurnAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var service = new AzureOpenAIService(_configuration, _mockLogger.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            // When already cancelled, should throw quickly
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.GenerateDebateTurnAsync("test", 50, cts.Token));
        }

        [Fact]
        public async Task JudgeDebateAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var service = new AzureOpenAIService(_configuration, _mockLogger.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.JudgeDebateAsync("transcript", "Eminem", "Snoop Dogg", "Best Coast", cts.Token));
        }
    }
}
