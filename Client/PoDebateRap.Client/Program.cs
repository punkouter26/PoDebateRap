using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoDebateRap.Client;
using Radzen; // Add Radzen namespace
using Radzen.Blazor; // Add Radzen Blazor namespace

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// For hosted Blazor WebAssembly, use the default HttpClient base address
// which will be the same origin as the hosting server
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000/") });

// Add Radzen services
builder.Services.AddRadzenComponents();

// Register DebateApiClient
// builder.Services.AddScoped<DebateApiClient>();

await builder.Build().RunAsync();
