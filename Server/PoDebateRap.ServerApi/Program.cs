using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.Services.Orchestration;
using PoDebateRap.ServerApi.Services.News;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;
using PoDebateRap.ServerApi.Hubs;
using PoDebateRap.ServerApi.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure User Secrets for Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers(); // Add controllers for Web API
builder.Services.AddRazorPages(); // Add Razor Pages support for Blazor
builder.Services.AddEndpointsApiExplorer(); // For OpenAPI/Swagger
builder.Services.AddSwaggerGen(); // For OpenAPI/Swagger

// Register Data Services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IRapperRepository, RapperRepository>();

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

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureTableStorageHealthCheck>(
        "azure_table_storage",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "database" })
    .AddCheck<AzureOpenAIHealthCheck>(
        "azure_openai",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "ai" })
    .AddCheck<TextToSpeechHealthCheck>(
        "azure_tts",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "ai" })
    .AddCheck<NewsApiHealthCheck>(
        "news_api",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "external" });

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

// Map Health Check Endpoints
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            isHealthy = report.Status == HealthStatus.Healthy,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Don't run any checks, just return if the app is running
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = "alive",
            timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
});

app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status == HealthStatus.Healthy ? "ready" : "not ready",
            isReady = report.Status == HealthStatus.Healthy,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        };

        if (report.Status != HealthStatus.Healthy)
        {
            context.Response.StatusCode = 503; // Service Unavailable
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

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
