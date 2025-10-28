# PoDebateRap.ServerApi

ASP.NET Core Web API that powers the PoDebateRap application. Provides REST endpoints for debate orchestration, integrates with Azure AI services (OpenAI, Speech), manages data storage, and hosts the Blazor WebAssembly client.

## Project Overview

- **Framework**: .NET 9.0
- **Type**: ASP.NET Core Web API
- **Hosting**: Hosts Blazor WebAssembly client (hybrid hosting model)
- **Ports**: HTTP 5000, HTTPS 5001
- **Architecture**: Vertical Slice with Clean Architecture boundaries

## Key Components

### Controllers

| Controller | Route | Purpose |
|------------|-------|---------|
| `DebateController` | `/api/debate` | Debate management (start, get turns, judge) |
| `DiagnosticsController` | `/api/diagnostics` | Health checks and system diagnostics |
| `HealthController` | `/api/health` | Service-specific health endpoints |

### Services

#### AI Services (`Services/AI/`)
- **`AzureOpenAIService`**: Integrates with Azure OpenAI GPT-4 for:
  - Rap verse generation (in rapper's style)
  - Debate judging (winner determination)
  - Contextual responses based on debate history

#### Speech Services (`Services/Speech/`)
- **`TextToSpeechService`**: Converts text to audio using Azure Speech Services:
  - Neural TTS voices
  - Rapper-specific voice mapping
  - MP3 audio generation

#### Data Services (`Services/Data/`)
- **`TableStorageService`**: Azure Table Storage client wrapper
- **`RapperRepository`**: Manages rapper profiles (CRUD operations)
- **`DebateRepository`**: Stores debate history and turns

#### Orchestration Services (`Services/Orchestration/`)
- **`DebateOrchestrator`**: Coordinates multi-step debate flow:
  1. Generate AI verse for current rapper
  2. Convert verse to audio
  3. Store turn in database
  4. Send real-time update via SignalR

#### News Services (`Services/News/`)
- **`NewsService`**: Fetches trending topics from NewsAPI

#### Telemetry Services (`Services/Telemetry/`)
- **`CustomTelemetryService`**: Application Insights custom event tracking:
  - Debate lifecycle events
  - AI model usage (tokens, latency)
  - TTS usage (characters, generation time)
  - Storage operations
  - Error tracking

### SignalR Hubs (`Hubs/`)
- **`DebateHub`**: Real-time communication for:
  - Debate progress updates
  - Turn completion notifications
  - Error broadcasts

## Key Methods

### DebateController

```csharp
// Start a new debate
POST /api/debate/start
{
  "rapper1Id": "eminem",
  "rapper2Id": "tupac",
  "topic": "Is social media good for society?"
}
→ Returns: DebateResponse with debateId

// Get all turns for a debate
GET /api/debate/{debateId}/turns
→ Returns: List<DebateTurn>

// Generate next turn
POST /api/debate/{debateId}/turn
{
  "currentRapperId": "eminem",
  "context": "Previous verse context"
}
→ Returns: DebateTurn with verse and audioBase64

// Judge the debate
POST /api/debate/{debateId}/judge
→ Returns: JudgmentResponse with winner and reasoning
```

### DebateOrchestrator

```csharp
// Orchestrate a complete debate turn
public async Task<DebateTurn> OrchestrateDebateTurnAsync(
    string debateId,
    string currentRapperId,
    string opponentId,
    string topic,
    List<string> previousVerses,
    CancellationToken cancellationToken = default)
{
    // 1. Generate verse via AI
    var verse = await _aiService.GenerateVerseAsync(...);
    
    // 2. Convert to audio
    var audio = await _ttsService.GenerateSpeechAsync(...);
    
    // 3. Save to database
    var turn = await _debateRepo.SaveTurnAsync(...);
    
    // 4. Broadcast via SignalR
    await _hubContext.Clients.All.SendAsync("TurnCompleted", turn);
    
    return turn;
}
```

### AzureOpenAIService

```csharp
// Generate a rap verse in the rapper's style
public async Task<VerseResponse> GenerateVerseAsync(
    string rapperId,
    string topic,
    string? context = null,
    CancellationToken cancellationToken = default)
{
    var rapper = await _rapperRepo.GetByIdAsync(rapperId);
    
    var prompt = $@"You are {rapper.Name}, a legendary rapper known for {rapper.Style}.
    Generate a rap verse debating this topic: {topic}
    Context from previous turns: {context}
    Style: {rapper.Style}
    Format: 4-8 bars with clear rhyme scheme";
    
    var completion = await _openAIClient.GetChatCompletionsAsync(...);
    
    return new VerseResponse
    {
        Verse = completion.Content,
        TokenCount = completion.Usage.TotalTokens,
        Success = true
    };
}

// Judge the debate and determine winner
public async Task<JudgmentResponse> JudgeDebateAsync(
    List<DebateTurn> turns,
    string topic,
    CancellationToken cancellationToken = default)
{
    var prompt = $@"You are an impartial rap battle judge. Analyze these verses:
    {string.Join("\n\n", turns.Select(t => $"{t.RapperName}: {t.Verse}"))}
    
    Criteria:
    1. Lyrical skill (rhyme scheme, wordplay, flow)
    2. Argumentation (relevance to topic, persuasiveness)
    3. Style and delivery
    
    Determine the winner and explain why.";
    
    // AI analysis
    return new JudgmentResponse { Winner = "...", Reasoning = "..." };
}
```

### TextToSpeechService

```csharp
// Convert text to MP3 audio
public async Task<byte[]> GenerateSpeechAsync(
    string text,
    string rapperId,
    CancellationToken cancellationToken = default)
{
    var voice = GetVoiceForRapper(rapperId);  // e.g., "en-US-GuyNeural"
    
    var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
    config.SpeechSynthesisVoiceName = voice;
    config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
    
    using var synthesizer = new SpeechSynthesizer(config, null);
    var result = await synthesizer.SpeakTextAsync(text);
    
    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
    {
        return result.AudioData;
    }
    
    throw new Exception($"TTS failed: {result.Reason}");
}

private string GetVoiceForRapper(string rapperId)
{
    return rapperId switch
    {
        "eminem" => "en-US-GuyNeural",
        "tupac" => "en-US-DavisNeural",
        "nas" => "en-US-JasonNeural",
        _ => "en-US-GuyNeural"
    };
}
```

### RapperRepository

```csharp
// Get rapper by ID
public async Task<Rapper?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
{
    var tableClient = await _tableService.GetTableClientAsync("Rappers");
    var response = await tableClient.GetEntityAsync<TableEntity>("Rappers", id, cancellationToken: cancellationToken);
    
    return MapFromTableEntity(response.Value);
}

// Get all rappers
public async Task<List<Rapper>> GetAllAsync(CancellationToken cancellationToken = default)
{
    var tableClient = await _tableService.GetTableClientAsync("Rappers");
    var rappers = new List<Rapper>();
    
    await foreach (var entity in tableClient.QueryAsync<TableEntity>(
        filter: "PartitionKey eq 'Rappers'",
        cancellationToken: cancellationToken))
    {
        rappers.Add(MapFromTableEntity(entity));
    }
    
    return rappers;
}

// Add or update rapper
public async Task<Rapper> UpsertAsync(Rapper rapper, CancellationToken cancellationToken = default)
{
    var tableClient = await _tableService.GetTableClientAsync("Rappers");
    var entity = MapToTableEntity(rapper);
    
    await tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
    
    return rapper;
}
```

### CustomTelemetryService

```csharp
// Track debate lifecycle
public void TrackDebateStarted(string debateId, string rapper1, string rapper2, string topic)
{
    _telemetryClient.TrackEvent("DebateStarted", new Dictionary<string, string>
    {
        ["DebateId"] = debateId,
        ["Rapper1"] = rapper1,
        ["Rapper2"] = rapper2,
        ["Topic"] = topic
    });
}

public void TrackDebateCompleted(string debateId, string winner, int totalTurns, double durationSeconds)
{
    _telemetryClient.TrackEvent("DebateCompleted", 
        properties: new Dictionary<string, string>
        {
            ["DebateId"] = debateId,
            ["Winner"] = winner
        },
        metrics: new Dictionary<string, double>
        {
            ["TotalTurns"] = totalTurns,
            ["DurationSeconds"] = durationSeconds
        });
}

// Track AI usage
public void TrackAIModelUsage(string modelName, string operation, int tokenCount, double responseTimeMs)
{
    _telemetryClient.TrackEvent("AIModelUsage",
        properties: new Dictionary<string, string>
        {
            ["ModelName"] = modelName,
            ["Operation"] = operation
        },
        metrics: new Dictionary<string, double>
        {
            ["TokenCount"] = tokenCount,
            ["ResponseTimeMs"] = responseTimeMs
        });
}
```

## Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Azure": {
    "OpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "DeploymentName": "gpt-4"
    },
    "Speech": {
      "Region": "eastus"
    }
  },
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2"
  }
}
```

### User Secrets (Development)
```bash
dotnet user-secrets set "Azure:OpenAI:ApiKey" "your-key"
dotnet user-secrets set "Azure:Speech:SubscriptionKey" "your-key"
dotnet user-secrets set "Azure:StorageConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "NewsApi:ApiKey" "your-key"
```

### Azure App Service Settings (Production)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`
- `Azure__OpenAI__ApiKey`
- `Azure__Speech__SubscriptionKey`
- `Azure__StorageConnectionString`
- `NewsApi__ApiKey`

