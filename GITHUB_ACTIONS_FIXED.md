# ✅ GitHub Actions CI/CD - FIXED!

## Solution Summary

### The Problem
The `azure/webapps-deploy@v3` action was failing with:
```
Deployment Failed, Error: Publish profile is invalid for app-name and slot-name provided.
```

### The Fix (Using Azure CLI)

**Steps taken:**

1. **Created Azure Service Principal** for GitHub Actions authentication:
   ```bash
   az ad sp create-for-rbac \
     --name "PoDebateRap-GitHub-Actions" \
     --role contributor \
     --scopes /subscriptions/f0504e26-451a-4249-8fb3-46270defdd5b/resourceGroups/PoDebateRap \
     --sdk-auth
   ```

2. **Set GitHub Secret** using `gh` CLI:
   ```powershell
   gh secret set AZURE_CREDENTIALS
   # Pasted the entire JSON output from step 1
   ```

3. **Updated Workflow** to use Azure CLI instead of webapps-deploy action:
   ```yaml
   - name: Login to Azure
     uses: azure/login@v1
     with:
       creds: ${{ secrets.AZURE_CREDENTIALS }}
   
   - name: Create deployment package
     run: |
       cd ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
       zip -r ../deploy.zip .
   
   - name: Deploy to Azure Web App
     run: |
       az webapp deploy \
         --name ${{ env.AZURE_WEBAPP_NAME }} \
         --resource-group ${{ env.AZURE_RESOURCE_GROUP }} \
         --src-path deploy.zip \
         --type zip \
         --async false
   ```

### ✅ Result

**First deployment after fix: SUCCESS!**
- Build: ✅ Passed
- Deploy: ✅ Passed  
- Health Check: ✅ Passed

Workflow run: https://github.com/punkouter25/PoDebateRap/actions/runs/18264615428

## Current CI/CD Status

✅ **Fully Operational**
- Triggers on push to `main` branch
- Manual trigger via `workflow_dispatch`
- Builds .NET 9.0 application
- Deploys to Azure App Service using Azure CLI
- Runs health check verification

## GitHub Secrets Configured

| Secret Name | Purpose | Status |
|-------------|---------|--------|
| `AZURE_CREDENTIALS` | Service principal for Azure CLI | ✅ Set |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | (Deprecated - not used anymore) | ⚠️ Can be deleted |

## Service Principal Details

- **Name**: PoDebateRap-GitHub-Actions
- **Role**: Contributor
- **Scope**: /subscriptions/f0504e26-451a-4249-8fb3-46270defdd5b/resourceGroups/PoDebateRap
- **Client ID**: caa6f961-ed33-4c09-9425-e98dc89ce423

## How to Deploy

### Automatic (Recommended)
Push any commit to `main` branch:
```bash
git add .
git commit -m "Your changes"
git push
```

### Manual Trigger
Using GitHub CLI:
```bash
gh workflow run azure-deploy.yml
```

Or via GitHub UI:
- Go to https://github.com/punkouter25/PoDebateRap/actions
- Click "Deploy PoDebateRap to Azure"
- Click "Run workflow"

## Monitoring Deployments

### Using GitHub CLI
```bash
# List recent workflow runs
gh run list --workflow=azure-deploy.yml

# Watch a specific run
gh run watch <RUN_ID>

# Watch latest run
gh run watch
```

### Using GitHub UI
https://github.com/punkouter25/PoDebateRap/actions

## Known Issues

### F1 Tier Disk Space
The Free tier (F1) has limited disk space (1GB). If deployment fails with "not enough space on disk":

**Solution:**
```bash
# Restart the app to clear temp files
az webapp restart --name PoDebateRap --resource-group PoDebateRap

# Then retry deployment
gh workflow run azure-deploy.yml
```

### Health Check Showing OpenAI Error
The current deployment shows:
```json
"AzureOpenAI": "Azure OpenAI Service Check failed: No such host is known. (poai-eastus2.openai.azure.com:443)"
```

**Cause**: Old endpoint format in deployed code.

**Solution**: Already fixed in `appsettings.json` (commit c7e547b). Next successful deployment will resolve this.

**Correct endpoint**: `https://eastus2.api.cognitive.microsoft.com/`

## Cleanup (Optional)

Remove the old publish profile secret (no longer used):
```bash
gh secret delete AZURE_WEBAPP_PUBLISH_PROFILE
```

## Advantages of This Approach

1. ✅ **More Reliable**: Azure CLI is more stable than webapps-deploy action
2. ✅ **Better Control**: Can use full Azure CLI capabilities
3. ✅ **Easier Debugging**: Clear error messages
4. ✅ **Standard Authentication**: Service principal is Azure's recommended approach
5. ✅ **Future-Proof**: Works with all Azure services, not just App Service

## Workflow File Location

`.github/workflows/azure-deploy.yml`

**Single workflow** - no duplicates ✅

## Next Steps

1. ✅ CI/CD is working - no action needed
2. ⚠️ If disk space issues occur, restart app before deploying
3. 📝 Consider upgrading to B1 tier ($13/month) for more disk space if needed

## Summary

**Problem**: Publish profile authentication failing  
**Solution**: Switched to Azure CLI with service principal  
**Status**: ✅ FIXED AND WORKING  
**First Success**: Deployment run #18264615428  
**Date Fixed**: October 5, 2025
