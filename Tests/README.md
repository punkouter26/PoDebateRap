# PoDebateRap Tests

Comprehensive test suite for the PoDebateRap application, covering unit tests, API tests, integration tests, and end-to-end system tests.

## Project Overview

- **Framework**: xUnit (Unit/Integration), Playwright (E2E)
- **.NET Version**: 9.0
- **Test Philosophy**: Test-Driven Development (TDD)
- **Coverage**: Business logic, API endpoints, Azure services, full user flows

## Test Projects

### 1. PoDebateRap.UnitTests

**Purpose**: Test business logic in isolation with mocked dependencies.

**Coverage**:
- Service layer logic
- Repository implementations (with mocked storage)
- Orchestration flows
- Edge cases and error handling

**Key Test Files**:
- `DebateOrchestratorTests.cs` - Debate orchestration logic
- `NewsServiceTests.cs` - News API integration (mocked)
- `RapperRepositoryTests.cs` - Repository pattern tests

**Run Command**:
```bash
dotnet test Tests/PoDebateRap.UnitTests
```

**Example Test**:
```csharp
[Fact]
public async Task GenerateVerse_ShouldReturnValidResponse()
{
    // Arrange
    var mockAIService = new Mock<IAzureOpenAIService>();
    mockAIService
        .Setup(x => x.GenerateVerseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new VerseResponse
        {
            Verse = "Test verse with rhymes and flow",
            Success = true,
            TokenCount = 50
        });
    
    var orchestrator = new DebateOrchestrator(mockAIService.Object, mockLogger.Object);
    
    // Act
    var result = await orchestrator.GenerateVerseAsync("eminem", "test topic");
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Contains("verse", result.Verse);
}
```

---

### 2. PoDebateRap.ApiTests

**Purpose**: Test HTTP endpoints and controller logic.

**Coverage**:
- Request routing
- Model binding
- Status code validation
- Response serialization

**Key Test Files**:
- `DebateControllerTests.cs` - Debate endpoints
- `DiagnosticsControllerTests.cs` - Health check endpoints

**Run Command**:
```bash
dotnet test Tests/PoDebateRap.ApiTests
```

**Example Test**:
```csharp
[Fact]
public async Task StartDebate_WithValidRequest_ReturnsOk()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var request = new DebateRequest
    {
        Rapper1Id = "eminem",
        Rapper2Id = "tupac",
        Topic = "Is social media good for society?"
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/debate/start", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var debate = await response.Content.ReadFromJsonAsync<DebateResponse>();
    Assert.NotNull(debate);
    Assert.NotEmpty(debate.DebateId);
}

[Fact]
public async Task StartDebate_WithInvalidRequest_ReturnsBadRequest()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var request = new DebateRequest
    {
        Rapper1Id = "eminem",
        Rapper2Id = "eminem",  // ❌ Same rapper
        Topic = "Short"        // ❌ Too short
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/debate/start", request);
    
    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

### 3. PoDebateRap.IntegrationTests

**Purpose**: Test real Azure service integrations (OpenAI, Speech, Storage, NewsAPI).

**Coverage**:
- Azure OpenAI verse generation
- Azure Speech TTS audio synthesis
- Azure Table Storage CRUD operations
- NewsAPI headline fetching

**Requirements**:
- Valid Azure credentials in user secrets
- Azurite running for local storage tests
- Internet connection for external API calls

**Key Test Files**:
- `AzureOpenAIServiceIntegrationTests.cs`
- `TextToSpeechServiceIntegrationTests.cs`
- `TableStorageServiceIntegrationTests.cs`
- `NewsServiceIntegrationTests.cs`
- `HealthEndpointIntegrationTests.cs`

**Run Command**:
```bash
# Ensure Azurite is running
azurite --silent --location ./AzuriteConfig

