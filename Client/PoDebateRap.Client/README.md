# PoDebateRap.Client

Blazor WebAssembly client for the PoDebateRap application. Provides an interactive, responsive UI for AI-powered rap debates with real-time updates.

## Project Overview

- **Framework**: .NET 9.0
- **Type**: Blazor WebAssembly
- **Hosting**: Hosted inside PoDebateRap.ServerApi
- **Architecture**: Component-based UI with SignalR for real-time communication

## Key Components

### Pages

#### `Home.razor` (`Components/Pages/Home.razor`)
Main application page that hosts the debate flow.

**Features**:
- Rapper selection dropdowns
- Topic selection (NewsAPI headlines or custom input)
- Debate setup form
- Debate arena (when debate is active)
- Diagnostics link

**Key Methods**:
```csharp
private async Task StartDebateAsync()
{
    var request = new DebateRequest
    {
        Rapper1Id = selectedRapper1,
        Rapper2Id = selectedRapper2,
        Topic = useCustomTopic ? customTopic : selectedHeadline
    };
    
    debateResponse = await Http.PostAsJsonAsync("/api/debate/start", request);
    currentDebateId = debateResponse.DebateId;
    
    // Navigate to arena
    showArena = true;
}
```

### Debate Components (`Components/Debate/`)

#### `DebateSetup.razor`
Rapper and topic selection interface.

**Features**:
- Dropdown for Rapper 1 selection (10 legendary rappers)
- Dropdown for Rapper 2 selection (cannot match Rapper 1)
- Radio buttons: Trending topics vs. Custom topic
- NewsAPI headline selection
- Custom topic text input (minimum 10 characters)
- **BEGIN DEBATE** button (enabled when valid selection)

**Validation**:
- Two different rappers must be selected
- Topic must be at least 10 characters
- Button disabled until all criteria met

**Key Methods**:
```csharp
private async Task LoadRappersAsync()
{
    rappers = await Http.GetFromJsonAsync<List<Rapper>>("/api/rappers");
}

private async Task LoadHeadlinesAsync()
{
    headlines = await Http.GetFromJsonAsync<List<NewsHeadline>>("/api/news/headlines");
}

private void OnRapper1Change(ChangeEventArgs e)
{
    selectedRapper1 = e.Value?.ToString();
    ValidateSelection();
}

private void ValidateSelection()
{
    isValid = !string.IsNullOrEmpty(selectedRapper1) &&
              !string.IsNullOrEmpty(selectedRapper2) &&
              selectedRapper1 != selectedRapper2 &&
              (!string.IsNullOrEmpty(selectedTopic) && selectedTopic.Length >= 10);
}
```

#### `DebateArena.razor`
Live debate display with real-time updates.

**Features**:
- Rapper profiles (avatar, name, style)
- Turn-by-turn verse display
- Audio playback for each verse
- Turn counter
- Judge's decision (winner + reasoning)
- **New Debate** button

**SignalR Integration**:
```csharp
protected override async Task OnInitializedAsync()
{
    hubConnection = new HubConnectionBuilder()
        .WithUrl(NavigationManager.ToAbsoluteUri("/debateHub"))
        .Build();
    
    hubConnection.On<DebateTurn>("ReceiveTurn", turn =>
    {
        turns.Add(turn);
        StateHasChanged();
    });
    
    hubConnection.On<JudgmentResponse>("ReceiveJudgment", judgment =>
    {
        this.judgment = judgment;
        debateComplete = true;
        StateHasChanged();
    });
    
    await hubConnection.StartAsync();
}
```

**Audio Playback**:
```csharp
private async Task PlayAudioAsync(string audioBase64)
{
    // Convert base64 to Blob URL
    await JSRuntime.InvokeVoidAsync("playAudio", audioBase64);
}
```

### Diagnostics Components (`Components/Diagnostics/`)

#### `Diag.razor`
System diagnostics and health checks page.

**Features**:
- Service health status cards (OpenAI, Speech, Storage, News)
- **Test Audio** button
- Audio byte inspection (first 50 bytes)
- RIFF header verification
- Real-time health check execution

**Key Methods**:
```csharp
private async Task LoadHealthStatusAsync()
{
    var openAIHealth = await Http.GetFromJsonAsync<HealthStatus>("/api/health/openai");
    var speechHealth = await Http.GetFromJsonAsync<HealthStatus>("/api/health/speech");
    var storageHealth = await Http.GetFromJsonAsync<HealthStatus>("/api/health/storage");
    var newsHealth = await Http.GetFromJsonAsync<HealthStatus>("/api/health/news");
    
    // Update UI
}

private async Task TestAudioAsync()
{
    var response = await Http.GetAsync("/api/diagnostics/test-audio");
    var audioBytes = await response.Content.ReadAsByteArrayAsync();
    
    // Verify RIFF header (52 49 46 46)
    var firstBytes = audioBytes.Take(4).ToArray();
    var isValid = firstBytes[0] == 0x52 && firstBytes[1] == 0x49 && 
                  firstBytes[2] == 0x46 && firstBytes[3] == 0x46;
    
    if (isValid)
    {
        await PlayAudioAsync(Convert.ToBase64String(audioBytes));
    }
}
```

### Layout Components (`Components/Layout/`)

#### `MainLayout.razor`
Root layout component.

