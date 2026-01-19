using Xunit;
using System.Threading.Tasks;
using PoDebateRap.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Web.Services.Speech;

namespace PoDebateRap.IntegrationTests
{
    /// <summary>
    /// Integration tests for TextToSpeechService.
    /// Uses CustomWebApplicationFactory with mocked TTS service.
    /// </summary>
    [Collection("IntegrationTests")]
    public class TextToSpeechServiceIntegrationTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public TextToSpeechServiceIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GenerateSpeechAsync_ReturnsValidWavData_WithShortText()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
            var text = "Hello, this is a test.";
            var voiceName = "en-US-JennyNeural";

            // Act
            var audioBytes = await service.GenerateSpeechAsync(text, voiceName, CancellationToken.None);

            // Assert
            Assert.NotNull(audioBytes);
            Assert.True(audioBytes.Length > 0, "Audio data should not be empty");

            // Verify WAV file header (RIFF)
            Assert.Equal(0x52, audioBytes[0]); // 'R'
            Assert.Equal(0x49, audioBytes[1]); // 'I'
            Assert.Equal(0x46, audioBytes[2]); // 'F'
            Assert.Equal(0x46, audioBytes[3]); // 'F'

            // Verify WAVE format
            Assert.Equal(0x57, audioBytes[8]);  // 'W'
            Assert.Equal(0x41, audioBytes[9]);  // 'A'
            Assert.Equal(0x56, audioBytes[10]); // 'V'
            Assert.Equal(0x45, audioBytes[11]); // 'E'

            // Audio should be at least 44 bytes (WAV header size)
            Assert.True(audioBytes.Length >= 44, "WAV file should be at least 44 bytes");
        }

        [Fact]
        public async Task GenerateSpeechAsync_SupportsDifferentVoices()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
            var text = "Testing different voices.";

            var voices = new[]
            {
                "en-US-JennyNeural",
                "en-US-GuyNeural",
                "en-US-AriaNeural"
            };

            // Act & Assert
            foreach (var voice in voices)
            {
                var audioBytes = await service.GenerateSpeechAsync(text, voice, CancellationToken.None);

                Assert.NotNull(audioBytes);
                Assert.True(audioBytes.Length > 0, $"Voice {voice} should generate audio");

                // Verify RIFF header
                Assert.Equal(0x52, audioBytes[0]); // 'R'
                Assert.Equal(0x49, audioBytes[1]); // 'I'
                Assert.Equal(0x46, audioBytes[2]); // 'F'
                Assert.Equal(0x46, audioBytes[3]); // 'F'
            }
        }
    }
}



