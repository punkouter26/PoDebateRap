using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace PoDebateRap.Web.Services.Telemetry;

/// <summary>
/// Custom telemetry service for tracking high-value application metrics
/// </summary>
public class CustomTelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<CustomTelemetryService> _logger;

    public CustomTelemetryService(TelemetryClient telemetryClient, ILogger<CustomTelemetryService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    /// <summary>
    /// Track debate started event with structured properties
    /// </summary>
    public void TrackDebateStarted(string rapper1, string rapper2, string topic)
    {
        var properties = new Dictionary<string, string>
        {
            { "Rapper1", rapper1 },
            { "Rapper2", rapper2 },
            { "Topic", topic },
            { "EventType", "DebateStarted" }
        };

        _telemetryClient.TrackEvent("DebateStarted", properties);
        _logger.LogInformation("Debate started: {Rapper1} vs {Rapper2} on topic {Topic}", rapper1, rapper2, topic);
    }

    /// <summary>
    /// Track debate completed event with performance metrics
    /// </summary>
    public void TrackDebateCompleted(string rapper1, string rapper2, string winner, int totalTurns, TimeSpan duration)
    {
        var properties = new Dictionary<string, string>
        {
            { "Rapper1", rapper1 },
            { "Rapper2", rapper2 },
            { "Winner", winner },
            { "EventType", "DebateCompleted" }
        };

        var metrics = new Dictionary<string, double>
        {
            { "TotalTurns", totalTurns },
            { "DurationSeconds", duration.TotalSeconds }
        };

        _telemetryClient.TrackEvent("DebateCompleted", properties, metrics);
        _logger.LogInformation("Debate completed: {Winner} won after {TotalTurns} turns in {Duration:F1}s",
            winner, totalTurns, duration.TotalSeconds);
    }

    /// <summary>
    /// Track AI model usage for cost and performance analysis
    /// </summary>
    public void TrackAIModelUsage(string modelName, string operation, int tokenCount, TimeSpan responseTime)
    {
        var properties = new Dictionary<string, string>
        {
            { "ModelName", modelName },
            { "Operation", operation },
            { "EventType", "AIModelUsage" }
        };

        var metrics = new Dictionary<string, double>
        {
            { "TokenCount", tokenCount },
            { "ResponseTimeMs", responseTime.TotalMilliseconds }
        };

        _telemetryClient.TrackEvent("AIModelUsage", properties, metrics);
        _logger.LogInformation("AI Model usage: {ModelName} for {Operation}, {TokenCount} tokens in {ResponseTime:F0}ms",
            modelName, operation, tokenCount, responseTime.TotalMilliseconds);
    }

    /// <summary>
    /// Track text-to-speech usage for cost analysis
    /// </summary>
    public void TrackTextToSpeechUsage(string voice, int characterCount, TimeSpan generationTime)
    {
        var properties = new Dictionary<string, string>
        {
            { "Voice", voice },
            { "EventType", "TextToSpeechUsage" }
        };

        var metrics = new Dictionary<string, double>
        {
            { "CharacterCount", characterCount },
            { "GenerationTimeMs", generationTime.TotalMilliseconds }
        };

        _telemetryClient.TrackEvent("TextToSpeechUsage", properties, metrics);
        _logger.LogInformation("TTS usage: {Voice}, {CharacterCount} chars in {GenerationTime:F0}ms",
            voice, characterCount, generationTime.TotalMilliseconds);
    }

    /// <summary>
    /// Track storage operations for performance monitoring
    /// </summary>
    public void TrackStorageOperation(string operation, string tableName, bool success, TimeSpan duration)
    {
        var properties = new Dictionary<string, string>
        {
            { "Operation", operation },
            { "TableName", tableName },
            { "Success", success.ToString() },
            { "EventType", "StorageOperation" }
        };

        var metrics = new Dictionary<string, double>
        {
            { "DurationMs", duration.TotalMilliseconds }
        };

        _telemetryClient.TrackEvent("StorageOperation", properties, metrics);

        if (!success)
        {
            _logger.LogWarning("Storage operation failed: {Operation} on {TableName} took {Duration:F0}ms",
                operation, tableName, duration.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Track custom errors with structured context
    /// </summary>
    public void TrackError(Exception ex, string operation, Dictionary<string, string>? additionalProperties = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Operation", operation },
            { "ErrorType", ex.GetType().Name }
        };

        if (additionalProperties != null)
        {
            foreach (var prop in additionalProperties)
            {
                properties[prop.Key] = prop.Value;
            }
        }

        _telemetryClient.TrackException(ex, properties);
        _logger.LogError(ex, "Error in operation {Operation}", operation);
    }

    /// <summary>
    /// Track custom dependency calls (external APIs, databases)
    /// </summary>
    public void TrackDependency(string dependencyType, string target, string command, TimeSpan duration, bool success)
    {
        var dependency = new DependencyTelemetry
        {
            Type = dependencyType,
            Target = target,
            Data = command,
            Duration = duration,
            Success = success,
            Timestamp = DateTimeOffset.UtcNow
        };

        _telemetryClient.TrackDependency(dependency);
    }
}
