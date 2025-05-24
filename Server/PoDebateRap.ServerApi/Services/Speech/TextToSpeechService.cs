using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO; // Add for MemoryStream and CopyToAsync
using System.Text; // Add for Encoding
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Speech
{
    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly SpeechConfig _speechConfig;
        private readonly ILogger<TextToSpeechService> _logger;

        public TextToSpeechService(IConfiguration configuration, ILogger<TextToSpeechService> logger)
        {
            _logger = logger;

            var speechRegion = configuration["Azure:Speech:Region"];
            var speechSubscriptionKey = configuration["Azure:Speech:SubscriptionKey"];

            if (string.IsNullOrEmpty(speechRegion) || string.IsNullOrEmpty(speechSubscriptionKey))
            {
                _logger.LogWarning("Azure Speech region or subscription key is not configured. Text-to-Speech will be unavailable.");
                _speechConfig = null; // Indicate TTS is not configured
            }
            else
            {
                _speechConfig = SpeechConfig.FromSubscription(speechSubscriptionKey, speechRegion);
                _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm); // Standard WAV format
                _logger.LogInformation("Azure Speech client initialized for region: {Region}", speechRegion);
            }
        }

        public async Task<byte[]> GenerateSpeechAsync(string text, string voiceName)
        {
            if (_speechConfig == null)
            {
                _logger.LogError("Azure Speech service is not configured. Cannot generate speech.");
                throw new InvalidOperationException("Azure Speech service is not configured.");
            }

            _logger.LogInformation("Generating speech for text: '{Text}' with voice: {Voice}", text, voiceName);
            try
            {
                // Set the voice on the SpeechConfig directly
                _speechConfig.SpeechSynthesisVoiceName = voiceName;

                using (var synthesizer = new SpeechSynthesizer(_speechConfig, null))
                {
                    using (var result = await synthesizer.SpeakTextAsync(text))
                    {
                        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                        {
                            _logger.LogInformation("Speech synthesis completed successfully.");
                            using (var audioDataStream = AudioDataStream.FromResult(result))
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    byte[] buffer = new byte[16000]; // Buffer for reading audio data
                                    uint bytesRead;
                                    while ((bytesRead = audioDataStream.ReadData(buffer)) > 0)
                                    {
                                        memoryStream.Write(buffer, 0, (int)bytesRead);
                                    }

                                    byte[] audioData = memoryStream.ToArray();

                                    // Construct WAV header
                                    int sampleRate = 16000; // From SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm
                                    int bitsPerSample = 16; // From SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm
                                    int numChannels = 1; // Mono

                                    int byteRate = sampleRate * numChannels * (bitsPerSample / 8);
                                    short blockAlign = (short)(numChannels * (bitsPerSample / 8));

                                    int audioDataLength = audioData.Length;
                                    int chunkSize = 36 + audioDataLength; // 36 bytes for header + audio data length

                                    using (var wavStream = new MemoryStream())
                                    using (var writer = new BinaryWriter(wavStream))
                                    {
                                        // RIFF header
                                        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                                        writer.Write(chunkSize);
                                        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                                        // fmt chunk
                                        writer.Write(Encoding.ASCII.GetBytes("fmt "));
                                        writer.Write(16); // Subchunk1Size for PCM
                                        writer.Write((short)1); // AudioFormat (1 for PCM)
                                        writer.Write((short)numChannels);
                                        writer.Write(sampleRate);
                                        writer.Write(byteRate);
                                        writer.Write(blockAlign);
                                        writer.Write((short)bitsPerSample);

                                        // data chunk
                                        writer.Write(Encoding.ASCII.GetBytes("data"));
                                        writer.Write(audioDataLength);
                                        writer.Write(audioData);

                                        var wavBytes = wavStream.ToArray();

                                        // Save the WAV file to the root directory for verification
                                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "test_speech.wav");
                                        await File.WriteAllBytesAsync(filePath, wavBytes);
                                        _logger.LogInformation("Saved generated speech to {FilePath}", filePath);

                                        return wavBytes;
                                    }
                                }
                            }
                        }
                        else if (result.Reason == ResultReason.Canceled)
                        {
                            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                            _logger.LogError("Speech synthesis canceled: Reason={Reason}, ErrorDetails={ErrorDetails}",
                                cancellation.Reason, cancellation.ErrorDetails);
                            throw new Exception($"Speech synthesis canceled: {cancellation.Reason} - {cancellation.ErrorDetails}");
                        }
                        else
                        {
                            _logger.LogError("Speech synthesis failed: Reason={Reason}", result.Reason);
                            throw new Exception($"Speech synthesis failed: {result.Reason}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during speech synthesis.");
                throw;
            }
        }
    }
}
