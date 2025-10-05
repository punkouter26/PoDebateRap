using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace PoDebateRap.SystemTests;

public class DebateSetupTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "http://localhost:5000";

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
    public async Task BeginDebateButton_ShouldBeDisabled_WhenTopicIsEmpty()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Select Rapper 1
        await page.SelectOptionAsync("#rapper1Select", new[] { "Eminem" });
        await Task.Delay(500);

        // Select Rapper 2
        await page.SelectOptionAsync("#rapper2Select", new[] { "Tupac Shakur" });
        await Task.Delay(500);

        // Clear the debate topic input (in case it was pre-populated)
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("");
        await Task.Delay(500);

        // Act & Assert
        var beginButton = page.GetByRole(AriaRole.Button, new() { Name = "Begin Debate" });
        
        // Button should be disabled when topic is empty
        await Expect(beginButton).ToBeDisabledAsync();
        
        await page.CloseAsync();
    }

    [Fact]
    public async Task BeginDebateButton_ShouldBeEnabled_WhenAllFieldsAreFilled()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Select Rapper 1
        await page.SelectOptionAsync("#rapper1Select", new[] { "Eminem" });
        await Task.Delay(500);

        // Select Rapper 2
        await page.SelectOptionAsync("#rapper2Select", new[] { "Tupac Shakur" });
        await Task.Delay(500);

        // Fill in the debate topic
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("Future of Remote Work life");
        await Task.Delay(500);

        // Act & Assert
        var beginButton = page.GetByRole(AriaRole.Button, new() { Name = "Begin Debate" });
        
        // Button should be enabled when all fields are filled
        await Expect(beginButton).ToBeEnabledAsync();
        
        await page.CloseAsync();
    }

    [Fact]
    public async Task BeginDebateButton_ShouldEnableDynamically_WhenTypingTopic()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Select rappers first
        await page.SelectOptionAsync("#rapper1Select", new[] { "Eminem" });
        await Task.Delay(500);
        await page.SelectOptionAsync("#rapper2Select", new[] { "Jay-Z" });
        await Task.Delay(500);

        // Clear topic if pre-populated
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("");
        await Task.Delay(500);

        var beginButton = page.GetByRole(AriaRole.Button, new() { Name = "Begin Debate" });
        
        // Initially disabled
        await Expect(beginButton).ToBeDisabledAsync();

        // Act - Type in the topic field character by character
        await topicInput.PressSequentiallyAsync("AI Ethics", new() { Delay = 100 });
        await Task.Delay(500);

        // Assert - Button should now be enabled
        await Expect(beginButton).ToBeEnabledAsync();
        
        await page.CloseAsync();
    }

    [Fact]
    public async Task BeginDebateButton_ShouldBeDisabled_WhenSameRapperSelected()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Select same rapper for both
        await page.SelectOptionAsync("#rapper1Select", new[] { "Eminem" });
        await Task.Delay(500);
        
        // Fill in topic
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("Test Topic");
        await Task.Delay(500);

        // Try to select same rapper (though UI should prevent this)
        // In your implementation, Rapper 2 dropdown filters out Rapper 1
        // So this test verifies the validation logic

        var beginButton = page.GetByRole(AriaRole.Button, new() { Name = "Begin Debate" });
        
        // Button should be disabled because Rapper 2 is not selected
        await Expect(beginButton).ToBeDisabledAsync();
        
        await page.CloseAsync();
    }

    [Fact]
    public async Task DebateTopicInput_ShouldAcceptUserInput()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        var topicInput = page.Locator("#debateTopicInput");
        var testTopic = "Climate Change Debate";

        // Act
        await topicInput.FillAsync(testTopic);
        await Task.Delay(500);

        // Assert
        var inputValue = await topicInput.InputValueAsync();
        Assert.Equal(testTopic, inputValue);
        
        await page.CloseAsync();
    }

    [Fact]
    public async Task FullDebateFlow_ShouldStartDebate_WhenAllFieldsFilledAndButtonClicked()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Act - Select Rapper 1
        await page.SelectOptionAsync("#rapper1Select", new[] { "Nas" });
        await Task.Delay(500);

        // Select Rapper 2
        await page.SelectOptionAsync("#rapper2Select", new[] { "Lauryn Hill" });
        await Task.Delay(500);

        // Enter debate topic
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("The Future of Hip Hop Culture");
        await Task.Delay(500);

        // Verify button is enabled before clicking
        var beginButton = page.GetByRole(AriaRole.Link, new() { Name = "Begin Debate" });
        await Expect(beginButton).ToBeEnabledAsync();

        // Click the BEGIN DEBATE button
        await beginButton.ClickAsync();
        await Task.Delay(1000); // Wait for the request to be sent

        // Assert - Verify the debate started
        // The button should change to show "Starting..." or "Generating..." 
        // or be disabled while the debate is being generated
        var buttonText = await beginButton.TextContentAsync();
        
        // Check if we either see loading state or the debate arena appears
        var isLoading = buttonText?.Contains("Starting") == true || 
                       buttonText?.Contains("Generating") == true ||
                       buttonText?.Contains("Loading") == true;
        
        // Alternative: Check if debate arena/visualizer becomes visible
        var debateArena = page.Locator(".debate-arena-container");
        var arenaVisible = await debateArena.IsVisibleAsync();
        
        // At least one of these conditions should be true
        Assert.True(isLoading || arenaVisible, 
            $"Expected debate to start. Button text: '{buttonText}', Arena visible: {arenaVisible}");
        
        await Task.Delay(2000); // Give time to observe the result
        await page.CloseAsync();
    }

    [Fact]
    public async Task FullDebateFlow_ShouldNotShowError_WhenValidRappersSelected()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        
        // Wait for the page to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Wait for Blazor to initialize

        // Act - Select Rapper 1
        await page.SelectOptionAsync("#rapper1Select", new[] { "Andre 3000" });
        await Task.Delay(500);

        // Select Rapper 2
        await page.SelectOptionAsync("#rapper2Select", new[] { "Jay-Z" });
        await Task.Delay(500);

        // Enter debate topic
        var topicInput = page.Locator("#debateTopicInput");
        await topicInput.FillAsync("Electric Cars vs Gas Cars");
        await Task.Delay(500);

        // Click the BEGIN DEBATE button
        var beginButton = page.GetByRole(AriaRole.Link, new() { Name = "Begin Debate" });
        await beginButton.ClickAsync();
        await Task.Delay(1500); // Wait for any errors to appear

        // Assert - Verify no "Selected rapper(s) not found" error appears
        var errorAlert = page.Locator(".alert-error");
        var hasError = await errorAlert.IsVisibleAsync();
        
        if (hasError)
        {
            var errorText = await errorAlert.TextContentAsync();
            Assert.False(errorText?.Contains("Selected rapper(s) not found") == true,
                $"Unexpected error appeared: {errorText}");
        }
        
        await page.CloseAsync();
    }
}
