targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (e.g., dev, prod)')
param environmentName string = 'prod'

@minLength(1)
@description('Primary location for all resources')
param location string = 'eastus2'

@description('Name of the resource group - defaults to solution name')
param resourceGroupName string = 'PoDebateRap'

@description('Name of the shared resource group containing the F1 App Service Plan')
param sharedResourceGroupName string = 'PoShared'

@description('Name of the existing F1 tier App Service Plan in the shared resource group')
param sharedAppServicePlanName string = 'PoShared'

// Generate unique resource names based on solution name
var baseName = 'PoDebateRap'
var resourceToken = toLower(uniqueString(subscription().id, baseName, location))
var tags = {
  'azd-env-name': environmentName
  'app-name': baseName
  'managed-by': 'azd'
}

// Resource group - use existing or create new
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: resourceGroupName
}

// Main resources module
module resources './resources.bicep' = {
  scope: rg
  params: {
    location: location
    baseName: baseName
    resourceToken: resourceToken
    tags: tags
    appServicePlanId: '/subscriptions/${subscription().subscriptionId}/resourceGroups/${sharedResourceGroupName}/providers/Microsoft.Web/serverfarms/${sharedAppServicePlanName}'
  }
}

// Outputs
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_TENANT_ID string = tenant().tenantId
output APP_SERVICE_NAME string = resources.outputs.appServiceName
output APP_SERVICE_URL string = resources.outputs.appServiceUrl
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.applicationInsightsConnectionString
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.storageAccountName
