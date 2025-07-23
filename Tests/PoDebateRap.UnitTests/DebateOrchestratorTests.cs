using Xunit;
using Moq;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace PoDebateRap.UnitTests
{
    public class DebateOrchestratorTests
    {
        private readonly Mock<ILogger<DebateOrchestrator>> _mockLogger;
        private readonly Mock<IAzureOpenAIService> _mockOpenAiService;
        private readonly Mock<ITextToSpeechService> _mockTtsService;
        private readonly IServiceCollection _serviceCollection;
        private readonly ServiceProvider _serviceProvider;

        public DebateOrchestratorTests()
        {
            _mockLogger = new Mock<ILogger<DebateOrchestrator>>();
            _mockOpenAiService = new Mock<IAzureOpenAIService>();
            _mockTtsService = new Mock<ITextToSpeechService>();

            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddScoped<ILogger<DebateOrchestrator>>(sp => _mockLogger.Object);
            _serviceCollection.AddScoped<IAzureOpenAIService>(sp => _mockOpenAiService.Object);
            _serviceCollection.AddScoped<ITextToSpeechService>(sp => _mockTtsService.Object);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task TestDebateOrchestrator_StartNewDebateAsync_InitialStateIsCorrect()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _serviceProvider);
            var rapper1 = new Rapper("Test Rapper 1");
            var rapper2 = new Rapper("Test Rapper 2");
            var topic = new Topic("Test Topic");

            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(new byte[0]);

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);

            // Assert
            Assert.True(orchestrator.CurrentState.IsDebateInProgress);
            Assert.Equal(0, orchestrator.CurrentState.CurrentTurn);
            Assert.Equal(rapper1, orchestrator.CurrentState.Rapper1);
            Assert.Equal(rapper2, orchestrator.CurrentState.Rapper2);
            Assert.Equal(topic, orchestrator.CurrentState.Topic);
        }

    }
}
