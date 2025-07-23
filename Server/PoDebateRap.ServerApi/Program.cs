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
using PoDebateRap.ServerApi.Hubs;

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

// --- Configure User Secrets for Development ---
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    Console.WriteLine("User Secrets configured for development.");
}
// --- End User Secrets Configuration ---

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers(); // Add controllers for Web API
builder.Services.AddRazorPages(); // Add Razor Pages support for Blazor
builder.Services.AddEndpointsApiExplorer(); // For OpenAPI/Swagger
builder.Services.AddSwaggerGen(); // For OpenAPI/Swagger

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


// Add Application Insights for telemetry and logging
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // In development, use the DeveloperExceptionPage for detailed error information
    app.UseDeveloperExceptionPage();
}
else
{
    // In production, use a more user-friendly error page and global exception handler
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles(); // Serve Blazor WebAssembly framework files
app.UseStaticFiles(); // Serve static files from wwwroot

app.UseRouting(); // Use routing middleware

app.UseAuthorization(); // Use authorization middleware

app.MapControllers(); // Map controllers
app.MapRazorPages(); // Map Razor Pages
app.MapHub<DebateHub>("/debatehub");
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
                topicRepository.SeedInitialTopicsAsync().GetAwaiter().GetResult();
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
