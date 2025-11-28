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

@description('Name of the existing Key Vault')
param keyVaultName string = 'kv-podebaterap'

@description('Email address for budget alerts')
param budgetAlertEmail string = 'punkouter26@gmail.com'

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

// Reference existing Key Vault for secrets
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// App Service for Blazor WebAssembly Hosted App
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: baseName
  location: location
  tags: union(tags, {
    'azd-service-name': 'api'
  })
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v10.0'
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
        {
          name: 'Azure__ApplicationInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'Azure__KeyVault__VaultUri'
          value: 'https://${keyVaultName}.vault.azure.net/'
        }
        // Azure OpenAI Configuration - Key Vault References
        {
          name: 'Azure__OpenAI__Endpoint'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=OpenAI-Endpoint)'
        }
        {
          name: 'Azure__OpenAI__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=OpenAI-ApiKey)'
        }
        {
          name: 'Azure__OpenAI__DeploymentName'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=OpenAI-DeploymentName)'
        }
        // Azure Speech Services Configuration - Key Vault References
        {
          name: 'Azure__Speech__Region'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Speech-Region)'
        }
        {
          name: 'Azure__Speech__Endpoint'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Speech-Endpoint)'
        }
        {
          name: 'Azure__Speech__SubscriptionKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Speech-SubscriptionKey)'
        }
        // NewsAPI Configuration - Key Vault Reference
        {
          name: 'NewsApi__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=NewsApi-ApiKey)'
        }
      ]
    }
  }
}

// Role Assignment: Key Vault Secrets User for App Service Managed Identity
resource keyVaultSecretUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appService.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Action Group for budget alerts
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'ag-${baseName}-budget'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'BudgetAlert'
    enabled: true
    emailReceivers: [
      {
        name: 'BudgetOwner'
        emailAddress: budgetAlertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

// $5 Monthly Budget with 80% threshold alert
resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: 'budget-${baseName}'
  properties: {
    category: 'Cost'
    amount: 5
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: '2025-01-01'
      endDate: '2030-12-31'
    }
    filter: {
      dimensions: {
        name: 'ResourceGroupName'
        operator: 'In'
        values: [
          resourceGroup().name
        ]
      }
    }
    notifications: {
      actual_GreaterThan_80_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 80
        thresholdType: 'Actual'
        contactEmails: [
          budgetAlertEmail
        ]
        contactGroups: [
          actionGroup.id
        ]
      }
      forecasted_GreaterThan_100_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Forecasted'
        contactEmails: [
          budgetAlertEmail
        ]
        contactGroups: [
          actionGroup.id
        ]
      }
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
