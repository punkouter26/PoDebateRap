# PoDebateRap.Shared

Shared library containing DTOs (Data Transfer Objects), domain models, and validation logic used by both the Client (Blazor WebAssembly) and Server (ASP.NET Core API) projects.

## Project Overview

- **Framework**: .NET 9.0
- **Type**: Class Library
- **Purpose**: Share contracts between client and server
- **Dependencies**: Minimal (System.ComponentModel.DataAnnotations only)

## Core Models

### Domain Entities

#### `Rapper.cs`
Represents a legendary rapper in the system.

```csharp
public class Rapper
{
    public required string Id { get; init; }          // Unique identifier (e.g., "eminem")
    public required string Name { get; init; }        // Display name (e.g., "Eminem")
    public required string Style { get; init; }       // Rap style description
    public string? ImageUrl { get; init; }            // Optional avatar URL
    public string? Era { get; init; }                 // Era (e.g., "90s", "2000s")
    public string? VoiceId { get; init; }             // Azure Speech voice mapping
}
```

**Usage**:
- Client: Display in dropdowns, show in debate arena
- Server: Database entity, AI prompt context

---

#### `Topic.cs`
Represents a debate topic.

```csharp
public class Topic
{
    public required string Id { get; init; }          // Unique identifier
    public required string Title { get; init; }       // Topic text
    public string? Source { get; init; }              // Source (e.g., "NewsAPI", "Custom")
    public DateTime CreatedAt { get; init; }          // Creation timestamp
    public bool IsTrending { get; init; }             // Trending flag
}
```

**Usage**:
- Client: Display trending topics
- Server: Store in database, pass to AI

---

### Request DTOs

#### `DebateRequests.cs`

**DebateRequest**
Request to start a new debate.

```csharp
public class DebateRequest
{
    [Required]
    [MinLength(1)]
    public required string Rapper1Id { get; set; }
    
    [Required]
    [MinLength(1)]
    public required string Rapper2Id { get; set; }
    
    [Required]
    [MinLength(10, ErrorMessage = "Topic must be at least 10 characters")]
    [MaxLength(500, ErrorMessage = "Topic must be less than 500 characters")]
    public required string Topic { get; set; }
}
```

**Validation Rules**:
- Both rapper IDs required and non-empty
- Topic: 10-500 characters
- (Implicit) Rapper IDs should be different (enforced server-side)

**TurnRequest**
Request to generate the next turn.

```csharp
public class TurnRequest
{
    [Required]
    public required string DebateId { get; set; }
    
    [Required]
    public required string CurrentRapperId { get; set; }
    
    public string? Context { get; set; }              // Optional previous verse context
    public List<string>? PreviousVerses { get; set; } // Optional debate history
}
```

---

### Response DTOs

#### `DebateRequests.cs` (continued)

**DebateResponse**
Response after starting a debate.

```csharp
public class DebateResponse
{
    public required string DebateId { get; init; }
    public required string Rapper1Name { get; init; }
    public required string Rapper2Name { get; init; }
    public required string Topic { get; init; }
    public DateTime StartedAt { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**DebateTurn**
Represents a single turn in the debate.

```csharp
public class DebateTurn
{
    public required string DebateId { get; init; }
    public required int TurnNumber { get; init; }
    public required string RapperId { get; init; }
    public required string RapperName { get; init; }
    public required string Verse { get; init; }
    public required string AudioBase64 { get; init; }  // MP3 audio as base64
    public DateTime Timestamp { get; init; }
    public int? TokenCount { get; init; }              // AI tokens used
    public int? AudioDurationMs { get; init; }         // Audio length
}
```

**JudgmentResponse**
AI judge's decision.

```csharp
public class JudgmentResponse
{
    public required string WinnerId { get; init; }
    public required string WinnerName { get; init; }
    public required string Reasoning { get; init; }    // Judge's explanation
    public int Rapper1Score { get; init; }             // Score out of 10
    public int Rapper2Score { get; init; }             // Score out of 10
    public DateTime JudgedAt { get; init; }
}
```

---

### AI Service DTOs

#### `AIRequests.cs`

**VerseRequest**
Request to generate a rap verse.

```csharp
public class VerseRequest
{
    [Required]
    public required string RapperId { get; set; }
    
    [Required]
    public required string Topic { get; set; }
    
