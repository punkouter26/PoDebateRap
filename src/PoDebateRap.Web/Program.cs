using PoDebateRap.Web.Services.Data;
using PoDebateRap.Web.Services.AI;
using PoDebateRap.Web.Services.Speech;
using PoDebateRap.Web.Services.Orchestration;
using PoDebateRap.Web.Services.News;
using PoDebateRap.Web.Services.Diagnostics;
using PoDebateRap.Web.Services.Factories;
using PoDebateRap.Web.Hubs;
using PoDebateRap.Web.HealthChecks;
using PoDebateRap.Web.Middleware;
using PoDebateRap.Web.Validators;
using PoDebateRap.Web.Endpoints;
using PoDebateRap.Web.Extensions;
using PoDebateRap.Web.Components;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using Azure.Identity;
using Radzen;

// Check if we're running in a test environment by looking for ASPNETCORE_ENVIRONMENT
var isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";

// Configure Serilog BEFORE creating the builder (skip for testing to avoid frozen logger issues)
if (!isTesting)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PoDebateRap")
        .WriteTo.Console()
        .WriteTo.Debug()
        .CreateBootstrapLogger();
}

try
{
    Log.Information("Starting PoDebateRap Web Application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
    builder.AddServiceDefaults();

    // Configure Azure Key Vault for secrets (both Development and Production)
    // In Development, user-secrets override Key Vault values if present
    // In Production, Key Vault is the primary source for secrets
    if (!isTesting)
    {
        var keyVaultName = builder.Configuration["Azure:KeyVault:Name"];
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            try
            {
                var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
                builder.Configuration.AddAzureKeyVault(
                    keyVaultUri, 
                    new DefaultAzureCredential(),
                    new PoDebateRapKeyVaultSecretManager());
                Log.Information("Azure Key Vault configured: {KeyVaultName}", keyVaultName);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to configure Azure Key Vault. Falling back to local configuration.");
            }
        }
    }

    // Configure Serilog with Application Insights (skip static logger freeze for testing)
    if (!isTesting)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PoDebateRap")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Debug());
    }
    else
    {
        // For testing, use simple logging without Serilog's freeze behavior
        builder.Services.AddLogging(logging => logging.AddConsole());
    }

    // Add Radzen components
    builder.Services.AddRadzenComponents();

    // Register HttpClient for server-side Blazor components to call APIs
    builder.Services.AddScoped(sp =>
    {
        var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
    });

    // Add Razor Components with Interactive Server and WebAssembly support
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    // Add SignalR for real-time debate updates
    builder.Services.AddSignalR();

    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<StartDebateRequestValidator>();

    // Add OpenAPI/Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register Global Exception Handler
    builder.Services.AddScoped<GlobalExceptionHandler>();

    // Register Data Services - Using Aspire Azure Table Storage integration
    // Skip Aspire table client in Testing environment (tests provide mocks)
    if (!isTesting)
    {
        builder.AddAzureTableClient("tables");
    }
    builder.Services.AddScoped<ITableStorageService, TableStorageService>();
    builder.Services.AddScoped<IRapperRepository, RapperRepository>();

    // Register AI Service
    builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();

    // Register TextToSpeech Service
    builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();

    // Register Debate Service Factory
    builder.Services.AddSingleton<IDebateServiceFactory, DebateServiceFactory>();

    // Register Debate Orchestrator as a Singleton
    builder.Services.AddSingleton<IDebateOrchestrator, DebateOrchestrator>();

    // Register News Service and configure HttpClient
    builder.Services.AddHttpClient<INewsService, NewsService>();

    // Register Diagnostics Service
    builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>();

    // Add Application Insights for telemetry and logging
    builder.Services.AddApplicationInsightsTelemetry();

    // Register Custom Telemetry Service
    builder.Services.AddScoped<PoDebateRap.Web.Services.Telemetry.CustomTelemetryService>();

    // Add additional Health Checks for external dependencies (skip in Testing)
    if (!isTesting)
    {
        builder.Services.AddHealthChecks()
            .AddCheck<AzureTableStorageHealthCheck>(
                "azure_table_storage",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "database", "live"])
            .AddCheck<AzureOpenAIHealthCheck>(
                "azure_openai",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "ai"])
            .AddCheck<TextToSpeechHealthCheck>(
                "azure_tts",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "ai"])
            .AddCheck<NewsApiHealthCheck>(
                "news_api",
                failureStatus: HealthStatus.Degraded,
                tags: ["external"]);
    }
    else
    {
        // Minimal health checks for testing
        builder.Services.AddHealthChecks();
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseWebAssemblyDebugging();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // Add Global Exception Handler Middleware
    app.UseMiddleware<GlobalExceptionHandler>();

    app.UseHttpsRedirection();

    // Map static assets (required for .NET 10 Blazor before AddInteractiveWebAssemblyRenderMode)
    app.MapStaticAssets();
    app.UseAntiforgery();

    // Map Aspire default endpoints (includes /alive endpoint)
    // Note: We don't call MapDefaultEndpoints() here because we define custom /health endpoint below
    if (app.Environment.IsDevelopment())
    {
        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    // Map custom health check endpoint with detailed JSON responses
    app.MapHealthChecks("/health", new HealthCheckOptions
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

    // Map SignalR hub
    app.MapHub<DebateHub>("/debatehub");

    // Map Minimal API endpoints
    app.MapDebateEndpoints();
    app.MapRapperEndpoints();
    app.MapTopicEndpoints();
    app.MapNewsEndpoints();
    app.MapDiagnosticsEndpoints();

    // Map Razor Components
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(PoDebateRap.Web.Client._Imports).Assembly);

    // --- Data Seeding ---
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var rapperRepository = services.GetRequiredService<IRapperRepository>();
            try
            {
                app.Logger.LogInformation("Attempting initial data seeding...");
                await rapperRepository.SeedInitialRappersAsync();
                app.Logger.LogInformation("Initial data seeding completed (if necessary).");
            }
            catch (Exception seedEx)
            {
                app.Logger.LogError(seedEx, "Error during initial data seeding. Seeding skipped.");
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down PoDebateRap Web Application");
    Log.CloseAndFlush();
}

// Make Program accessible to integration tests
public partial class Program { }
