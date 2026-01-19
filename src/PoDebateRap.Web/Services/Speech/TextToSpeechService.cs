using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO; // Add for MemoryStream and CopyToAsync
using System.Text; // Add for Encoding
using System.Threading.Tasks;

namespace PoDebateRap.Web.Services.Speech
{
    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly SpeechConfig? _speechConfig;
        private readonly ILogger<TextToSpeechService> _logger;

        /// <summary>
        /// Indicates whether the Text-to-Speech service is properly configured.
        /// </summary>
        public bool IsConfigured { get; }

        public TextToSpeechService(IConfiguration configuration, ILogger<TextToSpeechService> logger)
        {
            _logger = logger;

            var speechRegion = configuration["Azure:Speech:Region"];
            var speechSubscriptionKey = configuration["Azure:Speech:SubscriptionKey"];

            if (string.IsNullOrEmpty(speechRegion) || string.IsNullOrEmpty(speechSubscriptionKey))
            {
                _logger.LogWarning("Azure Speech region or subscription key is not configured. Text-to-Speech will be unavailable.");
                _speechConfig = null;
                IsConfigured = false;
            }
            else
            {
                _speechConfig = SpeechConfig.FromSubscription(speechSubscriptionKey, speechRegion);
                _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm); // Standard WAV format
                IsConfigured = true;
                _logger.LogInformation("Azure Speech client initialized for region: {Region}", speechRegion);
            }
        }

        public async Task<byte[]> GenerateSpeechAsync(string text, string voiceName, CancellationToken cancellationToken)
        {
            if (_speechConfig == null)
            {
                _logger.LogError("Azure Speech service is not configured. Cannot generate speech.");
                throw new InvalidOperationException("Azure Speech service is not configured.");
            }

            _logger.LogInformation("Generating speech for text: '{Text}' with voice: {Voice}", text, voiceName);
            try
            {
                _speechConfig.SpeechSynthesisVoiceName = voiceName;

                using (var synthesizer = new SpeechSynthesizer(_speechConfig, null))
                {
                    // This is a workaround to make SpeakTextAsync cancellable.
                    // The SDK's SpeakTextAsync doesn't directly accept a CancellationToken.
                    // We start the synthesis and then wait on a task that can be cancelled.
                    var synthesisTask = synthesizer.SpeakTextAsync(text);
                    var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);

                    var completedTask = await Task.WhenAny(synthesisTask, cancellationTask);

                    if (completedTask == cancellationTask)
                    {
                        // The cancellation token was triggered.
                        // We don't have a direct way to stop the synthesizer, but we can stop processing.
                        _logger.LogInformation("Speech synthesis was canceled.");
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    // The synthesis task completed.
                    using (var result = await synthesisTask)
                    {
                        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                        {
                            _logger.LogInformation("Speech synthesis completed successfully.");

                            // Use result.AudioData directly instead of AudioDataStream
                            // This avoids potential issues with stream reading
                            var audioBytes = result.AudioData;

                            if (audioBytes != null && audioBytes.Length > 0)
                            {
                                _logger.LogInformation("Retrieved {Size} bytes of audio data from result.AudioData", audioBytes.Length);
                                return audioBytes;
                            }
                            else
                            {
                                _logger.LogError("AudioData is null or empty despite successful synthesis");
                                throw new Exception("No audio data was generated");
                            }
                        }
                        else if (result.Reason == ResultReason.Canceled)
                        {
                            var cancellationDetails = SpeechSynthesisCancellationDetails.FromResult(result);
                            _logger.LogError("Speech synthesis canceled by service: Reason={Reason}, ErrorDetails={ErrorDetails}",
                                cancellationDetails.Reason, cancellationDetails.ErrorDetails);
                            throw new Exception($"Speech synthesis canceled: {cancellationDetails.Reason} - {cancellationDetails.ErrorDetails}");
                        }
                        else
                        {
                            _logger.LogError("Speech synthesis failed: Reason={Reason}", result.Reason);
                            throw new Exception($"Speech synthesis failed: {result.Reason}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Speech generation was canceled by the caller.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during speech synthesis.");
                throw;
            }
        }
    }
}
