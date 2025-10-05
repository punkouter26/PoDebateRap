using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Speech;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PoDebateRap.IntegrationTests
{
    public class TextToSpeechServiceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TextToSpeechService> _logger;

        public TextToSpeechServiceIntegrationTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddUserSecrets<TextToSpeechServiceIntegrationTests>()
                .AddEnvironmentVariables()
                .Build();

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TextToSpeechService>();
        }

        [Fact]
        public async Task GenerateSpeechAsync_ReturnsValidWavData_WithShortText()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
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
            
            _logger.LogInformation("Generated {Size} bytes of audio data", audioBytes.Length);
        }

        [Fact]
        public async Task GenerateSpeechAsync_ReturnsValidWavData_WithLongerText()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
            var text = "Hello! This is a longer text to speech test from the integration tests. " +
                      "If you can hear this, the text to speech service is working correctly. " +
                      "This test validates that longer text generates proportionally larger audio files.";
            var voiceName = "en-US-GuyNeural";

            // Act
            var audioBytes = await service.GenerateSpeechAsync(text, voiceName, CancellationToken.None);

            // Assert
            Assert.NotNull(audioBytes);
            Assert.True(audioBytes.Length > 100000, "Longer text should generate larger audio file (>100KB)");
            
            // Verify RIFF header
            var header = new byte[] { audioBytes[0], audioBytes[1], audioBytes[2], audioBytes[3] };
            Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header));
            
            _logger.LogInformation("Generated {Size} bytes of audio for longer text", audioBytes.Length);
        }

        [Fact]
        public async Task GenerateSpeechAsync_SupportsDifferentVoices()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
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
                
                _logger.LogInformation("Voice {Voice} generated {Size} bytes", voice, audioBytes.Length);
            }
        }

        [Fact]
        public async Task GenerateSpeechAsync_HandlesSpecialCharacters()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
            var text = "Hello! How are you? This costs $10.50. It's 50% off!";
            var voiceName = "en-US-JennyNeural";

            // Act
            var audioBytes = await service.GenerateSpeechAsync(text, voiceName, CancellationToken.None);

            // Assert
            Assert.NotNull(audioBytes);
            Assert.True(audioBytes.Length > 0);
            
            // Verify valid WAV
            var header = System.Text.Encoding.ASCII.GetString(audioBytes.Take(4).ToArray());
            Assert.Equal("RIFF", header);
            
            _logger.LogInformation("Special characters handled, generated {Size} bytes", audioBytes.Length);
        }

        [Fact]
        public async Task GenerateSpeechAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
            var text = new string('A', 10000); // Very long text
            var voiceName = "en-US-JennyNeural";
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await service.GenerateSpeechAsync(text, voiceName, cts.Token);
            });
        }

        [Fact]
        public async Task GenerateSpeechAsync_VerifyAudioDataNotAllZeros()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
            var text = "Testing that audio data is not all zeros.";
            var voiceName = "en-US-JennyNeural";

            // Act
            var audioBytes = await service.GenerateSpeechAsync(text, voiceName, CancellationToken.None);

            // Assert
            Assert.NotNull(audioBytes);
            Assert.True(audioBytes.Length > 0);
            
            // Check that not all bytes are zero (this was the bug we fixed)
            var nonZeroCount = audioBytes.Count(b => b != 0);
            Assert.True(nonZeroCount > audioBytes.Length * 0.5, 
                $"Audio data should not be mostly zeros. Found {nonZeroCount} non-zero bytes out of {audioBytes.Length}");
            
            // Verify first 50 bytes for debugging
            var first50 = string.Join(" ", audioBytes.Take(50).Select(b => b.ToString("X2")));
            _logger.LogInformation("First 50 bytes: {Bytes}", first50);
            
            // Should NOT be all zeros or all same value
            var uniqueBytes = audioBytes.Take(50).Distinct().Count();
            Assert.True(uniqueBytes > 5, "First 50 bytes should have variety, not be uniform");
        }

        [Fact]
        public async Task GenerateSpeechAsync_ValidatesAudioFormat()
        {
            // Arrange
            var service = new TextToSpeechService(_configuration, _logger);
            var text = "Format validation test.";
            var voiceName = "en-US-JennyNeural";

            // Act
            var audioBytes = await service.GenerateSpeechAsync(text, voiceName, CancellationToken.None);

            // Assert - Verify WAV format details
            Assert.NotNull(audioBytes);
            
            // RIFF header
            Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(audioBytes.Take(4).ToArray()));
            
            // WAVE format
            Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(audioBytes.Skip(8).Take(4).ToArray()));
            
            // fmt chunk
            Assert.Equal("fmt ", System.Text.Encoding.ASCII.GetString(audioBytes.Skip(12).Take(4).ToArray()));
            
            // Expected format: 16kHz, 16-bit, mono PCM
            // Audio format (should be 1 for PCM)
            var audioFormat = BitConverter.ToInt16(audioBytes, 20);
            Assert.Equal(1, audioFormat); // PCM
            
            // Number of channels (should be 1 for mono)
            var numChannels = BitConverter.ToInt16(audioBytes, 22);
            Assert.Equal(1, numChannels);
            
            // Sample rate (should be 16000 Hz)
            var sampleRate = BitConverter.ToInt32(audioBytes, 24);
            Assert.Equal(16000, sampleRate);
            
            // Bits per sample (should be 16)
            var bitsPerSample = BitConverter.ToInt16(audioBytes, 34);
            Assert.Equal(16, bitsPerSample);
            
            _logger.LogInformation("WAV format validated: {SampleRate}Hz, {BitsPerSample}-bit, {Channels} channel(s)",
                sampleRate, bitsPerSample, numChannels);
        }
    }
}
