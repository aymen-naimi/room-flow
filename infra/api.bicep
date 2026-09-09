@description('Azure region')
param location string = 'northeurope'

@description('Name prefix for the Container App')
param prefix string = 'roomflow'

@description('Container Apps environment resource ID')
param environmentId string

@description('ACR login server')
param acrLoginServer string

@description('User-assigned identity resource ID with AcrPull and Key Vault Secrets User')
param identityId string

@description('Full image reference including tag')
param apiImage string

@description('Key Vault URI used for Container App secret references')
param keyVaultUri string

@description('Allowed CORS origin (Static Web App HTTPS origin)')
param corsOrigin string

@description('Application Insights component name in this resource group')
param appInsightsName string

var unique = uniqueString(resourceGroup().id)
var containerAppName = take('${prefix}-api-${unique}', 32)

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

module containerApp 'modules/containerApp.bicep' = {
  name: 'container-app'
  params: {
    location: location
    name: containerAppName
    environmentId: environmentId
    acrLoginServer: acrLoginServer
    identityId: identityId
    apiImage: apiImage
    keyVaultUri: keyVaultUri
    corsOrigin: corsOrigin
    applicationInsightsConnectionString: appInsights.properties.ConnectionString
  }
}

output apiFqdn string = containerApp.outputs.fqdn
output containerAppName string = containerApp.outputs.name
