using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.HealthChecks;
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
            // Arrange - Build service provider with health checks
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IConfiguration>(_configuration);

            // Register HttpClientFactory required by NewsApiHealthCheck
            services.AddHttpClient();

            // Register dependencies for health checks
            services.AddSingleton<ITableStorageService, TableStorageService>();
            services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
            services.AddSingleton<ITextToSpeechService, TextToSpeechService>();

            // Register health checks
            services.AddHealthChecks()
                .AddCheck<AzureTableStorageHealthCheck>("Azure Table Storage")
                .AddCheck<AzureOpenAIHealthCheck>("Azure OpenAI")
                .AddCheck<TextToSpeechHealthCheck>("Text-to-Speech")
                .AddCheck<NewsApiHealthCheck>("News API");

            // Register DiagnosticsService
            services.AddScoped<IDiagnosticsService, DiagnosticsService>();

            var serviceProvider = services.BuildServiceProvider();
            var diagnosticsService = serviceProvider.GetRequiredService<IDiagnosticsService>();

            // Act
            var results = await diagnosticsService.RunAllChecksAsync();

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Any());
            Assert.Contains(results, r => r.CheckName == "Azure Table Storage");
            Assert.Contains(results, r => r.CheckName == "Azure OpenAI");
        }
    }
}
