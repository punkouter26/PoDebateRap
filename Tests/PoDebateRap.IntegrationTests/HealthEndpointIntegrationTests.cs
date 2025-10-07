using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

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
        public async Task DiagnosticsService_RunAllChecks_ReturnsResults()
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
            var results = await diagnosticsService.RunAllChecksAsync();

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Any());
            Assert.Contains(results, r => r.CheckName == "API Health");
            Assert.Contains(results, r => r.CheckName == "Internet Connection");
        }
    }
}
