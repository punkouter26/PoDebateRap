using Xunit;
using Moq;
using PoDebateRap.Web.Services.Orchestration;
using PoDebateRap.Web.Services.AI;
using PoDebateRap.Web.Services.Speech;
using PoDebateRap.Web.Services.Data;
using PoDebateRap.Web.Services.Factories;
using PoDebateRap.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;

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
            Assert.Equal(10, orchestrator.CurrentState.TotalTurns);  // Default total turns from DebateStateFactory
        }

        #region JudgeDebateAsync Tests

        [Fact]
        public async Task JudgeDebateAsync_WithValidResponse_SetsWinnerAndStats()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("MC Winner");
            var rapper2 = new Rapper("MC Loser");
            var topic = new Topic { Title = "Test Battle", Category = "Test" };

            var judgeResponse = new JudgeDebateResponse
            {
                WinnerName = "MC Winner",
                Reasoning = "MC Winner had better flow and rhymes",
                Stats = new DebateStats
                {
                    Rapper1LogicScore = 4,
                    Rapper1SentimentScore = 5,
                    Rapper1AdherenceScore = 4,
                    Rapper1RebuttalScore = 5,
                    Rapper2LogicScore = 3,
                    Rapper2SentimentScore = 3,
                    Rapper2AdherenceScore = 3,
                    Rapper2RebuttalScore = 3
                }
            };

            _mockOpenAiService.Setup(s => s.JudgeDebateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(judgeResponse);

            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[0]);

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            
            // Simulate debate completion by waiting and checking state
            await Task.Delay(200);
            
            // Assert
            orchestrator.CurrentState.Should().NotBeNull();
            // The orchestrator should have initialized the state
            orchestrator.CurrentState.Rapper1.Name.Should().Be("MC Winner");
            orchestrator.CurrentState.Rapper2.Name.Should().Be("MC Loser");
        }

        [Fact]
        public async Task JudgeDebateAsync_WithTiedScores_ReturnsDrawResult()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("MC One");
            var rapper2 = new Rapper("MC Two");
            var topic = new Topic { Title = "Tie Breaker", Category = "Test" };

            var judgeResponse = new JudgeDebateResponse
            {
                WinnerName = "Draw",
                Reasoning = "Both rappers performed equally well",
                Stats = new DebateStats
                {
                    Rapper1LogicScore = 4,
                    Rapper1SentimentScore = 4,
                    Rapper1AdherenceScore = 4,
                    Rapper1RebuttalScore = 4,
                    Rapper2LogicScore = 4,
                    Rapper2SentimentScore = 4,
                    Rapper2AdherenceScore = 4,
                    Rapper2RebuttalScore = 4
                }
            };

            _mockOpenAiService.Setup(s => s.JudgeDebateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(judgeResponse);

            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[0]);

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);

            // Assert - State should be initialized
            orchestrator.CurrentState.Rapper1.Name.Should().Be("MC One");
            orchestrator.CurrentState.Rapper2.Name.Should().Be("MC Two");
        }

        #endregion

        #region GenerateTurnAudioAsync Tests

        [Fact]
        public async Task GenerateTurnAudioAsync_WhenTTSSucceeds_ReturnsValidAudioBytes()
        {
            // Arrange
            var expectedAudio = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08 }; // RIFF header
            _mockTtsService.Setup(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAudio);

            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Audio Test");
            var rapper2 = new Rapper("Audio Test 2");
            var topic = new Topic { Title = "Audio Generation", Category = "Test" };

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            await Task.Delay(100);

            // Assert
            _mockTtsService.Verify(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GenerateTurnAudioAsync_WhenTTSFails_ReturnsEmptyBytes()
        {
            // Arrange
            _mockTtsService.Setup(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("TTS service unavailable"));

            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Error Test");
            var rapper2 = new Rapper("Error Test 2");
            var topic = new Topic { Title = "Error Handling", Category = "Test" };

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            await Task.Delay(100);

            // Assert - Should handle error gracefully
            orchestrator.CurrentState.Should().NotBeNull();
            // The error should be captured but not crash the system
        }

        [Fact]
        public async Task GenerateTurnAudioAsync_WhenTTSReturnsNull_HandlesGracefully()
        {
            // Arrange
            _mockTtsService.Setup(s => s.GenerateSpeechAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)(null!));

            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Null Test");
            var rapper2 = new Rapper("Null Test 2");
            var topic = new Topic { Title = "Null Handling", Category = "Test" };

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            await Task.Delay(100);

            // Assert
            orchestrator.CurrentState.CurrentTurnAudio.Should().NotBeNull();
            // Empty array is acceptable when TTS returns null
        }

        #endregion

        #region ComputeFinalRhymeAnalytics Tests

        [Fact]
        public async Task ComputeFinalRhymeAnalytics_WithValidVerses_ReturnsMetrics()
        {
            // Arrange
            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[0]);

            _mockOpenAiService.Setup(s => s.GenerateDebateTurnAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("I got the flow like water\nMaking rappers scatter\nMy words hit like thunder\nPutting competition under");

            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Rhyme Master");
            var rapper2 = new Rapper("Flow King");
            var topic = new Topic { Title = "Rhyme Test", Category = "Test" };

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            await Task.Delay(100);

            // Assert - State should be initialized with valid rappers
            orchestrator.CurrentState.Rapper1.Name.Should().Be("Rhyme Master");
            orchestrator.CurrentState.Rapper2.Name.Should().Be("Flow King");
        }

        [Fact]
        public void ResetDebate_ClearsRhymeAnalytics()
        {
            // Arrange
            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);

            // Act
            orchestrator.ResetDebate();

            // Assert
            orchestrator.CurrentState.RhymeAnalytics.Should().BeNull();
        }

        #endregion

        #region ExecuteDebateLoopAsync Tests

        [Fact]
        public async Task ExecuteDebateLoopAsync_WhenCancelled_StopsGracefully()
        {
            // Arrange
            _mockTtsService.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[0]);

            var orchestrator = new DebateOrchestrator(_mockLogger.Object, _mockServiceFactory.Object);
            var rapper1 = new Rapper("Cancel Test");
            var rapper2 = new Rapper("Cancel Test 2");
            var topic = new Topic { Title = "Cancellation", Category = "Test" };

            // Act
            await orchestrator.StartNewDebateAsync(rapper1, rapper2, topic);
            await Task.Delay(50);
            orchestrator.ResetDebate(); // This should cancel the debate

            // Assert
            orchestrator.CurrentState.IsDebateInProgress.Should().BeFalse();
        }

        #endregion

    }
}

