using System.Diagnostics.Metrics;

namespace PoDebateRap.Web.Services.Telemetry;

/// <summary>
/// OpenTelemetry-based custom metrics service for PoDebateRap.
/// Uses modern Meter API for business-critical metrics.
/// </summary>
public sealed class DebateMetrics : IDisposable
{
    private readonly Meter _debateMeter;
    private readonly Meter _aiMeter;
    private readonly Meter _speechMeter;

    // Debate metrics
    private readonly Counter<long> _debatesStarted;
    private readonly Counter<long> _debatesCompleted;
    private readonly Counter<long> _debatesFailed;
    private readonly Histogram<double> _debateDuration;
    private readonly UpDownCounter<long> _activeDebates;

    // AI metrics
    private readonly Counter<long> _aiRequests;
    private readonly Counter<long> _aiTokensUsed;
    private readonly Histogram<double> _aiResponseTime;
    private readonly Counter<long> _aiFailed;

    // Speech (TTS) metrics
    private readonly Counter<long> _ttsRequests;
    private readonly Counter<long> _ttsCharactersProcessed;
    private readonly Histogram<double> _ttsGenerationTime;
    private readonly Counter<long> _ttsFailed;

    public DebateMetrics()
    {
        // Initialize meters for different domains
        _debateMeter = new Meter("PoDebateRap.Debates", "1.0.0");
        _aiMeter = new Meter("PoDebateRap.AI", "1.0.0");
        _speechMeter = new Meter("PoDebateRap.Speech", "1.0.0");

        // Debate counters and histograms
        _debatesStarted = _debateMeter.CreateCounter<long>(
            "debates.started",
            description: "Total number of debates started");

        _debatesCompleted = _debateMeter.CreateCounter<long>(
            "debates.completed",
            description: "Total number of debates completed successfully");

        _debatesFailed = _debateMeter.CreateCounter<long>(
            "debates.failed",
            description: "Total number of debates that failed");

        _debateDuration = _debateMeter.CreateHistogram<double>(
            "debates.duration",
            unit: "ms",
            description: "Duration of debates in milliseconds");

        _activeDebates = _debateMeter.CreateUpDownCounter<long>(
            "debates.active",
            description: "Number of currently active debates");

        // AI counters and histograms
        _aiRequests = _aiMeter.CreateCounter<long>(
            "ai.requests",
            description: "Total number of AI API requests");

        _aiTokensUsed = _aiMeter.CreateCounter<long>(
            "ai.tokens.used",
            description: "Total number of tokens consumed by AI requests");

        _aiResponseTime = _aiMeter.CreateHistogram<double>(
            "ai.response.time",
            unit: "ms",
            description: "AI response time in milliseconds");

        _aiFailed = _aiMeter.CreateCounter<long>(
            "ai.requests.failed",
            description: "Total number of failed AI requests");

        // TTS counters and histograms
        _ttsRequests = _speechMeter.CreateCounter<long>(
            "tts.requests",
            description: "Total number of TTS requests");

        _ttsCharactersProcessed = _speechMeter.CreateCounter<long>(
            "tts.characters.processed",
            description: "Total number of characters converted to speech");

        _ttsGenerationTime = _speechMeter.CreateHistogram<double>(
            "tts.generation.time",
            unit: "ms",
            description: "TTS generation time in milliseconds");

        _ttsFailed = _speechMeter.CreateCounter<long>(
            "tts.requests.failed",
            description: "Total number of failed TTS requests");
    }

    // Debate tracking methods
    public void RecordDebateStarted(string rapper1, string rapper2, string topic)
    {
        _debatesStarted.Add(1,
            new KeyValuePair<string, object?>("rapper1", rapper1),
            new KeyValuePair<string, object?>("rapper2", rapper2),
            new KeyValuePair<string, object?>("topic", topic));
        _activeDebates.Add(1);
    }

    public void RecordDebateCompleted(string winner, double durationMs, int turnCount)
    {
        _debatesCompleted.Add(1,
            new KeyValuePair<string, object?>("winner", winner),
            new KeyValuePair<string, object?>("turn_count", turnCount));
        _debateDuration.Record(durationMs);
        _activeDebates.Add(-1);
    }

    public void RecordDebateFailed(string reason)
    {
        _debatesFailed.Add(1,
            new KeyValuePair<string, object?>("reason", reason));
        _activeDebates.Add(-1);
    }

    // AI tracking methods
    public void RecordAIRequest(string model, int promptTokens, int completionTokens, double responseTimeMs)
    {
        _aiRequests.Add(1,
            new KeyValuePair<string, object?>("model", model));
        _aiTokensUsed.Add(promptTokens + completionTokens,
            new KeyValuePair<string, object?>("model", model),
            new KeyValuePair<string, object?>("token_type", "total"));
        _aiResponseTime.Record(responseTimeMs,
            new KeyValuePair<string, object?>("model", model));
    }

    public void RecordAIFailure(string model, string errorType)
    {
        _aiFailed.Add(1,
            new KeyValuePair<string, object?>("model", model),
            new KeyValuePair<string, object?>("error_type", errorType));
    }

    // TTS tracking methods
    public void RecordTTSRequest(string voice, int characterCount, double generationTimeMs)
    {
        _ttsRequests.Add(1,
            new KeyValuePair<string, object?>("voice", voice));
        _ttsCharactersProcessed.Add(characterCount,
            new KeyValuePair<string, object?>("voice", voice));
        _ttsGenerationTime.Record(generationTimeMs,
            new KeyValuePair<string, object?>("voice", voice));
    }

    public void RecordTTSFailure(string voice, string errorType)
    {
        _ttsFailed.Add(1,
            new KeyValuePair<string, object?>("voice", voice),
            new KeyValuePair<string, object?>("error_type", errorType));
    }

    public void Dispose()
    {
        _debateMeter.Dispose();
        _aiMeter.Dispose();
        _speechMeter.Dispose();
    }
}
