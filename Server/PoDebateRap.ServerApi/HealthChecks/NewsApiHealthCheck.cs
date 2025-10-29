using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoDebateRap.ServerApi.HealthChecks;

public class NewsApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsApiHealthCheck> _logger;

    public NewsApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<NewsApiHealthCheck> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["NewsApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("News API key not configured");
                return HealthCheckResult.Degraded("News API key not configured");
            }

            // Simple connectivity check to News API with API key
            var response = await _httpClient.GetAsync(
                $"https://newsapi.org/v2/top-headlines?country=us&pageSize=1&apiKey={apiKey}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("News API health check succeeded");
                return HealthCheckResult.Healthy("News API is accessible");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("News API health check failed with status {StatusCode}: {Content}", 
                response.StatusCode, responseContent);
            return HealthCheckResult.Degraded($"News API returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "News API health check failed");
            return HealthCheckResult.Unhealthy(
                "News API is not accessible",
                exception: ex);
        }
    }
}
