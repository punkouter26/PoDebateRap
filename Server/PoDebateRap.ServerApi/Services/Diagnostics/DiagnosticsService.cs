using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Services.Diagnostics
{
    /// <summary>
    /// Diagnostics service that coordinates .NET health checks and transforms results.
    /// Follows SRP by delegating actual health checking to IHealthCheck implementations.
    /// </summary>
    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly ILogger<DiagnosticsService> _logger;
        private readonly HealthCheckService _healthCheckService;

        public DiagnosticsService(
            ILogger<DiagnosticsService> logger,
            HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        public async Task<List<DiagnosticResult>> RunAllChecksAsync()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var results = healthReport.Entries.Select(entry =>
            {
                var isHealthy = entry.Value.Status == HealthStatus.Healthy;

                if (!isHealthy)
                {
                    _logger.LogWarning("Health check {CheckName} failed with status {Status}: {Description}",
                        entry.Key, entry.Value.Status, entry.Value.Description);
                }

                return new DiagnosticResult
                {
                    CheckName = entry.Key,
                    Success = isHealthy,
                    Message = isHealthy
                        ? $"{entry.Key} is healthy"
                        : $"{entry.Key} failed: {entry.Value.Description ?? entry.Value.Exception?.Message ?? "Unknown error"}"
                };
            }).ToList();

            _logger.LogInformation("Completed {Count} health checks. Healthy: {Healthy}, Unhealthy: {Unhealthy}",
                results.Count,
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }
    }
}
