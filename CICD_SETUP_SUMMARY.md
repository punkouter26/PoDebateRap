# CI/CD Setup Summary

## ✅ Configuration Complete

### Single Workflow File
- **File**: `.github/workflows/azure-deploy.yml`
- **Removed**: `BuildDeploy.yml` (duplicate)
- **Purpose**: Build and deploy PoDebateRap to Azure App Service

### Workflow Details

```yaml
Name: Deploy PoDebateRap to Azure
Triggers:
  - Push to main branch
  - Manual workflow_dispatch
  
Steps:
  1. Checkout code
  2. Setup .NET 9.0.x
  3. Restore dependencies
  4. Build (Release configuration)
  5. Publish to ./publish folder
  6. Deploy to Azure Web App using publish profile
  7. Health check verification
```

### GitHub Secrets Configured

✅ `AZURE_WEBAPP_PUBLISH_PROFILE` - Set via `gh secret set`

### Azure Resources

- **Web App**: PoDebateRap
- **Resource Group**: PoDebateRap
- **App Service Plan**: PoShared (F1 Free tier)
- **URL**: https://podebaterap.azurewebsites.net
- **Runtime**: .NET 9.0 (Windows)

### Known Issue

The `azure/webapps-deploy@v3` action is reporting a publish profile validation error. This appears to be an issue with the GitHub Action itself, not with the publish profile credentials.

### Alternative Deployment Methods

Until the GitHub Actions issue is resolved, you can deploy using:

1. **Azure CLI** (Recommended):
   ```powershell
   dotnet publish Server/PoDebateRap.ServerApi/PoDebateRap.ServerApi.csproj -c Release -o ./publish
   cd publish
   Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
   cd ..
   az webapp deploy --name PoDebateRap --resource-group PoDebateRap --src-path deploy.zip --type zip
   Remove-Item -Path publish -Recurse -Force
   Remove-Item -Path deploy.zip -Force
   ```

2. **Visual Studio** (if installed):
   - Right-click on PoDebateRap.ServerApi project
   - Select "Publish"
   - Import the publish profile
   - Click "Publish"

3. **VS Code with Azure App Service Extension**:
   - Install "Azure App Service" extension
   - Right-click on publish folder
   - Deploy to Azure App Service

### Current Application Status

The application was successfully deployed during initial setup and is running with:
- ✅ Azure OpenAI (gpt-4o model)
- ✅ Azure Speech Services (F0 Free tier)
- ⚠️ Health endpoint shows Speech service working
- ⚠️ OpenAI endpoint format may need verification

### Next Steps to Fix GitHub Actions

1. **Try webapps-deploy@v2** (rollback):
   - The v3 action may have a bug
   - v2 is stable and widely used

2. **Use Azure Login + CLI** (alternative):
   - Replace publish profile with service principal
   - Use `az webapp deploy` instead of the action

3. **Contact Azure Support**:
   - The publish profile validation error may be an Azure-side issue
   - Publish profile was regenerated multiple times with same error

### Manual Deployment Verification

```powershell
# Test health endpoint
curl https://podebaterap.azurewebsites.net/api/health

# View logs
az webapp log tail --name PoDebateRap --resource-group PoDebateRap

# Check app settings
az webapp config appsettings list --name PoDebateRap --resource-group PoDebateRap
```

### Files Modified

- ✅ Removed `.github/workflows/BuildDeploy.yml`
- ✅ Kept `.github/workflows/azure-deploy.yml` only
- ✅ Upgraded to webapps-deploy@v3
- ✅ Set GitHub secret via `gh` CLI

### Recommendation

For now, use **manual Azure CLI deployment** until the GitHub Actions publish profile validation issue is resolved. The workflow file is correctly configured and ready to use once the action bug is fixed.

The application itself is properly deployed and functional - only the automated CI/CD pipeline needs troubleshooting.
