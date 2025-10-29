using Xunit;
using Moq;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.Factories;
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
        private readonly Mock<IRapperRepository> _mockRapperRepository;
        private readonly Mock<IDebateServiceFactory> _mockServiceFactory;
        private readonly Mock<IDebateServiceScope> _mockServiceScope;

        public DebateOrchestratorTests()
        {
            _mockLogger = new Mock<ILogger<DebateOrchestrator>>();
            _mockOpenAiService = new Mock<IAzureOpenAIService>();
            _mockTtsService = new Mock<ITextToSpeechService>();
            _mockRapperRepository = new Mock<IRapperRepository>();
            _mockServiceScope = new Mock<IDebateServiceScope>();
            _mockServiceFactory = new Mock<IDebateServiceFactory>();

            // Configure mock scope to return mock services
            _mockServiceScope.Setup(s => s.AIService).Returns(_mockOpenAiService.Object);
            _mockServiceScope.Setup(s => s.TTSService).Returns(_mockTtsService.Object);
            _mockServiceScope.Setup(s => s.RapperRepository).Returns(_mockRapperRepository.Object);

            // Configure factory to return mock scope
            _mockServiceFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);
        }

        [Fact]
        public async Task TestDebateOrchestrator_StartNewDebateAsync_InitialStateIsCorrect()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Test Rapper 1");
            var rapper2 = new Rapper("Test Rapper 2");
            var topic = new Topic { Title = "Test Topic", Category = "Test" };

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

        [Fact]
        public void ResetDebate_ClearsState()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);

            // Act
            orchestrator.ResetDebate();

            // Assert
            Assert.False(orchestrator.CurrentState.IsDebateInProgress);
            Assert.Equal(0, orchestrator.CurrentState.CurrentTurn);
            Assert.Empty(orchestrator.CurrentState.WinnerName);
            Assert.Empty(orchestrator.CurrentState.JudgeReasoning);
        }

        [Fact]
        public async Task SignalAudioPlaybackCompleteAsync_CompletesSuccessfully()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);

            // Act
            await orchestrator.SignalAudioPlaybackCompleteAsync();

            // Assert - No exception thrown means success
            Assert.True(true);
        }

        [Fact]
        public async Task StartNewDebateAsync_GeneratesIntroductionAudio()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper { Name = "MC Test", RowKey = "mc-test" };
            var rapper2 = new Rapper { Name = "DJ Mock", RowKey = "dj-mock" };
            var topic = new Topic { Title = "Unit Testing", Category = "Tech" };

            _mockTtsService.Setup(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3, 4 });

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);

            // Small delay to allow background task to start
            await Task.Delay(100);

            // Assert - Verify TTS was called at least once for introduction
            _mockTtsService.Verify(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<System.Threading.CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task StartNewDebateAsync_InitializesStateWithCorrectRappers()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Eminem");
            var rapper2 = new Rapper("Snoop Dogg");
            var topic = new Topic { Title = "Best Coast", Category = "Geography" };

            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new byte[0]);

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);

            // Assert
            Assert.Equal("Eminem", orchestrator.CurrentState.Rapper1.Name);
            Assert.Equal("Snoop Dogg", orchestrator.CurrentState.Rapper2.Name);
            Assert.True(orchestrator.CurrentState.IsRapper1Turn);
            Assert.Equal(10, orchestrator.CurrentState.TotalTurns);
        }

    }
}
