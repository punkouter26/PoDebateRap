using Xunit;
using System.Threading.Tasks;
using PoDebateRap.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Web.Services.AI;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for AzureOpenAIService (verse generation only).
    /// Uses CustomWebApplicationFactory with mocked OpenAI service.
    /// Speech synthesis tests are in TextToSpeechServiceIntegrationTests.
    /// </summary>
    [Collection("IntegrationTests")]
    public class AzureOpenAIServiceIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public AzureOpenAIServiceIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GenerateDebateTurnAsync_ReturnsResponse_WhenApiCallIsSuccessful()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
            var prompt = "Test prompt for debate turn generation.";
            var maxTokens = 50;

            // Act
            var response = await service.GenerateDebateTurnAsync(prompt, maxTokens, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrEmpty(response));
            Assert.True(response.Length > 0);
        }

        [Fact]
        public async Task JudgeDebateAsync_ReturnsJudgement()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
            var debateTranscript = "Rapper1 argued about technology, Rapper2 countered with politics.";
            var rapper1Name = "Eminem";
            var rapper2Name = "Snoop Dogg";
            var topic = "Technology vs Nature";

            // Act
            var result = await service.JudgeDebateAsync(debateTranscript, rapper1Name, rapper2Name, topic, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WinnerName);
            Assert.NotNull(result.Reasoning);
        }
    }
}



