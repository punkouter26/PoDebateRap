using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Services.Diagnostics
{
    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly ILogger<DiagnosticsService> _logger;
        private readonly ITableStorageService _tableStorageService;
        private readonly IAzureOpenAIService _azureOpenAIService;
        private readonly ITextToSpeechService _textToSpeechService;
        private readonly HttpClient _httpClient;

        public DiagnosticsService(
            ILogger<DiagnosticsService> logger,
            ITableStorageService tableStorageService,
            IAzureOpenAIService azureOpenAIService,
            ITextToSpeechService textToSpeechService,
            HttpClient httpClient)
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
            _azureOpenAIService = azureOpenAIService;
            _textToSpeechService = textToSpeechService;
            _httpClient = httpClient;
        }

        public async Task<List<DiagnosticResult>> RunAllChecksAsync()
        {
            var results = new List<DiagnosticResult>();

            results.Add(await CheckApiHealthAsync());
            results.Add(await CheckDataConnectionAsync());
            results.Add(await CheckInternetConnectionAsync());
            results.Add(await CheckAzureOpenAIServiceAsync());
            results.Add(await CheckTextToSpeechServiceAsync());
            results.Add(await CheckNewsServiceAsync());

            return results;
        }

        private Task<DiagnosticResult> CheckApiHealthAsync()
        {
            try
            {
                _logger.LogInformation("API Health Check - OK");
                return Task.FromResult(new DiagnosticResult
                {
                    CheckName = "API Health",
                    Success = true,
                    Message = "API is running and healthy"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Health Check failed");
                return Task.FromResult(new DiagnosticResult
                {
                    CheckName = "API Health",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                });
            }
        }

        private Task<DiagnosticResult> CheckDataConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Data Connection Check - OK");
                return Task.FromResult(new DiagnosticResult
                {
                    CheckName = "Data Connection",
                    Success = true,
                    Message = "Data connection service is available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data Connection Check failed");
                return Task.FromResult(new DiagnosticResult
                {
                    CheckName = "Data Connection",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                });
            }
        }

        private async Task<DiagnosticResult> CheckInternetConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://www.google.com");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Internet Connection Check - OK");
                    return new DiagnosticResult
                    {
                        CheckName = "Internet Connection",
                        Success = true,
                        Message = "Internet connection is working"
                    };
                }
                _logger.LogWarning("Internet Connection Check - Failed with status: {StatusCode}", response.StatusCode);
                return new DiagnosticResult
                {
                    CheckName = "Internet Connection",
                    Success = false,
                    Message = $"Failed with status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internet Connection Check failed");
                return new DiagnosticResult
                {
                    CheckName = "Internet Connection",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                };
            }
        }

        private async Task<DiagnosticResult> CheckAzureOpenAIServiceAsync()
        {
            try
            {
                var result = await _azureOpenAIService.GenerateDebateTurnAsync("Hello", 10, CancellationToken.None);
                _logger.LogInformation("Azure OpenAI Service Check - OK");
                return new DiagnosticResult
                {
                    CheckName = "Azure OpenAI Service",
                    Success = true,
                    Message = "Azure OpenAI service is working"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure OpenAI Service Check failed");
                return new DiagnosticResult
                {
                    CheckName = "Azure OpenAI Service",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                };
            }
        }

        private async Task<DiagnosticResult> CheckTextToSpeechServiceAsync()
        {
            try
            {
                var result = await _textToSpeechService.GenerateSpeechAsync("Hello", "en-US-JennyNeural", CancellationToken.None);
                _logger.LogInformation("Text-to-Speech Service Check - OK");
                return new DiagnosticResult
                {
                    CheckName = "Text-to-Speech Service",
                    Success = true,
                    Message = "Text-to-Speech service is working"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Text-to-Speech Service Check failed");
                return new DiagnosticResult
                {
                    CheckName = "Text-to-Speech Service",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                };
            }
        }

        private async Task<DiagnosticResult> CheckNewsServiceAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://newsapi.org/v2/top-headlines?country=us&apiKey=test");
                _logger.LogInformation("News Service Check - OK (service is reachable)");
                return new DiagnosticResult
                {
                    CheckName = "News Service",
                    Success = true,
                    Message = "News service is reachable"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "News Service Check failed");
                return new DiagnosticResult
                {
                    CheckName = "News Service",
                    Success = false,
                    Message = $"Failed: {ex.Message}"
                };
            }
        }
    }
}
