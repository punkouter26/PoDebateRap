# Audio Playback E2E Tests

## Overview
Comprehensive end-to-end tests to verify that text-to-speech audio generation and playback works correctly in the PoDebateRap application.

## Test Suite: `AudioPlaybackTests.cs`

### Test 1: `Debate_ShouldGenerateAndPlayAudio_ForFirstTurn`
**Purpose:** Verifies that audio is generated and playable for the first debate turn.

**Test Flow:**
1. Navigate to application
2. Select two rappers (Nas vs Jay-Z)
3. Enter a debate topic
4. Click "BEGIN DEBATE"
5. Wait for first turn audio generation (max 30 seconds)
6. Verify audio data is transmitted to browser
7. Verify no playback errors occur

**Assertions:**
- ✅ Audio is generated within 30 seconds
- ✅ `playAudio` JavaScript function is called with valid base64 data
- ✅ Audio base64 length is >100KB (substantial audio data)
- ✅ Audio object is created successfully in browser
- ✅ No audio errors occur during playback
- ✅ Screenshot is captured for debugging

**Expected Console Logs:**
```
🎵 playAudio called with base64 length: 1874800
🎵 Base64 prefix (first 50 chars): UklGRhz6AQBXQVZFZm10...
🎵 dotnetHelper: Present
✅ Audio object created successfully
🔊 Volume set to: 1
✅ Audio playback started successfully
▶️ Audio is now PLAYING!
```

---

### Test 2: `Debate_ShouldGenerateAudio_ForMultipleTurns`
**Purpose:** Ensures audio is generated for multiple consecutive debate turns.

**Test Flow:**
1. Start debate with Eminem vs Kendrick Lamar
2. Wait up to 60 seconds
3. Count number of `playAudio` calls
4. Verify at least 2 turns have audio

**Assertions:**
- ✅ At least 2 audio turns are generated
- ✅ Each turn triggers a separate `playAudio` call
- ✅ Audio continues throughout the debate

**Why This Matters:**
Tests the audio persistence fix - ensures audio isn't cleared prematurely between turns.

---

### Test 3: `AudioPlayback_ShouldNotHaveErrors_DuringDebate`
**Purpose:** Validates that no errors occur during audio generation and playback.

**Test Flow:**
1. Start debate with Tupac vs Biggie
2. Monitor console for error messages
3. Track success messages
4. Verify error-free operation

**Assertions:**
- ✅ No error messages (❌) appear in console
- ✅ At least one success message (✅) appears
- ✅ Audio system operates without failures

**Monitored Error Patterns:**
- `❌` emoji indicators
- Messages containing "Error" and "audio"
- Failed audio creation
- Playback failures

---

### Test 4: `AudioVolume_ShouldBeSetToMaximum`
**Purpose:** Confirms audio volume is set to maximum (1.0) for audibility.

**Test Flow:**
1. Start debate with Andre 3000 vs Rakim
2. Wait for audio setup
3. Check volume log messages
4. Verify volume is set to 1.0

**Assertions:**
- ✅ Volume setting log appears
- ✅ Volume is set to 1.0 (maximum)

---

## Running the Tests

### Option 1: Using PowerShell Script (Recommended)
```powershell
.\run-audio-tests.ps1
```

**Features:**
- Checks if server is running first
- Runs only audio-specific tests
- Provides colored output
- Shows test coverage summary

### Option 2: Manual dotnet test
```powershell
# Ensure server is running first
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj

# In a separate terminal:
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj `
    --filter "FullyQualifiedName~AudioPlaybackTests" `
    --logger "console;verbosity=detailed"
```

### Option 3: Run Individual Test
```powershell
dotnet test Tests/PoDebateRap.SystemTests/PoDebateRap.SystemTests.csproj `
    --filter "FullyQualifiedName~AudioPlaybackTests.Debate_ShouldGenerateAndPlayAudio_ForFirstTurn"
```

---

## Prerequisites

### 1. Server Must Be Running
```powershell
dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
```

Server should be accessible at: http://localhost:5000

### 2. Azure Speech Service Configured
Ensure `appsettings.Development.json` has valid credentials:
```json
"Azure": {
  "Speech": {
    "Region": "eastus",
    "SubscriptionKey": "your-key-here"
  }
}
```

### 3. Playwright Browsers Installed
```powershell
pwsh Tests/PoDebateRap.SystemTests/bin/Debug/net9.0/playwright.ps1 install
```

---

## Test Output Examples

### ✅ Passing Test
```
[AUDIO LOG] 🎵 playAudio called with base64 length: 1874800
[AUDIO LOG] 🎵 Creating audio with WAV format, data URL length: 1874900
[AUDIO LOG] ✅ Audio object created successfully
[AUDIO LOG] 🔊 Volume set to: 1
[AUDIO LOG] ✅ Audio playback started successfully
Audio detected at 8 seconds
Play audio log: 🎵 playAudio called with base64 length: 1874800
Audio base64 length: 1874800
Audio object created successfully
Screenshot saved to audio-test-screenshot.png

