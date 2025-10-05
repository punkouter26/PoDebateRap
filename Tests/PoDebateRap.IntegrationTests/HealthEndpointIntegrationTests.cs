using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using System.Threading.Tasks;
using System.Net.Http;

namespace PoDebateRap.IntegrationTests
{
    public class HealthEndpointIntegrationTests
    {
        private readonly IConfiguration _configuration;

        public HealthEndpointIntegrationTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets<HealthEndpointIntegrationTests>()
                .AddEnvironmentVariables()
                .Build();
        }

        [Fact]
        public async Task DiagnosticsService_CheckApiHealth_ReturnsSuccess()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DiagnosticsService>();
            var tableLogger = loggerFactory.CreateLogger<TableStorageService>();
            var aiLogger = loggerFactory.CreateLogger<AzureOpenAIService>();
            var ttsLogger = loggerFactory.CreateLogger<TextToSpeechService>();

            var httpClient = new HttpClient();
            var tableStorageService = new TableStorageService(_configuration, tableLogger);
            var azureOpenAIService = new AzureOpenAIService(_configuration, aiLogger);
            var textToSpeechService = new TextToSpeechService(_configuration, ttsLogger);

            var diagnosticsService = new DiagnosticsService(
                logger,
                tableStorageService,
                azureOpenAIService,
                textToSpeechService,
                httpClient);

            // Act
            var result = await diagnosticsService.CheckApiHealthAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("healthy", result.ToLower());
        }

        [Fact]
        public async Task DiagnosticsService_CheckInternetConnection_ReturnsSuccess()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DiagnosticsService>();
            var tableLogger = loggerFactory.CreateLogger<TableStorageService>();
            var aiLogger = loggerFactory.CreateLogger<AzureOpenAIService>();
            var ttsLogger = loggerFactory.CreateLogger<TextToSpeechService>();

            var httpClient = new HttpClient();
            var tableStorageService = new TableStorageService(_configuration, tableLogger);
            var azureOpenAIService = new AzureOpenAIService(_configuration, aiLogger);
            var textToSpeechService = new TextToSpeechService(_configuration, ttsLogger);

            var diagnosticsService = new DiagnosticsService(
                logger,
                tableStorageService,
                azureOpenAIService,
                textToSpeechService,
                httpClient);

            // Act
            var result = await diagnosticsService.CheckInternetConnectionAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("working", result.ToLower());
        }

        [Fact]
        public async Task DiagnosticsService_CheckDataConnection_ReturnsSuccess()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DiagnosticsService>();
            var tableLogger = loggerFactory.CreateLogger<TableStorageService>();
            var aiLogger = loggerFactory.CreateLogger<AzureOpenAIService>();
            var ttsLogger = loggerFactory.CreateLogger<TextToSpeechService>();

            var httpClient = new HttpClient();
            var tableStorageService = new TableStorageService(_configuration, tableLogger);
            var azureOpenAIService = new AzureOpenAIService(_configuration, aiLogger);
            var textToSpeechService = new TextToSpeechService(_configuration, ttsLogger);

            var diagnosticsService = new DiagnosticsService(
                logger,
                tableStorageService,
                azureOpenAIService,
                textToSpeechService,
                httpClient);

            // Act
            var result = await diagnosticsService.CheckDataConnectionAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("available", result.ToLower());
        }

        [Fact]
        public async Task DiagnosticsService_CheckAzureOpenAI_ReturnsSuccess()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DiagnosticsService>();
            var tableLogger = loggerFactory.CreateLogger<TableStorageService>();
            var aiLogger = loggerFactory.CreateLogger<AzureOpenAIService>();
            var ttsLogger = loggerFactory.CreateLogger<TextToSpeechService>();

            var httpClient = new HttpClient();
            var tableStorageService = new TableStorageService(_configuration, tableLogger);
            var azureOpenAIService = new AzureOpenAIService(_configuration, aiLogger);
            var textToSpeechService = new TextToSpeechService(_configuration, ttsLogger);

            var diagnosticsService = new DiagnosticsService(
                logger,
                tableStorageService,
                azureOpenAIService,
                textToSpeechService,
                httpClient);

            // Act
            var result = await diagnosticsService.CheckAzureOpenAIServiceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("working", result.ToLower());
        }
    }
}
