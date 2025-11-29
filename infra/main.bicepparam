using './main.bicep'

// Default parameters for azd deployment
param environmentName = 'prod'
param location = 'eastus2'
param resourceGroupName = 'PoDebateRap'
param sharedResourceGroupName = 'PoShared'
param sharedAppServicePlanName = 'PoShared'