    public string? OpponentId { get; set; }
    public string? Context { get; set; }
    public int MaxBars { get; set; } = 8;              // Default 8 bars
}
```

**VerseResponse**
AI-generated verse response.

```csharp
public class VerseResponse
{
    public required string Verse { get; init; }
    public int TokenCount { get; init; }
    public double ResponseTimeMs { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
```

---

### Diagnostics DTOs

#### `DiagnosticResult.cs`

**HealthStatus**
Service health check result.

```csharp
public class HealthStatus
{
    public required string Service { get; init; }
    public required string Status { get; init; }       // "Healthy", "Degraded", "Unhealthy"
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
```

**AudioDiagnosticResult**
Audio test result.

```csharp
public class AudioDiagnosticResult
{
    public required byte[] AudioBytes { get; init; }
    public required string FirstBytesHex { get; init; } // First 50 bytes as hex
    public bool IsValid { get; init; }                  // RIFF header check
    public int ByteCount { get; init; }
    public string? ErrorMessage { get; init; }
}
```

---

### News DTOs

#### `NewsHeadline.cs`

```csharp
public class NewsHeadline
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? Source { get; init; }
    public string? Url { get; init; }
    public DateTime PublishedAt { get; init; }
    public string? ImageUrl { get; init; }
}
```

**Usage**:
- Fetch from NewsAPI
- Display as debate topic options

---

### Statistics DTOs

#### `DebateStats.cs`

```csharp
public class DebateStats
{
    public required string DebateId { get; init; }
    public int TotalTurns { get; init; }
    public double DurationSeconds { get; init; }
    public int TotalTokensUsed { get; init; }
    public int TotalAudioDurationMs { get; init; }
    public Dictionary<string, int> RapperTurnCounts { get; init; } = new();
}
```

**Usage**:
- Post-debate analytics
- Application Insights metrics

---

## Validation Attributes

All DTOs use `System.ComponentModel.DataAnnotations` for validation:

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Field must have a value | `public required string Name { get; set; }` |
| `[MinLength(n)]` | Minimum string length | `[MinLength(10)]` |
| `[MaxLength(n)]` | Maximum string length | `[MaxLength(500)]` |
| `[Range(min, max)]` | Numeric range | `[Range(1, 10)]` |
| `[EmailAddress]` | Valid email format | `[EmailAddress]` |
| `[Url]` | Valid URL format | `[Url]` |

### Validation in Server
```csharp
[ApiController]
public class DebateController : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] DebateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);  // Automatic validation
        }
        
        // Process request
    }
}
```

### Validation in Client
```razor
<EditForm Model="request" OnValidSubmit="StartDebateAsync">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <InputText @bind-Value="request.Rapper1Id" />
    <ValidationMessage For="@(() => request.Rapper1Id)" />
    
    <button type="submit">Start Debate</button>
</EditForm>
```

---

## Immutability Pattern

Most DTOs use `init` accessors for immutability:

```csharp
// ✅ GOOD: Immutable after construction
public class DebateResponse
{
    public required string DebateId { get; init; }  // Cannot be changed after creation
}

var response = new DebateResponse { DebateId = "abc123" };
// response.DebateId = "xyz789";  // ❌ Compile error

// ❌ BAD: Mutable (unless mutability is needed)
public class DebateResponse
{
    public required string DebateId { get; set; }  // Can be changed anytime
}
```

**When to use `set`**:
- Request DTOs that need modification (e.g., form binding)
- DTOs that represent mutable state

**When to use `init`**:
- Response DTOs (server → client)
- Domain models (database entities)
- Any DTO that represents a snapshot in time

---

## JSON Serialization

All models are designed for JSON serialization:

```csharp
// Automatic JSON serialization
var json = JsonSerializer.Serialize(rapper);
var rapper = JsonSerializer.Deserialize<Rapper>(json);

// HTTP JSON extensions
var response = await Http.PostAsJsonAsync("/api/debate/start", request);
var debate = await response.Content.ReadFromJsonAsync<DebateResponse>();
```

**Property Naming**:
- C# properties use PascalCase (`RapperId`)
- JSON uses camelCase (`rapperId`) - automatic conversion

---

## Dependencies

**NuGet Packages**:
- `System.ComponentModel.Annotations` (for validation attributes)

**No other dependencies** - keeps the shared library lightweight.

---

## Best Practices

### 1. Use `required` for Non-Nullable Properties
```csharp
// ✅ GOOD
public required string Name { get; init; }

// ❌ BAD
public string Name { get; init; }  // CS8618 warning
```

### 2. Use `init` for Immutable Data
```csharp
// ✅ GOOD: Response DTOs
public required string DebateId { get; init; }

// ✅ ALSO GOOD: Request DTOs (need set for binding)
public required string RapperId { get; set; }
```

### 3. Validate at the Edges
```csharp
// ✅ GOOD: Validate in request DTOs
public class DebateRequest
{
    [Required]
    [MinLength(10)]
    public required string Topic { get; set; }
}

// ❌ BAD: No validation
public class DebateRequest
{
    public required string Topic { get; set; }  // No constraints
}
```

### 4. Keep DTOs Simple
```csharp
// ✅ GOOD: Data container only
public class Rapper
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}

// ❌ BAD: Business logic in DTO
public class Rapper
{
    public required string Id { get; init; }
    
    public void DoSomething()  // ❌ Don't put methods in DTOs
    {
        // Business logic
    }
}
```

---

## Adding New Models

### Checklist

1. **Create model in `Shared/Models/`**:
   ```csharp
   public class NewModel
   {
       public required string Id { get; init; }
       public required string Name { get; init; }
   }
   ```

2. **Add validation attributes** (if request DTO):
   ```csharp
   [Required]
   [MinLength(1)]
   public required string Name { get; set; }
   ```

3. **Use in Server** (controller/service):
   ```csharp
   [HttpPost]
   public async Task<IActionResult> Create([FromBody] NewModel model)
   {
       // Process
   }
   ```

4. **Use in Client** (component):
   ```csharp
   var response = await Http.PostAsJsonAsync("/api/endpoint", model);
   var result = await response.Content.ReadFromJsonAsync<NewModel>();
   ```

5. **Document in this README**

---

## Model Relationships

```
Rapper ──┐
         ├──> DebateRequest ──> DebateResponse ──> DebateTurn ──> JudgmentResponse
Rapper ──┘                                             │
                                                       └──> DebateStats
Topic ────────────────────────────────────────────────┘

NewsHeadline ──> Topic (source)

VerseRequest ──> VerseResponse ──> DebateTurn (verse + audio)

HealthStatus ──> DiagnosticResult
```

---

**Last Updated**: October 28, 2025
