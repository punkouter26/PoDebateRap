using Xunit;
using Moq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using PoDebateRap.Web.Services.Telemetry;
using System;

namespace PoDebateRap.UnitTests
{
    public class CustomTelemetryServiceTests
    {
        private readonly Mock<ILogger<CustomTelemetryService>> _mockLogger;
        private readonly TelemetryClient _telemetryClient;
        private readonly Mock<ITelemetryChannel> _mockChannel;

        public CustomTelemetryServiceTests()
        {
            _mockLogger = new Mock<ILogger<CustomTelemetryService>>();
            _mockChannel = new Mock<ITelemetryChannel>();
            
            var config = new TelemetryConfiguration
            {
                TelemetryChannel = _mockChannel.Object,
                ConnectionString = "InstrumentationKey=test-key"
            };
            _telemetryClient = new TelemetryClient(config);
        }

        [Fact]
        public void TrackDebateStarted_LogsCorrectInformation()
        {
            // Arrange
            var service = new CustomTelemetryService(_telemetryClient, _mockLogger.Object);

            // Act
            service.TrackDebateStarted("Eminem", "Snoop Dogg", "Best Coast");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Eminem") && v.ToString()!.Contains("Snoop Dogg")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TrackDebateCompleted_LogsWinnerAndDuration()
        {
            // Arrange
            var service = new CustomTelemetryService(_telemetryClient, _mockLogger.Object);
            var duration = TimeSpan.FromMinutes(5);

            // Act
            service.TrackDebateCompleted("Eminem", "Snoop Dogg", "Eminem", 10, duration);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Eminem") && 
                        v.ToString()!.Contains("10")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TrackAIModelUsage_LogsTokenCountAndResponseTime()
        {
            // Arrange
            var service = new CustomTelemetryService(_telemetryClient, _mockLogger.Object);
            var responseTime = TimeSpan.FromMilliseconds(500);

            // Act
            service.TrackAIModelUsage("gpt-4", "GenerateDebateTurn", 150, responseTime);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("gpt-4") && 
                        v.ToString()!.Contains("150")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TrackTextToSpeechUsage_LogsVoiceAndCharacterCount()
        {
            // Arrange
            var service = new CustomTelemetryService(_telemetryClient, _mockLogger.Object);
            var generationTime = TimeSpan.FromMilliseconds(1200);

            // Act
            service.TrackTextToSpeechUsage("en-US-GuyNeural", 500, generationTime);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("en-US-GuyNeural") && 
                        v.ToString()!.Contains("500")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_WithValidDependencies_InitializesSuccessfully()
        {
            // Act
            var service = new CustomTelemetryService(_telemetryClient, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }
    }
}
