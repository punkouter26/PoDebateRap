# AGENTS.MD - Machine-Readable Documentation for AI Coding Agents

> **Purpose**: This document provides structured, actionable information for AI coding assistants (LLMs) working on the PoDebateRap codebase. It documents build commands, project structure, code style conventions, and common pitfalls.

## 📋 Table of Contents
1. [Quick Reference](#quick-reference)
2. [Build & Test Commands](#build--test-commands)
3. [Project Structure](#project-structure)
4. [Code Style & Conventions](#code-style--conventions)
5. [Architecture Patterns](#architecture-patterns)
6. [Security & Development Gotchas](#security--development-gotchas)
7. [Azure Services Integration](#azure-services-integration)
8. [Testing Strategy](#testing-strategy)

---

## 🚀 Quick Reference

### Core Constraints
- **.NET Version**: **9.0** (MUST match major version, builds fail otherwise)
- **Ports**: HTTP **5000**, HTTPS **5001** (hardcoded, non-negotiable)
- **Naming Convention**: All projects MUST use `Po.AppName.*` prefix exactly
- **Storage**: Azure Table Storage (production), Azurite (local development)
- **Testing Framework**: xUnit for Unit/Integration, Playwright MCP for E2E
- **Code Style**: Use `dotnet format`, max **500 lines per file**
- **Task Automation**: Use **one-line CLI commands only**

### Essential Commands
```bash
# Build entire solution
dotnet build

# Run the application (API + Blazor WASM)
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Run all tests (excluding E2E)
dotnet test --filter "FullyQualifiedName!~SystemTests"

# Run only unit tests
dotnet test Tests/PoDebateRap.UnitTests/PoDebateRap.UnitTests.csproj

# Run integration tests (requires Azure credentials)
dotnet test Tests/PoDebateRap.IntegrationTests/PoDebateRap.IntegrationTests.csproj

# Format code
dotnet format

# Deploy to Azure
azd up
```

---

## 🔨 Build & Test Commands

### Building

```bash
# Restore dependencies
dotnet restore

# Build Debug configuration
dotnet build

# Build Release configuration
dotnet build --configuration Release

# Clean build artifacts
dotnet clean

# Publish for deployment
dotnet publish Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj -c Release -o ./publish
```

### Testing

```bash
# Run all tests with verbose output
dotnet test --verbosity normal

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# Run specific test project
dotnet test Tests/PoDebateRap.UnitTests/PoDebateRap.UnitTests.csproj

# Run specific test method
dotnet test --filter "FullyQualifiedName~PoDebateRap.UnitTests.DebateOrchestratorTests.GenerateVerse_ShouldReturnValidResponse"

# Run E2E tests (requires server running on localhost:5000)
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj
```

### Local Development

```bash
# Start Azurite (Azure Storage emulator)
azurite --silent --location ./AzuriteConfig --debug ./AzuriteConfig/debug.log

# Run with watch mode (auto-rebuild on changes)
dotnet watch run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Check for outdated packages
dotnet list package --outdated

# Update a specific package
dotnet add package <PackageName>
```

### Azure Deployment

```bash
# Provision infrastructure + deploy application
azd up

# Provision infrastructure only
azd provision

# Deploy application only
azd deploy

# Configure CI/CD pipeline
azd pipeline config

# View deployed application
azd browse
```

---

## 📁 Project Structure

### Solution Layout

```
PoDebateRap.sln                    # Solution file
├── Client/
│   └── PoDebateRap.Client/        # Blazor WebAssembly client
├── Server/
│   └── PoDebateRap.ServerApi/     # ASP.NET Core Web API
├── Shared/
│   └── PoDebateRap.Shared/        # Shared DTOs and models
├── Tests/
│   ├── PoDebateRap.UnitTests/     # Unit tests (mocked dependencies)
│   ├── PoDebateRap.ApiTests/      # API endpoint tests
│   ├── PoDebateRap.IntegrationTests/ # Azure service integration tests
│   └── PoDebateRap.SystemTests/   # E2E Playwright tests
├── infra/                         # Bicep infrastructure as code
├── DOCS/                          # KQL queries and documentation
└── Diagrams/                      # Architecture diagrams (SVG)
```

### Project Dependencies

```
PoDebateRap.ServerApi
├── → PoDebateRap.Client (hosted inside API)
├── → PoDebateRap.Shared
└── Azure SDK packages (OpenAI, Speech, Storage, ApplicationInsights)

PoDebateRap.Client
├── → PoDebateRap.Shared
└── Blazor WebAssembly packages

PoDebateRap.Shared
└── System.ComponentModel.DataAnnotations (for validation)

PoDebateRap.*Tests
├── → PoDebateRap.ServerApi (Integration/API tests)
└── xUnit, Moq, FluentAssertions
```

### Key Directories

| Directory | Purpose | Key Files |
|-----------|---------|-----------|
| `Server/PoDebateRap.ServerApi/Controllers/` | API endpoints | `DebateController.cs`, `DiagnosticsController.cs` |
| `Server/PoDebateRap.ServerApi/Services/` | Business logic | AI, Speech, Data, Orchestration, News |
| `Server/PoDebateRap.ServerApi/Hubs/` | SignalR real-time | `DebateHub.cs` |
| `Client/PoDebateRap.Client/Components/` | Blazor components | Debate, Diagnostics, Layout, Pages |
| `Shared/Models/` | Shared DTOs | `Rapper.cs`, `Topic.cs`, `DebateRequests.cs` |
| `infra/` | Infrastructure | `main.bicep`, `resources.bicep` |
| `DOCS/` | Documentation | `KQL_QUERIES.md` |

---

## 🎨 Code Style & Conventions

### General Rules

1. **File Length**: Maximum **500 lines per file**
   - If a file exceeds 500 lines, refactor into smaller, focused classes
   - Use partial classes if necessary for generated code

2. **Naming Conventions**:
   - **Projects**: `Po.AppName.ProjectType` (e.g., `PoDebateRap.ServerApi`)
   - **Namespaces**: Match folder structure exactly
   - **Classes**: PascalCase, descriptive names (`DebateOrchestrator`, `AzureOpenAIService`)
   - **Methods**: PascalCase, verb-based (`GenerateVerse`, `GetRapperById`)
   - **Variables**: camelCase for locals, PascalCase for properties
   - **Constants**: UPPER_SNAKE_CASE or PascalCase with `const` keyword

3. **Code Formatting**:
   - Use `dotnet format` before committing
   - 4 spaces indentation (no tabs)
   - Opening braces on new line (Allman style)
   - Max 120 characters per line

### C# Specific Conventions

```csharp
// ✅ GOOD: Nullable reference types enabled, required properties
public class Rapper
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Style { get; init; }
    public string? ImageUrl { get; init; }  // Nullable for optional properties
}

// ❌ BAD: No required modifiers, allows null
public class Rapper
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// ✅ GOOD: Dependency injection via constructor
public class DebateOrchestrator
{
    private readonly IAzureOpenAIService _aiService;
    private readonly ILogger<DebateOrchestrator> _logger;

    public DebateOrchestrator(
        IAzureOpenAIService aiService,
        ILogger<DebateOrchestrator> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }
}

// ❌ BAD: Service locator anti-pattern
public class DebateOrchestrator
{
    public async Task DoSomething()
    {
        var aiService = ServiceLocator.Get<IAzureOpenAIService>();
    }
}

// ✅ GOOD: Async methods with proper naming and cancellation
public async Task<VerseResponse> GenerateVerseAsync(
    string rapperId,
    string topic,
    CancellationToken cancellationToken = default)
{
    // Implementation
}

// ❌ BAD: Synchronous wrapper around async code
public VerseResponse GenerateVerse(string rapperId, string topic)
{
    return GenerateVerseAsync(rapperId, topic).Result; // Deadlock risk!
}
```

### Blazor Component Conventions

```razor
@* ✅ GOOD: Clear component structure *@
@page "/debate"
@inject IDebateService DebateService
@inject ILogger<DebateArena> Logger

<div class="debate-arena">
    @if (isLoading)
    {
        <p>Loading...</p>
    }
    else
    {
        <h1>@debate.Topic</h1>
    }
</div>

@code {
    private bool isLoading = true;
    private Debate? debate;

    protected override async Task OnInitializedAsync()
    {
        debate = await DebateService.GetCurrentDebateAsync();
        isLoading = false;
    }
}
```

### File Organization

```
ServiceName.cs
├── Using statements (grouped: System → Third-party → Local)
├── Namespace declaration
├── Public interfaces (if applicable)
├── Public classes
│   ├── Constants
│   ├── Fields (private, readonly)
│   ├── Constructor(s)
│   ├── Public properties
│   ├── Public methods
│   ├── Protected methods
│   └── Private methods
└── Internal/private helper classes (if needed)
```

---

## 🏛️ Architecture Patterns

### Architectural Style
**Vertical Slice Architecture** with Clean Architecture boundaries where complexity requires separation.

### Core Principles

1. **SOLID Principles**:
   - **S**ingle Responsibility: Each class has one reason to change
   - **O**pen/Closed: Open for extension, closed for modification
   - **L**iskov Substitution: Subtypes must be substitutable for base types
   - **I**nterface Segregation: Many specific interfaces > one general interface
   - **D**ependency Inversion: Depend on abstractions, not concretions

2. **Dependency Injection**:
   - All services registered in `Program.cs`
   - Constructor injection preferred
   - Scoped lifetime for services that maintain state per request
   - Singleton for stateless services
   - Transient for lightweight, stateless services

3. **Repository Pattern**:
   - Abstract data access behind interfaces (`IRapperRepository`, `IDebateRepository`)
   - Implementations use Azure Table Storage
   - Testable via mocking

### Service Layer Organization

```
Services/
├── AI/
│   ├── IAzureOpenAIService.cs         # Interface
│   └── AzureOpenAIService.cs          # Implementation
├── Speech/
│   ├── ITextToSpeechService.cs
│   └── TextToSpeechService.cs
├── Data/
│   ├── ITableStorageService.cs
│   ├── TableStorageService.cs
│   ├── IRapperRepository.cs
│   └── RapperRepository.cs
├── Orchestration/
│   ├── IDebateOrchestrator.cs
│   └── DebateOrchestrator.cs
├── News/
│   ├── INewsService.cs
│   └── NewsService.cs
└── Telemetry/
    └── CustomTelemetryService.cs
```

### Dependency Registration Pattern

```csharp
// Program.cs
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IRapperRepository, RapperRepository>();
builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
builder.Services.AddScoped<IDebateOrchestrator, DebateOrchestrator>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<CustomTelemetryService>();
```

---

## ⚠️ Security & Development Gotchas

### Configuration Management

**DO**:
- ✅ Use **User Secrets** for local development API keys
- ✅ Use **Azure App Service Application Settings** for production
- ✅ Store non-sensitive defaults in `appsettings.json`
- ✅ Use environment-specific files (`appsettings.Development.json`)

**DON'T**:
- ❌ Commit API keys to Git (even in private repos as best practice)
- ❌ Hardcode connection strings in code
- ❌ Use production credentials locally

### User Secrets Setup (Local Development)

```bash
# Set Azure OpenAI credentials
dotnet user-secrets set "Azure:OpenAI:Endpoint" "https://your-resource.openai.azure.com/" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
dotnet user-secrets set "Azure:OpenAI:ApiKey" "your-api-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
dotnet user-secrets set "Azure:OpenAI:DeploymentName" "gpt-4" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Set Azure Speech credentials
dotnet user-secrets set "Azure:Speech:Region" "eastus" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
dotnet user-secrets set "Azure:Speech:SubscriptionKey" "your-speech-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Use Azurite for local storage
dotnet user-secrets set "Azure:StorageConnectionString" "UseDevelopmentStorage=true" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# Set NewsAPI key
dotnet user-secrets set "NewsApi:ApiKey" "your-newsapi-key" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
```

### Common Pitfalls

#### 1. **Nullable Reference Types**
```csharp
// ❌ BAD: Compiler warning CS8618 (non-nullable property not initialized)
public class Rapper
{
    public string Id { get; set; }
}

// ✅ GOOD: Use 'required' modifier (C# 11+)
public class Rapper
{
    public required string Id { get; init; }
}

// ✅ ALSO GOOD: Nullable for optional properties
public class Rapper
{
    public string? ImageUrl { get; init; }  // Allows null
}
```

#### 2. **Async/Await Deadlocks**
```csharp
// ❌ BAD: Blocking on async code
public void DoSomething()
{
    var result = SomeAsyncMethod().Result;  // DEADLOCK RISK!
}

// ✅ GOOD: Async all the way
public async Task DoSomethingAsync()
{
    var result = await SomeAsyncMethod();
}
```

#### 3. **SignalR Hub Context**
```csharp
// ❌ BAD: Storing IHubContext as instance field
private IHubContext<DebateHub> _hubContext;

public async Task SendUpdate()
{
    await _hubContext.Clients.All.SendAsync("DebateUpdate", data);  // May not work as expected
}

// ✅ GOOD: Inject IHubContext where needed
public async Task SendUpdate(IHubContext<DebateHub> hubContext)
{
    await hubContext.Clients.All.SendAsync("DebateUpdate", data);
}
```

#### 4. **Azure Table Storage Partition/Row Keys**
```csharp
// ❌ BAD: Using GUIDs as partition keys (poor performance)
var entity = new TableEntity("a1b2c3d4-...", "row-key");

// ✅ GOOD: Use logical grouping for partition keys
var entity = new TableEntity("Rappers", "eminem");
var entity = new TableEntity($"Debate-{debateId}", $"Turn-{turnNumber}");
```

#### 5. **Blazor State Management**
```csharp
// ❌ BAD: Forgetting to call StateHasChanged
private string message = "Loading...";

public async Task LoadData()
{
    var data = await Service.GetDataAsync();
    message = "Loaded!";
    // UI won't update!
}

// ✅ GOOD: Explicitly refresh UI
public async Task LoadData()
{
    var data = await Service.GetDataAsync();
    message = "Loaded!";
    StateHasChanged();  // Force UI refresh
}
```

### Port Configuration

**CRITICAL**: Ports are **hardcoded** in the project constraints:
- HTTP: **5000**
- HTTPS: **5001**

If you need to change ports for any reason, you MUST update:
1. `Server/PoDebateRap.ServerApi/Properties/launchSettings.json`
2. Client-side API base URLs (if hardcoded)
3. Any E2E test configurations
4. CORS configuration in `Program.cs`

---

## ☁️ Azure Services Integration

### Required Azure Services

| Service | Purpose | Configuration Key |
|---------|---------|-------------------|
| **Azure OpenAI** | GPT-4 for rap verse generation | `Azure:OpenAI:Endpoint`, `Azure:OpenAI:ApiKey`, `Azure:OpenAI:DeploymentName` |
| **Azure Speech Services** | Neural text-to-speech | `Azure:Speech:Region`, `Azure:Speech:SubscriptionKey` |
| **Azure Table Storage** | Rapper profiles, debate history | `Azure:StorageConnectionString` |
| **Application Insights** | Telemetry and monitoring | `APPLICATIONINSIGHTS_CONNECTION_STRING` |

### Health Checks

The application implements health checks for all Azure dependencies:

```csharp
// Endpoints:
// GET /api/health/status - Overall health status
// GET /api/health/openai - Azure OpenAI health
// GET /api/health/speech - Azure Speech health
// GET /api/health/storage - Azure Storage health
// GET /api/health/news - NewsAPI health
```

**Health Check Response Format**:
```json
{
  "status": "Healthy" | "Degraded" | "Unhealthy",
  "service": "Azure OpenAI",
  "message": "Service is operational",
  "timestamp": "2025-10-28T12:34:56Z",
  "details": { /* service-specific metadata */ }
}
```

### Telemetry and Logging

**Logging Framework**: Serilog with Application Insights sink

**Log Levels**:
- `Trace`: Very detailed diagnostic information
- `Debug`: Internal system events for debugging
- `Information`: General informational messages (default minimum)
- `Warning`: Unexpected events that don't cause failures
- `Error`: Errors and exceptions
- `Critical`: Critical failures requiring immediate attention

**Custom Telemetry Events** (via `CustomTelemetryService`):
- `DebateStarted` - Debate initiation with rapper names, topic
- `DebateCompleted` - Debate completion with winner, duration, turn count
- `AIModelUsage` - AI model calls with token count, response time
- `TextToSpeechUsage` - TTS calls with character count, generation time
- `StorageOperation` - Table Storage operations with duration
- `Error` - Structured exception tracking

**KQL Queries**: See `DOCS/KQL_QUERIES.md` for essential monitoring queries.

---

## 🧪 Testing Strategy

### Test Types

1. **Unit Tests** (`PoDebateRap.UnitTests`):
   - Test business logic in isolation
   - Mock all external dependencies (Azure services, repositories)
   - Fast execution (< 1 second per test)
   - Example: `DebateOrchestratorTests`, `NewsServiceTests`

2. **API Tests** (`PoDebateRap.ApiTests`):
   - Test HTTP endpoints and request/response handling
   - Validate routing, model binding, status codes
   - Use WebApplicationFactory for in-memory testing

3. **Integration Tests** (`PoDebateRap.IntegrationTests`):
   - Test real Azure service integrations
   - Requires valid Azure credentials in user secrets
   - Slower execution (network calls)
   - Example: `AzureOpenAIServiceIntegrationTests`, `TableStorageServiceIntegrationTests`

4. **System/E2E Tests** (`PoDebateRap.SystemTests`):
   - Test full user flows with Playwright
   - Requires running server on localhost:5000
   - Browser automation tests
   - **Manually executed** - excluded from CI/CD

### Test-Driven Development (TDD) Workflow

```bash
# 1. Write a failing test
# Create test file: Tests/PoDebateRap.UnitTests/NewFeatureTests.cs

# 2. Run the test (it should fail)
dotnet test Tests/PoDebateRap.UnitTests --filter "FullyQualifiedName~NewFeatureTests"

# 3. Implement the feature
# Edit: Server/PoDebateRap.ServerApi/Services/NewFeature.cs

# 4. Run the test again (it should pass)
dotnet test Tests/PoDebateRap.UnitTests --filter "FullyQualifiedName~NewFeatureTests"

# 5. Refactor if needed, re-run tests
```

### Test Isolation for Integration Tests

**CRITICAL**: Integration tests must use isolated, disposable test tables/resources.

```csharp
// ✅ GOOD: Isolated test table with cleanup
[Fact]
public async Task TestTableOperation()
{
    var testTableName = $"TestTable{Guid.NewGuid():N}";
    var tableClient = new TableClient(connectionString, testTableName);
    
    try
    {
        await tableClient.CreateIfNotExistsAsync();
        
        // Perform test operations
        
        Assert.True(/* test assertion */);
    }
    finally
    {
        await tableClient.DeleteAsync();  // CLEANUP
    }
}

// ❌ BAD: Using shared production table
[Fact]
public async Task TestTableOperation()
{
    var tableClient = new TableClient(connectionString, "Rappers");  // Production data!
    // Test operations that modify production data
}
```

### Mocking Best Practices

```csharp
// ✅ GOOD: Clear, focused mocking
var mockAIService = new Mock<IAzureOpenAIService>();
mockAIService
    .Setup(x => x.GenerateVerseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new VerseResponse { Verse = "Test verse", Success = true });

var orchestrator = new DebateOrchestrator(mockAIService.Object, mockLogger.Object);

// ❌ BAD: Over-mocking, tight coupling to implementation
var mockAIService = new Mock<IAzureOpenAIService>(MockBehavior.Strict);
mockAIService
    .Setup(x => x.GenerateVerseAsync("eminem", "test topic", default))
    .ReturnsAsync(new VerseResponse { Verse = "Specific verse", Success = true });
// Test breaks if you pass different parameters
```

### E2E Test Execution

```bash
# 1. Start the server
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# 2. In a separate terminal, run E2E tests
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj

# Note: E2E tests are excluded from CI/CD and run manually
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

**File**: `.github/workflows/azure-dev.yml`

**Trigger**: Push to `main` branch

**Jobs**:
1. **Build**:
   - Checkout code
   - Setup .NET 9.0
   - Restore dependencies
   - Build in Release configuration
   - Publish application
   - Upload artifacts

2. **Test**:
   - Run unit tests
   - Run integration tests (with Azure credentials)
   - Integration tests may be skipped if credentials unavailable

3. **Deploy**:
   - Log in to Azure (federated credentials)
   - Provision infrastructure (Bicep)
   - Deploy application to Azure App Service

### Post-Deployment Verification Checklist

After deployment, verify:

- [ ] **Health Check**: `GET https://podebaterap.azurewebsites.net/api/health/status` returns HTTP 200
- [ ] **Swagger UI**: `https://podebaterap.azurewebsites.net/swagger` is accessible
- [ ] **Home Page**: `https://podebaterap.azurewebsites.net/` loads with correct title "PoDebateRap"
- [ ] **App Service Plan**: Verify using F1 tier from PoShared resource group
- [ ] **Application Insights**: Telemetry flowing to `appi-PoDebateRap-*`
- [ ] **HTTPS Redirect**: HTTP requests redirect to HTTPS

### Manual Deployment Commands

```bash
# Full deployment (provision + deploy)
azd up

# Just infrastructure provisioning
azd provision

# Just application deployment
azd deploy

# View deployment logs
azd show

# Open deployed application
azd browse
```

---

## 📊 Monitoring and Observability

### Application Insights Queries

See `DOCS/KQL_QUERIES.md` for detailed queries. Quick reference:

```kql
// User activity (last 7 days)
customEvents
| where timestamp > ago(7d)
| summarize Users = dcount(user_Id), Sessions = dcount(session_Id) by bin(timestamp, 1d)

// Error rate (last 24 hours)
requests
| where timestamp > ago(24h)
| summarize Total = count(), Failed = countif(success == false)
| extend ErrorRate = round(todouble(Failed) / todouble(Total) * 100, 2)

// Slowest requests (P95)
requests
| where timestamp > ago(1d)
| summarize P95 = percentile(duration, 95) by name
| top 10 by P95 desc
```

### Health Dashboard

Navigate to `/diag` for:
- Service health checks (OpenAI, Speech, Storage, News)
- Audio playback test
- First 50 bytes inspection (verify RIFF header)

---

## 🎯 Common Development Tasks

### Adding a New Azure Service

1. Add NuGet package:
   ```bash
   dotnet add Server/PoDebateRap.ServerApi package Azure.SomeService
   ```

2. Create service interface and implementation:
   ```csharp
   // Services/ISomeService.cs
   public interface ISomeService
   {
       Task<Result> DoSomethingAsync(CancellationToken cancellationToken = default);
   }
   
   // Services/SomeService.cs
   public class SomeService : ISomeService
   {
       private readonly ILogger<SomeService> _logger;
       
       public SomeService(ILogger<SomeService> logger)
       {
           _logger = logger;
       }
       
       public async Task<Result> DoSomethingAsync(CancellationToken cancellationToken = default)
       {
           // Implementation
       }
   }
   ```

3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<ISomeService, SomeService>();
   ```

4. Add configuration keys:
   ```bash
   dotnet user-secrets set "Azure:SomeService:Endpoint" "value" --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   ```

5. Add health check:
   ```csharp
   [HttpGet("someservice")]
   public async Task<IActionResult> CheckSomeService()
   {
       // Health check logic
   }
   ```

### Adding a New API Endpoint

1. Create/update controller:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class MyController : ControllerBase
   {
       [HttpGet("{id}")]
       [ProducesResponseType(typeof(MyResponse), StatusCodes.Status200OK)]
       [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
       public async Task<IActionResult> Get(string id)
       {
           // Implementation
       }
   }
   ```

2. Add request/response DTOs in `Shared/Models/`

3. Write unit tests in `Tests/PoDebateRap.UnitTests/`

4. Update Swagger documentation (automatic via attributes)

### Adding a New Blazor Component

1. Create component file: `Client/PoDebateRap.Client/Components/MyComponent.razor`

2. Follow Blazor conventions:
   ```razor
   @inject IMyService MyService
   
   <div class="my-component">
       @if (isLoading)
       {
           <p>Loading...</p>
       }
       else
       {
           <p>@data</p>
       }
   </div>
   
   @code {
       [Parameter]
       public string? Title { get; set; }
       
       private bool isLoading = true;
       private string? data;
       
       protected override async Task OnInitializedAsync()
       {
           data = await MyService.GetDataAsync();
           isLoading = false;
       }
   }
   ```

3. Reference in parent component:
   ```razor
   <MyComponent Title="Hello" />
   ```

---

## 🐛 Debugging Tips

### Serilog Structured Logging

```csharp
// ✅ GOOD: Structured logging with properties
_logger.LogInformation(
    "Debate started between {Rapper1} and {Rapper2} on topic {Topic}",
    rapper1.Name,
    rapper2.Name,
    topic);

// ❌ BAD: String interpolation (not structured)
_logger.LogInformation($"Debate started between {rapper1.Name} and {rapper2.Name}");
```

### Debugging Blazor Components

1. Use browser DevTools (F12)
2. Check Console for errors
3. Inspect Network tab for API calls
4. Use `@* Debug output *@` comments in Razor files
5. Add breakpoints in `@code` blocks

### Debugging Azure Service Issues

1. Check Application Insights Live Metrics
2. Review `Server/log.txt` (Serilog file sink)
3. Verify configuration keys are set
4. Test health check endpoints (`/api/health/*`)
5. Use Azurite logs for local storage debugging

---

## 📚 Additional Resources

- **.NET 9 Documentation**: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9
- **Blazor WebAssembly**: https://learn.microsoft.com/en-us/aspnet/core/blazor/
- **Azure SDK for .NET**: https://learn.microsoft.com/en-us/dotnet/azure/
- **xUnit Testing**: https://xunit.net/
- **Serilog**: https://serilog.net/
- **Azure Developer CLI**: https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/

---

**Last Updated**: October 28, 2025 - Phase 6 Documentation
**Maintained By**: AI Coding Agents & Human Developers
