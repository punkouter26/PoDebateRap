using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace PoDebateRap.SystemTests;

/// <summary>
/// E2E tests to verify audio playback functionality during debates.
/// Tests ensure that audio is generated, transmitted, and playable in the browser.
/// </summary>
public class AudioPlaybackTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "http://localhost:5000";

    public AudioPlaybackTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Set to true for CI/CD
            SlowMo = 100 // Slow down by 100ms for visibility during debugging
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }

    [Fact]
    public async Task Debate_ShouldGenerateAndPlayAudio_ForFirstTurn()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        var consoleMessages = new List<string>();
        var audioPlaybackLogs = new List<string>();

        // Capture console logs
        page.Console += (_, msg) =>
        {
            var message = msg.Text;
            consoleMessages.Add(message);
            
            // Track audio-related logs
            if (message.Contains("playAudio") || message.Contains("🎵") || message.Contains("Audio"))
            {
                audioPlaybackLogs.Add(message);
                _output.WriteLine($"[AUDIO LOG] {message}");
            }
        };

        await page.GotoAsync(BaseUrl);
        _output.WriteLine("Page loaded");

        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Select Rapper 1
        _output.WriteLine("Selecting Rapper 1: Nas");
        await page.SelectOptionAsync("#rapper1Select", new[] { "Nas" });
        await Task.Delay(500);

        // Select Rapper 2
        _output.WriteLine("Selecting Rapper 2: Jay-Z");
        await page.SelectOptionAsync("#rapper2Select", new[] { "Jay-Z" });
        await Task.Delay(500);

        // Enter a topic
        _output.WriteLine("Entering topic");
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("The Future of Hip Hop");
        await Task.Delay(500);

        // Click Begin Debate button
        _output.WriteLine("Clicking Begin Debate button");
        var beginButton = page.Locator("a.btn-primary:has-text('Begin Debate')");
        await beginButton.ClickAsync();

        // Wait for debate to start generating
        _output.WriteLine("Waiting for debate to start...");
        await Task.Delay(3000);

        // Wait for first turn audio to be generated (up to 30 seconds)
        _output.WriteLine("Waiting for first turn audio generation...");
        var audioGenerated = false;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(1000);
            
            // Check if we have audio playback logs
            if (audioPlaybackLogs.Any(log => log.Contains("playAudio called with base64 length")))
            {
                audioGenerated = true;
                _output.WriteLine($"Audio detected at {i + 1} seconds");
                break;
            }
        }

        // Assert: Audio was generated
        Assert.True(audioGenerated, "Audio should be generated for the first turn within 30 seconds");

        // Assert: playAudio function was called with valid base64 data
        var playAudioLog = audioPlaybackLogs.FirstOrDefault(log => log.Contains("playAudio called with base64 length"));
        Assert.NotNull(playAudioLog);
        _output.WriteLine($"Play audio log: {playAudioLog}");

        // Extract the base64 length from the log
        // Expected format: "🎵 playAudio called with base64 length: 125678"
        var lengthMatch = System.Text.RegularExpressions.Regex.Match(playAudioLog, @"length:\s*(\d+)");
        if (lengthMatch.Success)
        {
            var audioLength = int.Parse(lengthMatch.Groups[1].Value);
            _output.WriteLine($"Audio base64 length: {audioLength}");
            Assert.True(audioLength > 100000, "Audio data should be substantial (>100KB base64)");
        }

        // Assert: Audio object was created successfully
        var audioCreatedLog = audioPlaybackLogs.FirstOrDefault(log => log.Contains("Audio object created successfully") || log.Contains("✅"));
        Assert.NotNull(audioCreatedLog);
        _output.WriteLine("Audio object created successfully");

        // Assert: Check for any audio playback errors
        var audioErrors = audioPlaybackLogs.Where(log => log.Contains("Error") || log.Contains("❌")).ToList();
        if (audioErrors.Any())
        {
            _output.WriteLine("Audio errors detected:");
            foreach (var error in audioErrors)
            {
                _output.WriteLine($"  - {error}");
            }
        }
        Assert.Empty(audioErrors);

        // Wait a bit longer to see if audio plays
        _output.WriteLine("Waiting for audio playback confirmation...");
        await Task.Delay(5000);

        // Check if audio started playing
        var playbackStartedLog = audioPlaybackLogs.FirstOrDefault(log => 
            log.Contains("Audio playback started successfully") || 
            log.Contains("Audio is now PLAYING"));
        
        if (playbackStartedLog != null)
        {
            _output.WriteLine($"Audio playback started: {playbackStartedLog}");
        }

        // Take a screenshot for debugging
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "audio-test-screenshot.png",
            FullPage = true
        });
        _output.WriteLine("Screenshot saved to audio-test-screenshot.png");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Debate_ShouldGenerateAudio_ForMultipleTurns()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        var audioCallCount = 0;

        // Capture console logs and count playAudio calls
        page.Console += (_, msg) =>
        {
            var message = msg.Text;
            if (message.Contains("🎵 playAudio called with base64 length"))
            {
                audioCallCount++;
                _output.WriteLine($"[AUDIO #{audioCallCount}] {message}");
            }
        };

        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Select rappers and topic
        _output.WriteLine("Setting up debate: Eminem vs Kendrick Lamar");
        await page.SelectOptionAsync("#rapper1Select", new[] { "Eminem" });
        await Task.Delay(300);
        await page.SelectOptionAsync("#rapper2Select", new[] { "Kendrick Lamar" });
        await Task.Delay(300);
        await page.Locator("#debateTopicInput").FillAsync("Best Lyricist");
        await Task.Delay(300);

        // Start debate
        _output.WriteLine("Starting debate");
        await page.Locator("a.btn-primary:has-text('Begin Debate')").ClickAsync();

        // Wait for at least 2 turns of audio (up to 60 seconds)
        _output.WriteLine("Waiting for multiple audio turns...");
        for (int i = 0; i < 60; i++)
        {
            await Task.Delay(1000);
            
            if (audioCallCount >= 2)
            {
                _output.WriteLine($"Found {audioCallCount} audio turns at {i + 1} seconds");
                break;
            }
        }

        // Assert: At least 2 audio turns were generated
        Assert.True(audioCallCount >= 2, $"Expected at least 2 audio turns, but got {audioCallCount}");
        _output.WriteLine($"Test passed! {audioCallCount} audio turns detected");

        await page.CloseAsync();
    }

    [Fact]
    public async Task AudioPlayback_ShouldNotHaveErrors_DuringDebate()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        var audioErrors = new List<string>();
        var audioSuccesses = new List<string>();

        // Capture console logs
        page.Console += (_, msg) =>
        {
            var message = msg.Text;
            
            // Track errors
            if (message.Contains("❌") || (message.Contains("Error") && message.Contains("audio")))
            {
                audioErrors.Add(message);
                _output.WriteLine($"[ERROR] {message}");
            }
            
            // Track successes
            if (message.Contains("✅") || message.Contains("successfully"))
            {
                if (message.Contains("audio") || message.Contains("playback") || message.Contains("Audio"))
                {
                    audioSuccesses.Add(message);
                    _output.WriteLine($"[SUCCESS] {message}");
                }
            }
        };

        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Start a simple debate
        await page.SelectOptionAsync("#rapper1Select", new[] { "Tupac Shakur" });
        await Task.Delay(300);
        await page.SelectOptionAsync("#rapper2Select", new[] { "The Notorious B.I.G." });
        await Task.Delay(300);
        await page.Locator("#debateTopicInput").FillAsync("East Coast vs West Coast");
        await Task.Delay(300);
        await page.Locator("a.btn-primary:has-text('Begin Debate')").ClickAsync();

        // Wait for first audio turn
        _output.WriteLine("Waiting for audio generation...");
        await Task.Delay(15000); // Wait 15 seconds

        // Assert: No audio errors occurred
        if (audioErrors.Any())
        {
            _output.WriteLine($"Found {audioErrors.Count} audio errors:");
            foreach (var error in audioErrors)
            {
                _output.WriteLine($"  - {error}");
            }
        }
        Assert.Empty(audioErrors);

        // Assert: At least one success log
        Assert.NotEmpty(audioSuccesses);
        _output.WriteLine($"Audio system working correctly with {audioSuccesses.Count} success events");

        await page.CloseAsync();
    }

    [Fact]
    public async Task AudioVolume_ShouldBeSetToMaximum()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        var volumeSet = false;
        var volumeValue = "";

        // Capture console logs
        page.Console += (_, msg) =>
        {
            var message = msg.Text;
            
            if (message.Contains("🔊 Volume set to:"))
            {
                volumeSet = true;
                volumeValue = message;
                _output.WriteLine($"[VOLUME] {message}");
            }
        };

        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Start debate
        await page.SelectOptionAsync("#rapper1Select", new[] { "Andre 3000" });
        await Task.Delay(300);
        await page.SelectOptionAsync("#rapper2Select", new[] { "Rakim" });
        await Task.Delay(300);
        await page.Locator("#debateTopicInput").FillAsync("Evolution of Hip Hop");
        await Task.Delay(300);
        await page.Locator("a.btn-primary:has-text('Begin Debate')").ClickAsync();

        // Wait for audio setup
        await Task.Delay(10000);

        // Assert: Volume was set
        Assert.True(volumeSet, "Volume should be set when audio is created");
        
        // Assert: Volume is 1.0 (maximum)
        Assert.Contains("1", volumeValue);
        _output.WriteLine("Volume correctly set to maximum");

        await page.CloseAsync();
    }
}