**Structure**:
```razor
<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <a href="https://github.com/punkouter26/PoDebateRap" target="_blank">GitHub</a>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

#### `NavMenu.razor`
Navigation menu.

**Links**:
- Home (`/`)
- Diagnostics (`/diag`)

## State Management

### Component State
Blazor components manage local state with `@code` blocks:

```csharp
@code {
    private List<Rapper> rappers = new();
    private string? selectedRapper1;
    private string? selectedRapper2;
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        isLoading = false;
    }
}
```

### Real-Time State (SignalR)
Debate state updates pushed from server:

```csharp
hubConnection.On<DebateTurn>("ReceiveTurn", turn =>
{
    turns.Add(turn);
    StateHasChanged();  // Force UI refresh
});
```

## HTTP Communication

### API Calls
```csharp
// Injected HttpClient
@inject HttpClient Http

// GET request
var rappers = await Http.GetFromJsonAsync<List<Rapper>>("/api/rappers");

// POST request
var response = await Http.PostAsJsonAsync("/api/debate/start", request);
var debate = await response.Content.ReadFromJsonAsync<DebateResponse>();
```

## JavaScript Interop

### Audio Playback (`wwwroot/js/audio.js`)
```javascript
window.playAudio = function(base64Audio) {
    const audioBlob = base64ToBlob(base64Audio, 'audio/mp3');
    const audioUrl = URL.createObjectURL(audioBlob);
    
    const audio = new Audio(audioUrl);
    audio.play();
    
    audio.onended = () => URL.revokeObjectURL(audioUrl);
};

function base64ToBlob(base64, mimeType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
}
```

**Usage in Blazor**:
```csharp
await JSRuntime.InvokeVoidAsync("playAudio", audioBase64);
```

## Styling

### CSS Structure
- `wwwroot/css/app.css` - Global styles
- `wwwroot/css/bootstrap/bootstrap.min.css` - Bootstrap framework
- Component-specific styles in `<style>` blocks

### Responsive Design
Mobile-first approach with Bootstrap grid:

```css
@media (max-width: 768px) {
    .rapper-card {
        width: 100%;
        margin: 10px 0;
    }
}

@media (min-width: 769px) {
    .rapper-card {
        width: 45%;
        display: inline-block;
    }
}
```

### Key CSS Classes
- `.debate-arena` - Main debate container
- `.rapper-card` - Rapper profile card
- `.verse-container` - Individual verse display
- `.turn-indicator` - Turn number badge
- `.audio-controls` - Playback controls

## Component Hierarchy

```
App.razor
└── Router
    └── RouteView
        └── MainLayout
            └── NavMenu
            └── @Body
                ├── Home.razor
                │   ├── DebateSetup.razor (when !showArena)
                │   └── DebateArena.razor (when showArena)
                └── Diag.razor
```

## Data Models (from Shared)

### Rapper
```csharp
public class Rapper
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Style { get; init; }
    public string? ImageUrl { get; init; }
    public string? Era { get; init; }
}
```

### DebateRequest
```csharp
public class DebateRequest
{
    public required string Rapper1Id { get; set; }
    public required string Rapper2Id { get; set; }
    public required string Topic { get; set; }
}
```

### DebateTurn
```csharp
public class DebateTurn
{
    public required string DebateId { get; init; }
    public required int TurnNumber { get; init; }
    public required string RapperId { get; init; }
    public required string RapperName { get; init; }
    public required string Verse { get; init; }
    public required string AudioBase64 { get; init; }
    public DateTime Timestamp { get; init; }
}
```

## Running the Client

The client is hosted inside the Server project and doesn't run independently.

```bash
# Run the entire application (Server + Client)
cd ../../Server/PoDebateRap.ServerApi
dotnet run

# Client available at:
# http://localhost:5000
```

## Development Workflow

### Hot Reload
Blazor WebAssembly supports hot reload:

```bash
dotnet watch run --project ../Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj
```

Changes to `.razor` files automatically refresh the browser.

### Debugging
1. **Browser DevTools** (F12):
   - Console: JavaScript errors, log output
   - Network: API calls, response inspection
   - Application: Local storage, session data

2. **Visual Studio Code**:
   - Set breakpoints in `@code` blocks
   - Use Debug mode (F5)
   - Inspect component state

## Best Practices

### Component Design
```csharp
// ✅ GOOD: Clear parameter definition
[Parameter]
public required string DebateId { get; set; }

[Parameter]
public EventCallback OnDebateComplete { get; set; }

// ❌ BAD: No required modifier for non-nullable
[Parameter]
public string DebateId { get; set; }
```

### State Updates
```csharp
// ✅ GOOD: Call StateHasChanged after async operations
private async Task LoadDataAsync()
{
    data = await Http.GetFromJsonAsync<Data>("/api/data");
    StateHasChanged();
}

// ❌ BAD: Forgetting to refresh UI
private async Task LoadDataAsync()
{
    data = await Http.GetFromJsonAsync<Data>("/api/data");
    // UI won't update!
}
```

### Error Handling
```csharp
// ✅ GOOD: Try-catch with user feedback
private async Task StartDebateAsync()
{
    try
    {
        isLoading = true;
        await Http.PostAsJsonAsync("/api/debate/start", request);
    }
    catch (Exception ex)
    {
        errorMessage = $"Failed to start debate: {ex.Message}";
    }
    finally
    {
        isLoading = false;
    }
}
```

## Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Components.WebAssembly` - Blazor WASM runtime
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer` - Development server
- `Microsoft.AspNetCore.SignalR.Client` - Real-time communication
- `System.Net.Http.Json` - JSON HTTP extensions

### Project References
- `PoDebateRap.Shared` - Shared models and DTOs

## Future Enhancements

- [ ] Application Insights JavaScript SDK for client-side telemetry
- [ ] Progressive Web App (PWA) support
- [ ] Offline mode with local caching
- [ ] User authentication (Azure AD B2C)
- [ ] Debate history browsing
- [ ] Social sharing features

---

**Last Updated**: October 28, 2025
