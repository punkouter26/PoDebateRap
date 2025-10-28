// Parameters
@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources (PoDebateRap)')
param baseName string

@description('Unique token for resource naming')
param resourceToken string

@description('Tags to apply to all resources')
param tags object = {}

@description('Resource ID of the existing F1 tier App Service Plan')
param appServicePlanId string

// Log Analytics Workspace for Application Insights
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'law-${baseName}-${resourceToken}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018' // Pay-as-you-go tier
    }
    retentionInDays: 30 // Minimum retention for cost savings
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights for monitoring
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${baseName}-${resourceToken}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    RetentionInDays: 30
    IngestionMode: 'LogAnalytics' // Use Log Analytics workspace
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Azure Storage Account for Table Storage (debate data)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'stdebaterap${resourceToken}' // Must be globally unique and 3-24 chars
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS' // Cheapest option - Locally Redundant Storage
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true // Required for connection string access
    publicNetworkAccess: 'Enabled'
  }
}

// Table Service for Storage Account
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// App Service for Blazor WebAssembly Hosted App
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: baseName
  location: location
  tags: union(tags, {
    'azd-service-name': 'api'
  })
  kind: 'app'
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: true  // Required for F1 tier
      alwaysOn: false              // Must be disabled for F1 tier
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/api/health'
      cors: {
        allowedOrigins: [
          'https://${baseName}.azurewebsites.net'
        ]
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Azure__StorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        // Note: OpenAI, Speech, and NewsAPI keys need to be configured separately as they're external services
        {
          name: 'Azure__ApplicationInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
      ]
    }
  }
}

// Outputs
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output storageAccountName string = storageAccount.name
