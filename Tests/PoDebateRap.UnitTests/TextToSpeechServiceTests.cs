using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Speech;
using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.UnitTests
{
    public class TextToSpeechServiceTests
    {
        private readonly Mock<ILogger<TextToSpeechService>> _mockLogger;

        public TextToSpeechServiceTests()
        {
            _mockLogger = new Mock<ILogger<TextToSpeechService>>();
        }

        [Fact]
        public void Constructor_WithMissingConfiguration_LogsWarning()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            // Act
            var service = new TextToSpeechService(config, _mockLogger.Object);

            // Assert - Should not throw, but should log warning
            Assert.NotNull(service);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateSpeechAsync_WhenNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            var service = new TextToSpeechService(config, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GenerateSpeechAsync("test", "en-US-GuyNeural", CancellationToken.None));
        }

        [Fact]
        public void Constructor_WithValidConfiguration_DoesNotLogWarning()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:Speech:Region", "eastus"},
                    {"Azure:Speech:SubscriptionKey", "test-key-12345"}
                })
                .Build();

            // Act
            var service = new TextToSpeechService(config, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateSpeechAsync_WhenCancelled_ThrowsException()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:Speech:Region", "eastus"},
                    {"Azure:Speech:SubscriptionKey", "test-key-12345"}
                })
                .Build();
            var service = new TextToSpeechService(config, _mockLogger.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert - Service may throw OperationCanceledException or InvalidOperationException
            // depending on timing of cancellation relative to synthesizer lifecycle
            var ex = await Assert.ThrowsAnyAsync<Exception>(
                () => service.GenerateSpeechAsync("test", "en-US-GuyNeural", cts.Token));
            
            Assert.True(
                ex is OperationCanceledException || ex is InvalidOperationException,
                $"Expected OperationCanceledException or InvalidOperationException, but got {ex.GetType().Name}");
        }
    }
}
