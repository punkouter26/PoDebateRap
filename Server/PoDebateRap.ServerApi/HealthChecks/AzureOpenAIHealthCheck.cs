using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.ServerApi.Services.AI;

namespace PoDebateRap.ServerApi.HealthChecks;

public class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly IAzureOpenAIService _openAIService;
    private readonly ILogger<AzureOpenAIHealthCheck> _logger;

    public AzureOpenAIHealthCheck(
        IAzureOpenAIService openAIService,
        ILogger<AzureOpenAIHealthCheck> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate a minimal test response to verify OpenAI connectivity
            var testResponse = await _openAIService.GenerateDebateTurnAsync(
                "Test",
                maxTokens: 10,
                cancellationToken: cancellationToken);

            if (!string.IsNullOrEmpty(testResponse))
            {
                _logger.LogInformation("Azure OpenAI health check succeeded");
                return HealthCheckResult.Healthy("Azure OpenAI service is accessible");
            }

            _logger.LogWarning("Azure OpenAI health check returned empty response");
            return HealthCheckResult.Degraded("Azure OpenAI service returned empty response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI health check failed");
            return HealthCheckResult.Unhealthy(
                "Azure OpenAI service is not accessible",
                exception: ex);
        }
    }
}
