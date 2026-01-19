using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.Web.Services.Data;

namespace PoDebateRap.Web.HealthChecks;

public class AzureTableStorageHealthCheck : IHealthCheck
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<AzureTableStorageHealthCheck> _logger;

    public AzureTableStorageHealthCheck(
        ITableStorageService tableStorageService,
        ILogger<AzureTableStorageHealthCheck> logger)
    {
        _tableStorageService = tableStorageService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get a table client to verify table storage connectivity
            var tableClient = await _tableStorageService.GetTableClientAsync("Rappers");

            // Try to query for entities (take first one) to verify table is accessible
            await foreach (var entity in _tableStorageService.GetEntitiesAsync<Azure.Data.Tables.TableEntity>("Rappers"))
            {
                // Just verify we can read at least one entity
                break;
            }

            _logger.LogInformation("Azure Table Storage health check succeeded");
            return HealthCheckResult.Healthy("Azure Table Storage is accessible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Table Storage health check failed");
            return HealthCheckResult.Unhealthy(
                "Azure Table Storage is not accessible",
                exception: ex);
        }
    }
}
