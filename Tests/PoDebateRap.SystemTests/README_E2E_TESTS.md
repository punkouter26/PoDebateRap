# Playwright E2E Tests for PoDebateRap

## Setup

1. Install Playwright browsers (one-time setup):
   ```powershell
   dotnet build Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj
   cd Tests/PoDebateRap.SystemTests
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```

## Running Tests

### Prerequisites
The server MUST be running on http://localhost:5000 before running tests.

```powershell
# Start the server in a separate terminal
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
```

### Run All E2E Tests
```powershell
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj --filter "FullyQualifiedName~DebateSetupTests"
```

### Run Specific Test
```powershell
# Test that button enables when typing topic
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj --filter "FullyQualifiedName~BeginDebateButton_ShouldEnableDynamically_WhenTypingTopic"
```

## Test Cases

### 1. BeginDebateButton_ShouldBeDisabled_WhenTopicIsEmpty
**Purpose**: Verifies button is disabled when topic field is empty  
**Steps**:
- Select two different rappers
- Clear the topic field
- Expect: Button should be disabled

### 2. BeginDebateButton_ShouldBeEnabled_WhenAllFieldsAreFilled  
**Purpose**: Verifies button enables when all fields are properly filled  
**Steps**:
- Select Rapper 1
- Select Rapper 2 (different from Rapper 1)
- Fill in a topic
- Expect: Button should be enabled

### 3. BeginDebateButton_ShouldEnableDynamically_WhenTypingTopic ⚠️
**Purpose**: Verifies button enables dynamically as user types in topic field  
**Steps**:
- Select two rappers
- Clear topic field (button should be disabled)
- Type text into topic field
- Expect: Button should enable as soon as text is entered

**Current Status**: FAILING - This test identified the bug!  
**Issue**: The two-way binding from DebateSetup component to Home component's `DebateTopicInput` property is not working properly.

### 4. BeginDebateButton_ShouldBeDisabled_WhenSameRapperSelected
**Purpose**: Validates that button stays disabled if same rapper selected for both  
**Steps**:
- Select a rapper for Rapper 1
- Enter a topic
- Expect: Button disabled because Rapper 2 is not selected (UI prevents selecting same rapper)

### 5. DebateTopicInput_ShouldAcceptUserInput
**Purpose**: Basic test that topic input field accepts and stores text  
**Steps**:
- Type text into topic input
- Verify the input value matches what was typed

## Known Issues

### Issue: Button Not Enabling When Typing
**Test**: `BeginDebateButton_ShouldEnableDynamically_WhenTypingTopic`  
**Status**: FAILING  
**Description**: When user types in the debate topic field, the "Begin Debate" button remains disabled even though all validation requirements are met (two different rappers selected + non-empty topic).

**Root Cause**: The `@oninput` event handler in `DebateSetup.razor` is not properly triggering Blazor's change detection to update the parent `Home.razor` component's `DebateTopicInput` property and re-evaluate `IsStartDisabled()`.

**Files Involved**:
- `Client/PoDebateRap.Client/Components/Debate/DebateSetup.razor`
- `Client/PoDebateRap.Client/Components/Pages/Home.razor`

## Manual Testing Instructions

1. Start the server
2. Open http://localhost:5000 in a browser
3. Open browser DevTools (F12)
4. Select Lauryn Hill for Rapper 1
5. Select Jay-Z for Rapper 2
6. Clear the debate topic field if pre-populated
7. Type anything into the topic field
8. ❌ BUG: Notice the "Begin Debate" button stays disabled
9. ✅ EXPECTED: Button should enable as soon as you start typing

## Troubleshooting

### Error: ERR_CONNECTION_REFUSED
- Make sure the server is running on http://localhost:5000
- Check that the server started successfully without errors

### Error: Browser not installed
- Run the Playwright install command:
  ```powershell
  pwsh Tests/PoDebateRap.SystemTests/bin/Debug/net9.0/playwright.ps1 install
  ```

### Tests running but server keeps stopping
- Don't run tests while also building the project
- Run server in a separate terminal window
- Consider using `dotnet watch` for auto-reload during development
