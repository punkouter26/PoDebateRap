using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.ServerApi.Logging; // For FileLoggerProvider
using PoDebateRap.Shared.Models; // For DebateState, etc.
using Azure.Identity; // For DefaultAzureCredential
using Azure.AI.OpenAI; // For OpenAIClient
using Microsoft.CognitiveServices.Speech; // For SpeechConfig
using Microsoft.CognitiveServices.Speech.Audio; // For AudioDataStream
using Microsoft.Extensions.Configuration; // Required for IConfiguration

var builder = WebApplication.CreateBuilder(args);

// --- Configure Logging ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
{
    var logFilePath = Path.Combine(builder.Environment.ContentRootPath, "..", "log.txt");
    builder.Logging.AddProvider(new FileLoggerProvider(logFilePath));
    Console.WriteLine($"File logging enabled at: {logFilePath}");
}
// --- End Logging Configuration ---

// --- Add Key Vault Configuration ---
var keyVaultUri = builder.Configuration["KeyVaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
        Console.WriteLine($"Successfully added Key Vault configuration source: {keyVaultUri}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error adding Key Vault configuration source: {ex.Message}");
    }
}
else
{
    Console.WriteLine("KeyVaultUri not found in configuration. Skipping Key Vault setup.");
}
// --- End Key Vault Configuration ---

// Add services to the container.
builder.Services.AddControllers(); // Add controllers for Web API
builder.Services.AddRazorPages(); // Add Razor Pages support for Blazor
builder.Services.AddEndpointsApiExplorer(); // For OpenAPI/Swagger
builder.Services.AddSwaggerGen(); // For OpenAPI/Swagger

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:5000", "https://localhost:5001") // Adjust origins as needed for your Blazor client
                         .AllowAnyHeader()
                         .AllowAnyMethod());
});

// Register Data Services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IRapperRepository, RapperRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

// Register AI Service
builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();

// Register TextToSpeech Service
builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();

// Register Debate Orchestrator as a Singleton
builder.Services.AddSingleton<IDebateOrchestrator, DebateOrchestrator>();

// Register News Service and configure HttpClient
builder.Services.AddHttpClient<INewsService, NewsService>();

// Register Diagnostics Service
builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();
builder.Services.AddHttpClient(); // Ensure HttpClientFactory is available (needed for DebateOrchestrator and NewsService)

// Add Application Insights for telemetry and logging
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles(); // Serve Blazor WebAssembly framework files
app.UseStaticFiles(); // Serve static files from wwwroot

app.UseRouting(); // Use routing middleware

app.UseCors("AllowSpecificOrigin"); // Use CORS policy

app.UseAuthorization(); // Use authorization middleware

app.MapControllers(); // Map controllers
app.MapRazorPages(); // Map Razor Pages
app.MapFallbackToFile("index.html"); // Fallback to index.html for client-side routing

// --- Data Seeding ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var rapperRepository = services.GetRequiredService<IRapperRepository>();
        var topicRepository = services.GetRequiredService<ITopicRepository>();

        var configuration = services.GetRequiredService<IConfiguration>();
        if (!string.IsNullOrEmpty(configuration["Azure:StorageConnectionString"]))
        {
            try
            {
                app.Logger.LogInformation("Attempting initial data seeding...");
                rapperRepository.SeedInitialRappersAsync().GetAwaiter().GetResult();
                app.Logger.LogInformation("Initial data seeding completed (if necessary).");
            }
            catch (Exception seedEx)
            {
                app.Logger.LogError(seedEx, "Error during initial data seeding (likely due to invalid connection string or table access issue). Seeding skipped.");
            }
        }
        else
        {
            app.Logger.LogWarning("Azure Storage connection string not found. Skipping initial data seeding.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during data seeding.");
    }
}
// --- End Data Seeding ---

app.Run();
