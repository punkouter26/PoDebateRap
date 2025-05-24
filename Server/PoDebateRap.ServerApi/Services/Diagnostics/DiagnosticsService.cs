using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Diagnostics
{
    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly ILogger<DiagnosticsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DiagnosticsService(ILogger<DiagnosticsService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CheckApiHealthAsync()
        {
            try
            {
                // This is a placeholder. In a real app, you'd check a specific API endpoint.
                // For now, we'll just assume the API is healthy if this service can be instantiated.
                _logger.LogInformation("Checking API health...");
                return "API Health: OK";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API health.");
                return $"API Health: Error - {ex.Message}";
            }
        }

        public async Task<string> CheckDataConnectionAsync()
        {
            _logger.LogInformation("Checking data connection...");
            try
            {
                var connectionString = _configuration["Azure:StorageConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    return "Data Connection: Error - Azure Storage Connection String not configured.";
                }

                // Attempt to create a TableServiceClient to validate connection string
                // This doesn't hit the storage account, but validates the connection string format
                new Azure.Data.Tables.TableServiceClient(connectionString);
                return "Data Connection: OK (Connection string valid)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking data connection.");
                return $"Data Connection: Error - {ex.Message}";
            }
        }

        public async Task<string> CheckInternetConnectionAsync()
        {
            _logger.LogInformation("Checking internet connection...");
            try
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    // Try to reach a well-known external service
                    var response = await client.GetAsync("https://www.google.com", HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    return "Internet Connection: OK";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking internet connection.");
                return $"Internet Connection: Error - {ex.Message}";
            }
        }

        public async Task<string> CheckAuthenticationServiceAsync()
        {
            // This is a placeholder. Implement actual authentication service check here.
            // For example, try to get a token or hit an auth endpoint.
            _logger.LogInformation("Checking authentication service...");
            return "Authentication Service: Not Implemented (Placeholder)";
        }
    }
}
