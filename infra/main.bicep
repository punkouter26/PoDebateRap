targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (e.g., dev, prod)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string = 'eastus'

@description('Name of the resource group for app-specific resources')
param resourceGroupName string = 'PoDebateRap'

@description('Name of the shared resource group')
param sharedResourceGroupName string = 'PoShared'

// Tags that should be applied to all resources
var tags = {
  'azd-env-name': environmentName
  'app': 'PoDebateRap'
}

// Create app-specific resource group
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Reference shared resource group
resource sharedRg 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: sharedResourceGroupName
}

// Deploy app-specific resources
module resources 'resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    tags: tags
    environmentName: environmentName
    sharedResourceGroupName: sharedResourceGroupName
  }
}

output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = resources.outputs.containerAppsEnvironmentId
output WEB_URI string = resources.outputs.webUri
