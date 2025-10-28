using './main.bicep'

// Default parameters for azd deployment
param environmentName = 'prod'
param location = 'eastus'
param resourceGroupName = 'PoDebateRap'
param sharedResourceGroupName = 'PoShared'
param sharedAppServicePlanName = 'PoShared'
