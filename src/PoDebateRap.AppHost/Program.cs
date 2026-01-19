var builder = DistributedApplication.CreateBuilder(args);

// Azure Table Storage - uses local Azurite (npm) for development
// Connection string is configured in appsettings.Development.json

// Add environment variables for shared Azure services (from PoShared resource group)
// These are configured via appsettings.json and Key Vault in production
var web = builder.AddProject<Projects.PoDebateRap_Web>("web")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
