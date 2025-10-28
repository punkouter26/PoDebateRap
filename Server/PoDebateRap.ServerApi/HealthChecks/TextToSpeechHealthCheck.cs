using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.ServerApi.Services.Speech;

namespace PoDebateRap.ServerApi.HealthChecks;

public class TextToSpeechHealthCheck : IHealthCheck
{
    private readonly ITextToSpeechService _ttsService;
    private readonly ILogger<TextToSpeechHealthCheck> _logger;

    public TextToSpeechHealthCheck(
        ITextToSpeechService ttsService,
        ILogger<TextToSpeechHealthCheck> logger)
    {
        _ttsService = ttsService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate a minimal test audio to verify TTS connectivity
            var testAudio = await _ttsService.GenerateSpeechAsync(
                "Test",
                "en-US-JennyNeural",
                cancellationToken: cancellationToken);

            if (testAudio != null && testAudio.Length > 0)
            {
                _logger.LogInformation("Text-to-Speech health check succeeded");
                return HealthCheckResult.Healthy("Text-to-Speech service is accessible");
            }

            _logger.LogWarning("Text-to-Speech health check returned empty audio");
            return HealthCheckResult.Degraded("Text-to-Speech service returned empty audio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Text-to-Speech health check failed");
            return HealthCheckResult.Unhealthy(
                "Text-to-Speech service is not accessible",
                exception: ex);
        }
    }
}