Test passed! 1 audio turns detected
```

### ❌ Failing Test (Server Not Running)
```
✗ Server is NOT running on http://localhost:5000

Please start the server first:
  dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
```

### ❌ Failing Test (No Audio Generated)
```
Assert.True() Failure
Expected: True
Actual:   False
Message: Audio should be generated for the first turn within 30 seconds
```

---

## Debugging Tips

### 1. Check Server Logs
Look for these log messages:
```
info: PoDebateRap.ServerApi.Services.Speech.TextToSpeechService[0]
      Speech synthesis completed successfully.
info: PoDebateRap.ServerApi.Services.Orchestration.DebateOrchestrator[0]
      🎵 Generated audio for turn 1, size: 1874800 bytes
```

### 2. Check Browser Console (if headless=false)
The tests run with a visible browser by default. Open DevTools (F12) to see:
- 🎵 Audio loading messages
- ✅ Success indicators
- ❌ Error messages

### 3. Review Screenshot
Each test saves a screenshot to `audio-test-screenshot.png` showing the debate state when audio should be playing.

### 4. Increase Wait Times
If tests are flaky due to slow network/API:
```csharp
await Task.Delay(15000); // Increase from 10000 to 15000
```

---

## Technical Details

### Audio Data Flow
1. **Server**: Azure Speech Service generates WAV audio (16kHz, 16-bit mono)
2. **Server**: Audio bytes stored in `DebateState.CurrentTurnAudio`
3. **API**: `/api/Debate/state` endpoint returns audio as byte array
4. **Client**: Blazor polls every 1 second
5. **Client**: Converts bytes to base64 string
6. **Client**: Calls JavaScript `playAudio(dotnetHelper, base64Audio)`
7. **Browser**: Creates Audio object with data URL
8. **Browser**: Plays audio automatically

### What the Tests Verify
- ✅ Step 1-2: Server generates audio (check server logs)
- ✅ Step 3-4: API transmits audio (check network tab)
- ✅ Step 5-6: Client receives and processes audio (check C# logs)
- ✅ Step 7-8: Browser plays audio (check JS console logs)

### Common Issues

#### Browser Autoplay Policy
**Symptom:** Console shows "NotAllowedError: play() failed"

**Solution:** The test clicks a button (user interaction) before audio plays, which should satisfy autoplay policies. If this still fails, the code includes a fallback that waits for the next click.

#### Azure Speech Rate Limits
**Symptom:** Audio generation takes >30 seconds or fails

**Solution:** 
- Check Azure Speech Service quota
- Verify subscription key is valid
- Check server logs for API errors

#### Race Condition (Fixed)
**Symptom:** Audio data is null when client polls

**Solution:** Already fixed in `DebateOrchestrator.cs` - audio is no longer cleared prematurely.

---

## CI/CD Integration

For automated testing in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Install Playwright
  run: pwsh Tests/PoDebateRap.SystemTests/bin/Debug/net9.0/playwright.ps1 install

- name: Start Server
  run: dotnet run --project Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj &
  
- name: Wait for Server
  run: |
    for i in {1..30}; do
      if curl -f http://localhost:5000; then break; fi
      sleep 1
    done

- name: Run Audio Tests
  run: dotnet test Tests/PoDebateRap.SystemTests --filter "AudioPlaybackTests"
  env:
    HEADLESS: true  # Run browsers in headless mode
```

**Note:** Set `Headless = true` in test initialization for CI/CD:
```csharp
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = Environment.GetEnvironmentVariable("HEADLESS") == "true"
});
```

---

## Test Maintenance

### When to Update Tests

1. **Audio Format Changes**: Update assertions if switching from WAV to MP3, etc.
2. **Voice Changes**: Update rapper names if voice assignments change
3. **UI Changes**: Update selectors if HTML IDs/classes change
4. **Timing Changes**: Adjust wait times if debate generation speeds change

### Adding New Audio Tests

Follow this pattern:
```csharp
[Fact]
public async Task YourNewTest()
{
    var page = await _browser!.NewPageAsync();
    
    // Capture audio logs
    page.Console += (_, msg) => { /* log tracking */ };
    
    // Setup and start debate
    // ...
    
    // Wait for audio
    // ...
    
    // Assert audio behaviors
    Assert.True(/* your condition */);
    
    await page.CloseAsync();
}
```

---

## Success Criteria

All 4 tests should pass, indicating:
- ✅ Audio generates for first turn
- ✅ Audio continues for multiple turns  
- ✅ No errors during playback
- ✅ Volume is audible (maximum)

If all tests pass, text-to-speech is **fully functional**! 🎉
