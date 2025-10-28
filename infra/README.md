# PoDebateRap Azure Infrastructure

This directory contains the Bicep Infrastructure as Code (IaC) files for deploying PoDebateRap to Azure.

## Architecture

The infrastructure follows Azure best practices with minimal resources and cost-optimized tiers:

- **Resource Group**: `PoDebateRap` (standardized naming based on solution name)
- **App Service Plan**: Existing F1 (Free) tier plan named `PoShared` in the `PoShared` resource group
- **App Service**: Blazor WebAssembly hosted application
- **Application Insights**: Web application monitoring and telemetry
- **Log Analytics Workspace**: Centralized logging for Application Insights
- **Storage Account**: Azure Table Storage for debate data (rappers, stats)

## Deployment

### Prerequisites

1. Azure CLI installed and authenticated (`az login`)
2. Azure Developer CLI (azd) installed
3. .NET 9.0 SDK installed
4. Access to the `PoShared` resource group with the F1 App Service Plan

### Deploy to Azure

The entire infrastructure is deployed with a single command:

```bash
azd up
```

This will:
1. Build the .NET application
2. Provision all Azure resources using Bicep
3. Deploy the application to Azure App Service
4. Output the application URL

### Preview Changes Only

To see what resources will be created/modified without deploying:

```bash
azd provision --preview
```

### Infrastructure Only

To provision resources without deploying code:

```bash
azd provision
```

### Code Deployment Only

To deploy code to existing infrastructure:

```bash
azd deploy
```

## Configuration

### Default Parameters

All parameters have default values for zero-prompt deployment:

- **Environment Name**: `prod`
- **Location**: `eastus2` (matches existing shared plan)
- **Resource Group**: `PoDebateRap`
- **Shared Resource Group**: `PoShared`
- **Shared App Service Plan**: `PoShared`

### Customize Parameters

Edit `main.bicepparam` to change defaults, or override at deployment:

```bash
azd provision --set location=westus2
```

## Local Development vs. Cloud

### Storage
- **Local**: Uses Azurite with `UseDevelopmentStorage=true`
- **Cloud**: Uses Azure Storage Account (automatically configured)

### Application Settings

The following app settings are automatically configured in Azure:

- `ASPNETCORE_ENVIRONMENT`: Production
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Auto-configured from Application Insights
- `Azure__StorageConnectionString`: Auto-configured from Storage Account
- `Azure__ApplicationInsights__ConnectionString`: Auto-configured

### External Services (Manual Configuration Required)

The following services require manual configuration via Azure Portal:

1. **Azure OpenAI**:
   - Set `Azure__OpenAI__Endpoint`
   - Set `Azure__OpenAI__ApiKey`
   - Set `Azure__OpenAI__DeploymentName`

2. **Azure Speech Services**:
   - Set `Azure__Speech__Region`
   - Set `Azure__Speech__Endpoint`
   - Set `Azure__Speech__SubscriptionKey`

3. **News API**:
   - Set `NewsApi__ApiKey`

## Resource Naming Convention

- App Service: `PoDebateRap` (solution name)
- Application Insights: `appi-PoDebateRap-{uniqueToken}`
- Log Analytics: `law-PoDebateRap-{uniqueToken}`
- Storage Account: `stdebaterap{uniqueToken}` (lowercase, no special chars)

The `uniqueToken` is generated from subscription ID, base name, and location to ensure global uniqueness.

## Cost Optimization

All resources use the cheapest/free tiers:

- **App Service Plan**: F1 (Free) - Shared, reused across projects
- **Application Insights**: Pay-as-you-go with 30-day retention
- **Log Analytics Workspace**: Pay-as-you-go
- **Storage Account**: Standard LRS (Locally Redundant Storage)

Expected monthly cost: ~$0-5 (excluding external services like OpenAI/Speech)

## Health Checks

The App Service is configured with a health check endpoint:

- **Path**: `/api/health`
- **Checks**: Azure Table Storage, OpenAI, Text-to-Speech, News API
- **Endpoints**: 
  - `/api/health` - Overall health
  - `/api/health/live` - Liveness probe
  - `/api/health/ready` - Readiness probe

## Troubleshooting

### Shared App Service Plan Not Found

If deployment fails with "ResourceNotFound" for the App Service Plan:

1. Verify the `PoShared` resource group exists
2. Verify the `PoShared` App Service Plan exists in that resource group
3. Verify it's an F1 (Free) tier plan
4. Verify you have access to the resource group

### Storage Account Name Conflict

Storage account names must be globally unique. If deployment fails:

1. The `resourceToken` in the name ensures uniqueness per subscription/location
2. Try a different location if needed
3. The deployment will auto-generate a unique name

## Files

- **main.bicep**: Entry point, defines subscription-level deployment
- **resources.bicep**: All resource definitions and configurations
- **main.bicepparam**: Default parameter values
- **abbreviations.json**: Resource naming abbreviations (legacy, not currently used)
