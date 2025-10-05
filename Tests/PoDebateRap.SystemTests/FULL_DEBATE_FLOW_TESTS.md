# E2E Tests for Full Debate Flow

This document describes the new End-to-End (E2E) tests that verify the complete debate flow functionality.

## New Tests Added

### 1. `FullDebateFlow_ShouldStartDebate_WhenAllFieldsFilledAndButtonClicked`

**Purpose**: Verifies the complete happy path of starting a debate

**Test Steps**:
1. Navigate to http://localhost:5000
2. Select "Nas" as Rapper 1
3. Select "Lauryn Hill" as Rapper 2
4. Enter "The Future of Hip Hop Culture" as the debate topic
5. Verify the "BEGIN DEBATE" button is enabled
6. Click the "BEGIN DEBATE" button
7. Verify the debate starts (either loading state appears or debate arena becomes visible)

**Expected Outcome**:
- Button shows loading state ("Starting...", "Generating...", or "Loading...")
- OR debate arena/visualizer becomes visible
- No errors are displayed

### 2. `FullDebateFlow_ShouldNotShowError_WhenValidRappersSelected`

**Purpose**: Verifies that selecting valid rappers and starting a debate does NOT produce the "Selected rapper(s) not found" error

**Test Steps**:
1. Navigate to http://localhost:5000
2. Select "Andre 3000" as Rapper 1
3. Select "Jay-Z" as Rapper 2
4. Enter "Electric Cars vs Gas Cars" as the debate topic
5. Click the "BEGIN DEBATE" button
6. Wait for any errors to appear
7. Verify NO "Selected rapper(s) not found" error is shown

**Expected Outcome**:
- No error alert with "Selected rapper(s) not found" appears
- Debate starts successfully

## Running the Tests

### Prerequisites

1. **Azurite must be running** (for local Azure Storage emulation)
   ```powershell
   azurite --silent --location ./AzuriteData
   ```

2. **Server must be running** on http://localhost:5000
   ```powershell
   dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
   ```

### Run All E2E Tests

Use the provided PowerShell script:

```powershell
.\run-e2e-tests.ps1
```

Or run manually:

```powershell
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj --logger "console;verbosity=normal"
```

### Run Only Full Debate Flow Tests

Use the provided PowerShell script:

```powershell
.\run-full-debate-flow-tests.ps1
```

Or run manually:

```powershell
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj --filter "FullyQualifiedName~FullDebateFlow" --logger "console;verbosity=detailed"
```

### Run a Specific Test

```powershell
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj --filter "FullyQualifiedName~FullDebateFlow_ShouldStartDebate_WhenAllFieldsFilledAndButtonClicked" --logger "console;verbosity=detailed"
```

## Test Configuration

- **Base URL**: http://localhost:5000
- **Browser**: Chromium (Playwright)
- **Headless Mode**: `false` (visible browser for debugging)
- **Slow Motion**: 100ms (for visibility during test execution)

## Existing Tests

The test suite also includes these existing tests:

1. `BeginDebateButton_ShouldBeDisabled_WhenTopicIsEmpty` - Verifies button is disabled without a topic
2. `BeginDebateButton_ShouldBeEnabled_WhenAllFieldsAreFilled` - Verifies button is enabled when all fields are filled
3. `BeginDebateButton_ShouldEnableDynamically_WhenTypingTopic` - Verifies button enables as user types
4. `BeginDebateButton_ShouldBeDisabled_WhenSameRapperSelected` - Verifies validation for same rapper selection
5. `DebateTopicInput_ShouldAcceptUserInput` - Verifies topic input field accepts text

## Troubleshooting

### Test Fails with "net::ERR_CONNECTION_REFUSED"

**Problem**: The server is not running on http://localhost:5000

**Solution**: 
1. Start Azurite: `azurite --silent --location ./AzuriteData`
2. Start the server: `dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj`
3. Wait for "Now listening on: http://localhost:5000" message
4. Run the tests

### Test Fails with "Selected rapper(s) not found"

**Problem**: The application is not properly connected to Azurite

**Solution**:
1. Verify Azurite is running
2. Check `appsettings.Development.json` has the correct Azurite connection string:
   ```json
   "StorageConnectionString": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
   ```
3. Restart the server

### Browser Doesn't Open During Tests

**Problem**: Playwright browsers are not installed

**Solution**:
```powershell
pwsh Tests/PoDebateRap.SystemTests/bin/Debug/net9.0/playwright.ps1 install
```

## CI/CD Considerations

For running tests in CI/CD pipelines, update the test configuration to use headless mode:

```csharp
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true,  // Change to true for CI/CD
    SlowMo = 0        // Remove slow motion for CI/CD
});
```

## Visual Verification

Since `Headless = false` is set by default, you can watch the tests execute in a real browser window. This is helpful for:

- Debugging test failures
- Verifying UI interactions
- Understanding test flow
- Demonstrating functionality

## Coverage

These E2E tests cover the critical user journey:

1. ✅ Loading the application
2. ✅ Selecting rappers from dropdowns
3. ✅ Entering a debate topic
4. ✅ Button enable/disable states
5. ✅ Clicking the "BEGIN DEBATE" button
6. ✅ Verifying debate starts without errors
7. ✅ Validating rapper data is loaded from storage

## Future Enhancements

Consider adding tests for:

- Debate turns generation and display
- Audio playback functionality
- Results modal and voting
- Leaderboard updates
- Error handling for API failures
- Multiple debate sessions
