# Phase 5: Azure Deployment Complete ✅

## Summary
Successfully deployed PoDebateRap to Azure App Service with GitHub Actions CI/CD pipeline.

## Resources Created

### PoShared Resource Group (Shared Infrastructure)
- **App Service Plan**: PoShared
  - SKU: F1 (Free tier)
  - Location: East US 2
  - Capacity: 10 web apps maximum
  - Cost: Free

### PoDebateRap Resource Group (Application Resources)
- **Web App**: PoDebateRap
  - URL: https://podebaterap.azurewebsites.net
  - Runtime: .NET 9.0
  - App Service Plan: PoShared (cross-RG reference)
  
- **Azure OpenAI**: PoDebateRap-OpenAI
  - Endpoint: https://eastus2.api.cognitive.microsoft.com/
  - Model Deployed: gpt-4o (2024-08-06)
  - Deployment Name: gpt-4o
  - SKU: S0 (Standard)
  - API Key: Stored in appsettings.json
  
- **Speech Services**: PoDebateRap-Speech
  - Region: eastus2
  - Endpoint: https://eastus2.api.cognitive.microsoft.com/
  - SKU: F0 (Free tier)
  - Subscription Key: Stored in appsettings.json

## Configuration

### Updated Files
1. **appsettings.json** - Production Azure credentials (safe in private repo)
   - Azure OpenAI endpoint and API key
   - Azure Speech region and subscription key
   - Existing storage and NewsAPI keys retained

2. **.github/workflows/azure-deploy.yml** - CI/CD pipeline
   - Triggers on push to main branch
   - Builds, tests, and deploys automatically
   - Includes health check verification

### GitHub Actions Workflow
The workflow is configured and pushed to GitHub. To enable automated deployments:

1. **Set up GitHub Secret** (Required for first deployment):
   ```powershell
   # Get the publish profile
   az webapp deployment list-publishing-profiles --name PoDebateRap --resource-group PoDebateRap --xml
   
   # Copy the XML output
   # Go to: https://github.com/punkouter25/PoDebateRap/settings/secrets/actions
   # Click "New repository secret"
   # Name: AZURE_WEBAPP_PUBLISH_PROFILE
   # Value: Paste the XML publish profile
   ```

2. **Alternative: Download Publish Profile from Azure Portal**:
   - Go to https://portal.azure.com
   - Navigate to PoDebateRap App Service
   - Click "Download publish profile" button
   - Open the .PublishSettings file
   - Copy the entire XML content
   - Add as GitHub secret named `AZURE_WEBAPP_PUBLISH_PROFILE`

## Manual Deployment (Alternative)

If GitHub Actions is not set up yet, you can deploy manually:

```powershell
# Build the application
dotnet publish Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj -c Release -o ./publish

# Create deployment package
cd publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
cd ..

# Deploy to Azure
az webapp deploy --name PoDebateRap --resource-group PoDebateRap --src-path deploy.zip --type zip

# Clean up
Remove-Item -Path publish -Recurse -Force
Remove-Item -Path deploy.zip -Force
```

## Health Check

Test the application health:
```powershell
curl https://podebaterap.azurewebsites.net/api/health
```

Expected response (when all services are configured):
```json
{
  "isHealthy": true,
  "checks": {
    "API": "API is running and healthy",
    "Storage": "Data connection service is available",
    "AzureOpenAI": "Azure OpenAI service is working",
    "TextToSpeech": "Text-to-Speech service is working",
    "Internet": "Internet connection is working",
    "NewsAPI": "News service is reachable"
  }
}
```

## Current Status

✅ PoShared Resource Group created
✅ PoShared App Service Plan created (F1 tier)
✅ PoDebateRap Web App created
✅ Azure OpenAI resource created with gpt-4o model
✅ Azure Speech Services resource created (Free tier)
✅ Production credentials configured in appsettings.json
✅ GitHub Actions workflow created and pushed
✅ Changes committed and pushed to GitHub
⏳ GitHub secret setup (manual step required)
⏳ First automated deployment (pending secret setup)

## Next Steps

1. **Set up GitHub Secret** (5 minutes):
   - Follow instructions above to add AZURE_WEBAPP_PUBLISH_PROFILE secret
   - This enables automated deployments on every push to main

2. **Verify Deployment**:
   - Push a change to trigger the workflow
   - Watch the Actions tab: https://github.com/punkouter25/PoDebateRap/actions
   - Verify health endpoint after deployment

3. **Optional: Create Azure Storage Account**:
   - Currently using existing pohappytrumpstorage
   - Consider creating PoDebateRap-specific storage if needed

## Troubleshooting

### Disk Space Issues (F1 Tier)
If deployment fails with "not enough space on disk":
- The F1 tier has 1GB storage limit
- Restart the app: `az webapp restart --name PoDebateRap --resource-group PoDebateRap`
- Use GitHub Actions for deployment (more efficient than manual zip deploy)

### Health Check Failures
- **Azure OpenAI**: Verify endpoint format (regional vs custom subdomain)
- **Speech Services**: Check region matches (eastus2)
- **Storage**: Ensure connection string is valid
- **NewsAPI**: Verify API key is active

### GitHub Actions Failures
- Check that AZURE_WEBAPP_PUBLISH_PROFILE secret is set
- Verify .NET 9.0 is specified in workflow (preview version)
- Check Actions tab for detailed error logs

## Cost Breakdown

| Resource | SKU | Monthly Cost |
|----------|-----|--------------|
| App Service Plan (PoShared) | F1 | $0 (Free) |
| Azure OpenAI | S0 | Pay-per-use* |
| Speech Services | F0 | $0 (Free tier) |
| Azure Storage | Existing | Shared cost |

*Azure OpenAI charges based on token usage (~$0.0025/1K tokens for GPT-4o)

## Architecture Benefits

1. **Centralized Infrastructure**: PoShared plan can host up to 10 apps
2. **Resource Isolation**: Each app has dedicated Azure AI resources
3. **CI/CD Ready**: Automated deployments on every commit
4. **Health Monitoring**: Built-in health checks for all services
5. **Secure Configuration**: Credentials in private repo appsettings.json

## Completion

Phase 5 deployment infrastructure is complete. The application is ready for automated deployments once the GitHub secret is configured.

**Live Application**: https://podebaterap.azurewebsites.net
**Repository**: https://github.com/punkouter25/PoDebateRap
**GitHub Actions**: https://github.com/punkouter25/PoDebateRap/actions
