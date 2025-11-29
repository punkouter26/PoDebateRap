using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoDebateRap.Client;
using PoDebateRap.Client.Services;
using Radzen;
using Radzen.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// For hosted Blazor WebAssembly, use the hosting server's base address
// This will be localhost in development and the Azure URL in production
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Radzen services (includes NotificationService, DialogService, TooltipService, ContextMenuService)
builder.Services.AddRadzenComponents();

// Register custom app services
builder.Services.AddScoped<AppNotificationService>();

await builder.Build().RunAsync();
