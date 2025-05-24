param location string = resourceGroup().location
param webAppName string = 'PoDebateRapWebApp'
param storageAccountName string = 'podebaterapstorage'
param applicationInsightsConnectionString string // New parameter for App Insights connection string

// Updated on May 24, 2025 - Testing CI/CD pipeline

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppName
  location: location
  properties: {
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0' // Specify .NET 9.0 for Linux App Service
      appSettings: [
        {
          name: 'Azure__StorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
      ]
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS' // Locally-redundant storage
  }
  kind: 'StorageV2' // General-purpose v2
  properties: {
    accessTier: 'Hot'
  }
}

output webAppHostName string = webApp.properties.defaultHostName
output storageAccountName string = storageAccount.name
