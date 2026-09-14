@description('Azure region for Service Bus and related regional resources')
param location string

@description('Azure region for Function App, plan, identity and host storage (Y1 quota)')
param functionLocation string = location

@description('Name prefix')
param prefix string

@description('Principal ID of the Container App user-assigned identity (Service Bus sender)')
param apiIdentityPrincipalId string

@description('Object ID of the GitHub Actions OIDC service principal (blob package upload)')
param deployerPrincipalId string

@description('Application Insights connection string for the Function App')
@secure()
param applicationInsightsConnectionString string

var unique = uniqueString(resourceGroup().id)
var functionUnique = uniqueString(resourceGroup().id, functionLocation)
var serviceBusNamespaceName = take(toLower('${prefix}-sb-${unique}'), 50)
var queueName = 'booking-events'
var emailServiceName = take(toLower('${prefix}-email-${unique}'), 63)
var communicationServiceName = take(toLower('${prefix}-acs-${unique}'), 63)
var storageName = take('st${prefix}${functionUnique}', 24)
var functionPlanName = take('${prefix}-func-plan-${functionUnique}', 40)
var functionAppName = take(toLower('${prefix}-func-${functionUnique}'), 60)
var functionIdentityName = take('${prefix}-func-id-${functionUnique}', 64)
var functionReleasesContainerName = 'function-releases'
var functionPackageBlobName = 'latest.zip'
var functionPackageBlobUrl = 'https://${storageName}.blob.${az.environment().suffixes.storage}/${functionReleasesContainerName}/${functionPackageBlobName}'

var serviceBusSenderRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
)
var serviceBusReceiverRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)
// Communication and Email Service Owner — least built-in role suited to ACS Email send
var communicationEmailOwnerRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '09976791-48a7-449e-bb21-39d1a415f350'
)
var storageBlobDataOwnerRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
)
var storageBlobDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
)
var storageQueueDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '974c5e8b-45b9-4653-9b13-2a4ce91b0b2c'
)
var storageTableDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '0a9a7e1f-b9d0-4cc4-a60d-0311b340aba9'
)

resource functionIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: functionIdentityName
  location: functionLocation
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource bookingEventsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

resource apiSenderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, apiIdentityPrincipalId, serviceBusSenderRoleId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: serviceBusSenderRoleId
    principalId: apiIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource functionReceiverRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, functionIdentity.id, serviceBusReceiverRoleId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: serviceBusReceiverRoleId
    principalId: functionIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource emailService 'Microsoft.Communication/emailServices@2023-06-01-preview' = {
  name: emailServiceName
  location: 'global'
  properties: {
    dataLocation: 'Europe'
  }
}

resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-06-01-preview' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource communicationService 'Microsoft.Communication/communicationServices@2023-06-01-preview' = {
  name: communicationServiceName
  location: 'global'
  properties: {
    dataLocation: 'Europe'
    linkedDomains: [
      emailDomain.id
    ]
  }
}

resource functionEmailRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(communicationService.id, functionIdentity.id, communicationEmailOwnerRoleId)
  scope: communicationService
  properties: {
    roleDefinitionId: communicationEmailOwnerRoleId
    principalId: functionIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: functionLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

resource storageBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'
}

resource functionReleasesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageBlobService
  name: functionReleasesContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource functionStorageBlobOwnerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionIdentity.id, storageBlobDataOwnerRoleId)
  scope: storage
  properties: {
    roleDefinitionId: storageBlobDataOwnerRoleId
    principalId: functionIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource functionStorageQueueContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionIdentity.id, storageQueueDataContributorRoleId)
  scope: storage
  properties: {
    roleDefinitionId: storageQueueDataContributorRoleId
    principalId: functionIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource functionStorageTableContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionIdentity.id, storageTableDataContributorRoleId)
  scope: storage
  properties: {
    roleDefinitionId: storageTableDataContributorRoleId
    principalId: functionIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource deployerStorageBlobContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, deployerPrincipalId, storageBlobDataContributorRoleId)
  scope: storage
  properties: {
    roleDefinitionId: storageBlobDataContributorRoleId
    principalId: deployerPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource functionPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: functionPlanName
  location: functionLocation
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: functionLocation
  kind: 'functionapp,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${functionIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|10.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storage.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: functionIdentity.properties.clientId
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: functionPackageBlobUrl
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE_BLOB_MI_RESOURCE_ID'
          value: functionIdentity.id
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: functionIdentity.properties.clientId
        }
        {
          name: 'ServiceBus__fullyQualifiedNamespace'
          value: '${serviceBusNamespace.name}.servicebus.windows.net'
        }
        {
          name: 'ServiceBusQueueName'
          value: queueName
        }
        {
          name: 'Email__Endpoint'
          value: 'https://${communicationService.name}.communication.azure.com'
        }
        {
          name: 'Email__SenderAddress'
          value: 'DoNotReply@${emailDomain.properties.fromSenderDomain}'
        }
      ]
    }
  }
  dependsOn: [
    bookingEventsQueue
    functionReleasesContainer
    functionStorageBlobOwnerRole
    functionStorageQueueContributorRole
    functionStorageTableContributorRole
  ]
}

output serviceBusFullyQualifiedNamespace string = '${serviceBusNamespace.name}.servicebus.windows.net'
output serviceBusQueueName string = queueName
output functionAppName string = functionApp.name
output functionStorageAccountName string = storage.name
output functionPackageBlobUrl string = functionPackageBlobUrl
output functionReleasesContainerName string = functionReleasesContainerName
output functionPackageBlobName string = functionPackageBlobName
output emailEndpoint string = 'https://${communicationService.name}.communication.azure.com'
output emailSenderAddress string = 'DoNotReply@${emailDomain.properties.fromSenderDomain}'
