using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.AI;
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
        private readonly IAzureOpenAIService _openAIService;

        public DiagnosticsService(ILogger<DiagnosticsService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, IAzureOpenAIService openAIService)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _openAIService = openAIService;
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

        public async Task<string> CheckAzureOpenAIServiceAsync()
        {
            _logger.LogInformation("Checking Azure OpenAI Service...");
            try
            {
                // Attempt a simple text generation to verify connectivity and authentication
                var testPrompt = "Say 'hello' in a rap battle style.";
                var response = await _openAIService.GenerateDebateTurnAsync(testPrompt, 10, CancellationToken.None);
                if (!string.IsNullOrEmpty(response))
                {
                    return "Azure OpenAI Service: OK";
                }
                return "Azure OpenAI Service: Error - No response from service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Azure OpenAI Service.");
                return $"Azure OpenAI Service: Error - {ex.Message}";
            }
        }

        public async Task<string> CheckTextToSpeechServiceAsync()
        {
            _logger.LogInformation("Testing Text-to-Speech service...");
            try
            {
                // Attempt a simple speech synthesis to verify connectivity and authentication
                var testText = "Hello from diagnostics page";
                var testVoice = "en-US-DavisNeural"; // A common default voice
                var audioBytes = await _openAIService.GenerateSpeechAsync(testText, testVoice, CancellationToken.None);

                if (audioBytes != null && audioBytes.Length > 0)
                {
                    return "Text-to-Speech Service: OK";
                }
                return "Text-to-Speech Service: Error - No audio generated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Text-to-Speech service test failed.");
                return $"Text-to-Speech Service: Error - {ex.Message}";
            }
        }
    }
}