## Health Checks

The API exposes comprehensive health check endpoints:

```http
GET /api/health/status          # Overall application health
GET /api/health/openai          # Azure OpenAI connectivity
GET /api/health/speech          # Azure Speech Services
GET /api/health/storage         # Azure Table Storage
GET /api/health/news            # NewsAPI connectivity
```

Each endpoint returns:
```json
{
  "status": "Healthy" | "Degraded" | "Unhealthy",
  "service": "Azure OpenAI",
  "message": "Service is operational",
  "timestamp": "2025-10-28T12:00:00Z"
}
```

## Error Handling

The API uses **global exception middleware** that transforms all errors into RFC 7807 Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Rapper with ID 'unknown' not found",
  "instance": "/api/debate/start"
}
```

## Logging

**Serilog** with structured logging and Application Insights sink:

```csharp
Log.Information(
    "Debate turn generated for {RapperId} on topic {Topic} in {ElapsedMs}ms",
    rapperId,
    topic,
    elapsedMs);
```

Logs are written to:
- Console (Development)
- Application Insights (Production)
- File: `log.txt` (All environments)

## Running the Server

```bash
# Development mode with watch
dotnet watch run

# Production mode
dotnet run --configuration Release

# With specific environment
ASPNETCORE_ENVIRONMENT=Staging dotnet run
```

## Testing

```bash
# Run all tests for this project
dotnet test ../../Tests/PoDebateRap.UnitTests
dotnet test ../../Tests/PoDebateRap.IntegrationTests

# Run specific test class
dotnet test --filter "FullyQualifiedName~DebateOrchestratorTests"
```

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.SignalR` - Real-time communication
- `Azure.AI.OpenAI` - GPT-4 integration
- `Microsoft.CognitiveServices.Speech` - Text-to-speech
- `Azure.Data.Tables` - Table Storage
- `Serilog.AspNetCore` - Structured logging
- `Serilog.Sinks.ApplicationInsights` - Telemetry sink
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation

### Project References
- `PoDebateRap.Client` - Blazor WebAssembly client (hosted)
- `PoDebateRap.Shared` - Shared models and DTOs

## API Documentation

Swagger UI is available at: **`http://localhost:5000/swagger`**

OpenAPI spec: **`http://localhost:5000/swagger/v1/swagger.json`**

---

**Last Updated**: October 28, 2025