# Run integration tests
dotnet test Tests/PoDebateRap.IntegrationTests
```

**Configuration** (`appsettings.test.json`):
```json
{
  "Azure": {
    "OpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "DeploymentName": "gpt-4"
    },
    "Speech": {
      "Region": "eastus"
    },
    "StorageConnectionString": "UseDevelopmentStorage=true"
  },
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2"
  }
}
```

**Example Test**:
```csharp
[Fact]
public async Task GenerateVerse_WithValidInput_ReturnsAudio()
{
    // Arrange
    var service = new AzureOpenAIService(configuration, logger);
    
    // Act
    var result = await service.GenerateVerseAsync("eminem", "Is AI good for humanity?");
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.NotEmpty(result.Verse);
    Assert.True(result.TokenCount > 0);
    Assert.Contains("AI", result.Verse, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public async Task TableStorage_CreateAndRetrieve_Succeeds()
{
    // Arrange
    var tableName = $"TestTable{Guid.NewGuid():N}";
    var tableClient = new TableClient(connectionString, tableName);
    
    try
    {
        await tableClient.CreateIfNotExistsAsync();
        
        var entity = new TableEntity("TestPartition", "TestRow")
        {
            ["Name"] = "Test Rapper"
        };
        
        // Act
        await tableClient.AddEntityAsync(entity);
        var retrieved = await tableClient.GetEntityAsync<TableEntity>("TestPartition", "TestRow");
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test Rapper", retrieved.Value["Name"]);
    }
    finally
    {
        await tableClient.DeleteAsync();  // CLEANUP
    }
}
```

**CRITICAL**: Integration tests MUST use isolated test resources and clean up after themselves.

---

### 4. PoDebateRap.E2ETests (TypeScript Playwright)

**Purpose**: Comprehensive end-to-end browser automation tests using TypeScript and Playwright.

**Coverage**:
- Full user flows (debate setup → execution → results)
- Audio playback functionality
- UI interactions and responsiveness
- Real-time SignalR updates

**Requirements**:
- Server running on `http://localhost:5000`
- Playwright browsers installed

**Key Test Files**:
- `DebateSetupTests.cs` - UI setup flow
- `AudioPlaybackTests.cs` - Audio functionality (see `AUDIO_TESTING.md`)
- `FULL_DEBATE_FLOW_TESTS.md` - Test scenarios documentation

**Audio Testing Documentation**: See `AUDIO_TESTING.md` for comprehensive guide to audio playback E2E tests.

---

### 5. PoDebateRap.E2ETests (E2E - TypeScript)

**Purpose**: Comprehensive TypeScript Playwright E2E tests covering all main UI functionality.

**Coverage**:
- Debate setup form validation (desktop & mobile)
- Full debate flow (initialization, verses, audio)
- Audio playback and controls
- Diagnostics/health check page
- Mobile responsiveness and touch interactions
- Desktop and mobile viewports (Chromium only)

**Requirements**:
- Node.js 18+ and npm
- Playwright for Node
- Server running on `http://localhost:5000`

**Key Test Files**:
- `tests/debate-setup.spec.ts` - Form validation and setup
- `tests/debate-flow.spec.ts` - Complete debate flows
- `tests/audio-playback.spec.ts` - Audio generation and playback
- `tests/diagnostics.spec.ts` - Health check page
- `tests/helpers/page-objects.ts` - Page Object Models

**Documentation**: See `PoDebateRap.E2ETests/README.md` for detailed setup and usage.

**Run Command**:
```bash
# 1. Start the server
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# 2. In a separate terminal, run C# E2E tests
dotnet test Tests/PoDebateRap.SystemTests
```

**Installation**:
```bash
# Install Playwright browsers
pwsh Tests/PoDebateRap.SystemTests/bin/Debug/net9.0/playwright.ps1 install
```

**Example Test**:
```csharp
[Fact]
public async Task DebateSetup_SelectRappers_EnablesStartButton()
{
    // Arrange
    await using var browser = await playwright.Chromium.LaunchAsync();
    await using var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();
    
    // Act
    await page.GotoAsync("http://localhost:5000");
    
    // Select Rapper 1
    await page.SelectOptionAsync("#rapper1-select", "eminem");
    
    // Select Rapper 2
    await page.SelectOptionAsync("#rapper2-select", "tupac");
    
    // Enter topic
    await page.FillAsync("#topic-input", "Is AI a threat to humanity?");
    
    // Assert
    var startButton = await page.QuerySelectorAsync("#start-debate-button");
    Assert.NotNull(startButton);
    
    var isEnabled = await startButton.IsEnabledAsync();
    Assert.True(isEnabled);
}

[Fact]
public async Task AudioPlayback_TestButton_PlaysSound()
{
    // Arrange
    await using var browser = await playwright.Chromium.LaunchAsync();
    await using var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();
    
    // Act
    await page.GotoAsync("http://localhost:5000/diag");
    await page.ClickAsync("#test-audio-button");
    
    // Wait for audio element to appear
    await page.WaitForSelectorAsync("audio", new() { Timeout = 5000 });
    
    // Assert
    var audioElement = await page.QuerySelectorAsync("audio");
    Assert.NotNull(audioElement);
    
    var src = await audioElement.GetAttributeAsync("src");
    Assert.StartsWith("blob:", src);
}
```

**Note**: C# E2E tests (SystemTests) are **manually executed** and excluded from CI/CD.

**Run Command**:
```bash
cd Tests/PoDebateRap.E2ETests
npm install
npx playwright install chromium
npm test
```

**Features**:
- ✅ Page Object Model pattern
- ✅ Desktop viewport (1280x720)
- ✅ Mobile viewport (Pixel 5 - 393x851)
- ✅ Chromium only for consistency
- ✅ Auto-start server via webServer config
- ✅ Screenshot/video on failure
- ✅ HTML test reports

**Test Tags**:
- `@desktop` - Desktop viewport tests
- `@mobile` - Mobile viewport tests

```bash
# Run only desktop tests
npm run test:desktop

# Run only mobile tests
npm run test:mobile

# Run in headed mode (see browser)
npm run test:headed

# Open interactive UI
npm run test:ui
```

**Note**: TypeScript E2E tests can be integrated into CI/CD or run manually.

---

## Test Isolation

### Unit Tests
- ✅ **Fully isolated** - all dependencies mocked
- ✅ No network calls
- ✅ No database access
- ✅ Fast execution (< 1 second per test)

### Integration Tests
- ⚠️ **Requires real Azure services**
- ✅ Uses isolated test tables (created/deleted per test)
- ✅ No shared state between tests
- ⏱️ Slower execution (network latency)

### E2E Tests
- ⚠️ **Requires running server**
- ⚠️ May share database state
- ⏱️ Slowest execution (browser automation)

---

## Mocking Best Practices

### Good Mocking
```csharp
// ✅ GOOD: Clear, focused setup
var mockService = new Mock<IAzureOpenAIService>();
mockService
    .Setup(x => x.GenerateVerseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new VerseResponse
    {
        Verse = "Test verse",
        Success = true
    });
```

### Bad Mocking
```csharp
// ❌ BAD: Over-specific, brittle
var mockService = new Mock<IAzureOpenAIService>(MockBehavior.Strict);
mockService
    .Setup(x => x.GenerateVerseAsync("eminem", "exact topic", default))
    .ReturnsAsync(new VerseResponse { Verse = "Specific verse" });
// Breaks if you change input values
```

---

## Test-Driven Development (TDD) Workflow

### Red → Green → Refactor

1. **Red**: Write a failing test
   ```csharp
   [Fact]
   public async Task NewFeature_ShouldWork()
   {
       // Arrange
       var service = new NewService();
       
       // Act
       var result = await service.NewMethodAsync();
       
       // Assert
       Assert.NotNull(result);  // ❌ Fails (method doesn't exist)
   }
   ```

2. **Green**: Implement minimum code to pass
   ```csharp
   public class NewService
   {
       public async Task<Result> NewMethodAsync()
       {
           return new Result();  // ✅ Passes
       }
   }
   ```

3. **Refactor**: Improve code while keeping tests green
   ```csharp
   public async Task<Result> NewMethodAsync()
   {
       // Add proper implementation
       // Tests still pass ✅
   }
   ```

---

## Running Tests

### All Tests (Excluding E2E)
```bash
dotnet test --filter "FullyQualifiedName!~SystemTests"
```

### Specific Test Project
```bash
dotnet test Tests/PoDebateRap.UnitTests
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~DebateOrchestratorTests"
```

### Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~DebateOrchestratorTests.GenerateVerse_ShouldReturnValidResponse"
```

### With Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

### Verbose Output
```bash
dotnet test --verbosity normal
```

---

## Test Data Management

### Unit Tests
Use hardcoded test data:
```csharp
private Rapper CreateTestRapper()
{
    return new Rapper
    {
        Id = "test-rapper",
        Name = "Test Rapper",
        Style = "Test style with complex rhymes"
    };
}
```

### Integration Tests
Use disposable test resources:
```csharp
[Fact]
public async Task TestTableOperation()
{
    var testTableName = $"TestTable{Guid.NewGuid():N}";
    var tableClient = new TableClient(connectionString, testTableName);
    
    try
    {
        await tableClient.CreateIfNotExistsAsync();
        
        // Test operations
    }
    finally
    {
        await tableClient.DeleteAsync();  // ALWAYS CLEANUP
    }
}
```

### E2E Tests
Use consistent test data:
```csharp
private const string TEST_RAPPER_1 = "eminem";
private const string TEST_RAPPER_2 = "tupac";
private const string TEST_TOPIC = "Is AI a threat to humanity?";
```

---

## Assertions

### xUnit Assertions
```csharp
// Equality
Assert.Equal(expected, actual);
Assert.NotEqual(unexpected, actual);

// Null checks
Assert.Null(value);
Assert.NotNull(value);

// Boolean
Assert.True(condition);
Assert.False(condition);

// Collections
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);

// Strings
Assert.StartsWith("prefix", str);
Assert.EndsWith("suffix", str);
Assert.Contains("substring", str);

// Exceptions
await Assert.ThrowsAsync<InvalidOperationException>(async () =>
{
    await service.ThrowErrorAsync();
});

// Ranges
Assert.InRange(actual, low, high);
```

### FluentAssertions (Optional)
```csharp
// More readable assertions
result.Should().NotBeNull();
result.Verse.Should().Contain("rhyme");
result.Success.Should().BeTrue();
result.TokenCount.Should().BeGreaterThan(0);

await action.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("Specific error message");
```

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
jobs:
  test:
    name: Test
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Setup .NET 9.0
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run Unit Tests
        run: dotnet test Tests/PoDebateRap.UnitTests --configuration Release --no-restore --verbosity normal
      
      - name: Run Integration Tests
        run: dotnet test Tests/PoDebateRap.IntegrationTests --configuration Release --no-restore --verbosity normal
```

**Note**: E2E tests (SystemTests) are **excluded from CI/CD** and run manually.

---

## Debugging Tests

### Visual Studio Code
1. Set breakpoint in test method
2. Click **Debug Test** above test method
3. Step through code with F10/F11

### Command Line
```bash
# Run test with debugger attached
dotnet test --logger "console;verbosity=detailed"
```

### Playwright Debugging
```bash
# Run E2E tests in headed mode (visible browser)
PWDEBUG=1 dotnet test Tests/PoDebateRap.SystemTests
```

---

## Test Coverage Report

### Current Status

| Project | Test Count | Passing | Status |
|---------|-----------|---------|--------|
| UnitTests | 4 | 4 | ✅ 100% |
| ApiTests | 4 | 4 | ✅ 100% |
| IntegrationTests | 16 | 12 | ⚠️ 75% (requires Azure credentials) |
| SystemTests (C#) | 12 | 7 | ⚠️ 58% (manual execution) |
| E2ETests (TypeScript) | 35+ | - | ✅ Ready (new) |
| **Total** | **71+** | **27** | **38%** |

### Coverage Goals
- **Unit Tests**: > 80% code coverage
- **Integration Tests**: Cover all Azure service interactions
- **E2E Tests**: Cover all critical user flows

---

## Adding New Tests

### Checklist

1. **Identify test type**: Unit, API, Integration, or E2E?
2. **Create test class**: Follow naming convention (`*Tests.cs`)
3. **Write test method**: Use descriptive names (`MethodName_Scenario_ExpectedOutcome`)
4. **Arrange**: Set up test data and mocks
5. **Act**: Execute the method under test
6. **Assert**: Verify expected outcomes
7. **Cleanup**: Dispose resources if needed
8. **Run**: Verify test passes
9. **Document**: Add to this README if it's a new test category

---

## Dependencies

### NuGet Packages
- `xunit` - Test framework
- `xunit.runner.visualstudio` - VS Code integration
- `Moq` - Mocking library
- `FluentAssertions` - Readable assertions
- `Microsoft.AspNetCore.Mvc.Testing` - API testing
- `Microsoft.Playwright` - Browser automation

---

**Last Updated**: October 28, 2025
