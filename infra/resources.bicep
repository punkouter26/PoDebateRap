@description('Location for all resources')
param location string

@description('Tags to apply to all resources')
param tags object

@description('Name of the environment')
param environmentName string

@description('Name of the shared resource group')
param sharedResourceGroupName string

// Reference existing shared Container Apps Environment
resource sharedContainerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: 'cae-poshared'
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing shared Container Registry
resource sharedContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: 'acrposhared'
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Managed Identity
resource sharedManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: 'mi-poshared-apps'
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Key Vault
resource sharedKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-poshared'
  scope: resourceGroup(sharedResourceGroupName)
}

// App-specific Storage Account (already created)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: 'stpodebaterap'
}

// Container App for the web application
resource webContainerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'ca-podebaterap-web'
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${sharedManagedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: sharedContainerAppsEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: '${sharedContainerRegistry.name}.azurecr.io'
          identity: sharedManagedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'storage-connection'
          keyVaultUrl: 'https://kv-poshared.vault.azure.net/secrets/PoDebateRap-StorageConnection'
          identity: sharedManagedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: '${sharedContainerRegistry.name}.azurecr.io/podebaterap-web:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'Azure__KeyVault__VaultUri'
              value: 'https://kv-poshared.vault.azure.net/'
            }
            {
              name: 'Azure__OpenAI__Endpoint'
              value: 'https://poshared-openai.cognitiveservices.azure.com/'
            }
            {
              name: 'Azure__OpenAI__DeploymentName'
              value: 'gpt-4o'
            }
            {
              name: 'Azure__Speech__Endpoint'
              value: 'https://pofoxnews-shared-speech.cognitiveservices.azure.com/'
            }
            {
              name: 'Azure__Speech__Region'
              value: 'eastus'
            }
            {
              name: 'Azure__Storage__AccountName'
              value: storageAccount.name
            }
            {
              name: 'Azure__ManagedIdentity__ClientId'
              value: sharedManagedIdentity.properties.clientId
            }
            {
              name: 'ConnectionStrings__tables'
              secretRef: 'storage-connection'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: 'InstrumentationKey=0dd09920-6fad-4ebf-a399-e7440a0eabd5;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

output containerRegistryEndpoint string = '${sharedContainerRegistry.name}.azurecr.io'
output containerAppsEnvironmentId string = sharedContainerAppsEnv.id
output webUri string = 'https://${webContainerApp.properties.configuration.ingress.fqdn}'
