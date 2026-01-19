using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PoDebateRap.Web.Services.AI;
using PoDebateRap.Web.Services.Speech;
using PoDebateRap.Web.Services.Data;
using PoDebateRap.Web.Services.News;
using PoDebateRap.Shared.Models;
using Azure.Data.Tables;

namespace PoDebateRap.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that mocks external Azure dependencies
/// to allow integration tests to run without live API credentials.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IAzureOpenAIService> MockOpenAIService { get; } = new();
    public Mock<ITextToSpeechService> MockTtsService { get; } = new();
    public Mock<ITableStorageService> MockTableStorageService { get; } = new();
    public Mock<IRapperRepository> MockRapperRepository { get; } = new();
    public Mock<INewsService> MockNewsService { get; } = new();
    public Mock<TableServiceClient> MockTableServiceClient { get; } = new();

    static CustomWebApplicationFactory()
    {
        // Set environment variable BEFORE any tests run to signal Program.cs to skip Serilog bootstrap
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific settings on top of existing configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Azure:StorageConnectionString"] = "UseDevelopmentStorage=true",
                ["Azure:OpenAI:Endpoint"] = "https://mock.openai.azure.com/",
                ["Azure:OpenAI:ApiKey"] = "mock-api-key",
                ["Azure:OpenAI:DeploymentName"] = "gpt-4o",
                ["Azure:Speech:Endpoint"] = "https://mock.speech.azure.com/",
                ["Azure:Speech:SubscriptionKey"] = "mock-subscription-key",
                ["Azure:Speech:Region"] = "eastus",
                ["NewsApi:ApiKey"] = "mock-news-api-key",
                // Disable Aspire Azure Table client from connection string lookup
                ["ConnectionStrings:tables"] = "UseDevelopmentStorage=true",
            });
        });

        // Setup default mock behaviors BEFORE ConfigureServices
        SetupMockDefaults();

        // Use ConfigureServices to ADD mock service registrations
        // In .NET DI, the LAST registration wins when resolving, so adding mocks
        // after the real services effectively replaces them without needing RemoveAll
        builder.ConfigureServices(services =>
        {
            // Register mock TableServiceClient to satisfy Aspire dependencies
            services.AddSingleton(MockTableServiceClient.Object);

            // Register mocks as singletons - these will be resolved instead of the real services
            // because they are registered AFTER the real implementations
            services.AddSingleton<IAzureOpenAIService>(MockOpenAIService.Object);
            services.AddSingleton<ITextToSpeechService>(MockTtsService.Object);
            services.AddSingleton<ITableStorageService>(MockTableStorageService.Object);
            services.AddSingleton<IRapperRepository>(MockRapperRepository.Object);
            services.AddSingleton<INewsService>(MockNewsService.Object);
        });
    }

    private void SetupMockDefaults()
    {
        // Setup OpenAI mock
        MockOpenAIService
            .Setup(s => s.GenerateDebateTurnAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mock rap verse response from AI");

        MockOpenAIService
            .Setup(s => s.JudgeDebateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JudgeDebateResponse
            {
                WinnerName = "Mock Winner",
                Reasoning = "Mock reasoning for the winner",
                Stats = new DebateStats
                {
                    Rapper1LogicScore = 4,
                    Rapper1SentimentScore = 4,
                    Rapper1AdherenceScore = 4,
                    Rapper1RebuttalScore = 4,
                    Rapper2LogicScore = 3,
                    Rapper2SentimentScore = 3,
                    Rapper2AdherenceScore = 3,
                    Rapper2RebuttalScore = 3
                }
            });

        // Setup TTS mock - return valid WAV header
        var mockWavData = CreateMockWavData();
        MockTtsService
            .Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockWavData);

        // Setup Rapper Repository mock
        MockRapperRepository
            .Setup(r => r.GetAllRappersAsync())
            .ReturnsAsync(GetTestRappers());

        MockRapperRepository
            .Setup(r => r.SeedInitialRappersAsync())
            .Returns(Task.CompletedTask);

        // Setup News Service mock
        MockNewsService
            .Setup(s => s.GetTopHeadlinesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<NewsHeadline>
            {
                new() { Title = "Mock Headline 1", Description = "Test description" },
                new() { Title = "Mock Headline 2", Description = "Test description 2" }
            });
    }

    private static byte[] CreateMockWavData()
    {
        // Create a minimal valid WAV file header (44 bytes)
        var header = new byte[44];
        
        // RIFF header
        header[0] = 0x52; // 'R'
        header[1] = 0x49; // 'I'
        header[2] = 0x46; // 'F'
        header[3] = 0x46; // 'F'
        
        // File size (36 + data size) - little endian
        BitConverter.GetBytes(36).CopyTo(header, 4);
        
        // WAVE format
        header[8] = 0x57;  // 'W'
        header[9] = 0x41;  // 'A'
        header[10] = 0x56; // 'V'
        header[11] = 0x45; // 'E'
        
        // fmt chunk
        header[12] = 0x66; // 'f'
        header[13] = 0x6D; // 'm'
        header[14] = 0x74; // 't'
        header[15] = 0x20; // ' '
        
        // Subchunk1Size (16 for PCM)
        BitConverter.GetBytes(16).CopyTo(header, 16);
        
        // AudioFormat (1 for PCM)
        BitConverter.GetBytes((short)1).CopyTo(header, 20);
        
        // NumChannels (1 for mono)
        BitConverter.GetBytes((short)1).CopyTo(header, 22);
        
        // SampleRate (16000)
        BitConverter.GetBytes(16000).CopyTo(header, 24);
        
        // ByteRate
        BitConverter.GetBytes(32000).CopyTo(header, 28);
        
        // BlockAlign
        BitConverter.GetBytes((short)2).CopyTo(header, 32);
        
        // BitsPerSample
        BitConverter.GetBytes((short)16).CopyTo(header, 34);
        
        // data chunk
        header[36] = 0x64; // 'd'
        header[37] = 0x61; // 'a'
        header[38] = 0x74; // 't'
        header[39] = 0x61; // 'a'
        
        // Data size
        BitConverter.GetBytes(0).CopyTo(header, 40);
        
        return header;
    }

    private static List<Rapper> GetTestRappers()
    {
        return new List<Rapper>
        {
            new("Eminem") { RowKey = "eminem", Wins = 5, Losses = 2 },
            new("Snoop Dogg") { RowKey = "snoop-dogg", Wins = 4, Losses = 3 },
            new("Jay-Z") { RowKey = "jay-z", Wins = 6, Losses = 1 },
            new("Kendrick Lamar") { RowKey = "kendrick-lamar", Wins = 7, Losses = 0 },
            new("Nas") { RowKey = "nas", Wins = 3, Losses = 4 }
        };
    }
}

