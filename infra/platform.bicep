@description('Azure region')
param location string = 'northeurope'

@description('Name prefix for resources')
param prefix string = 'roomflow'

@description('SQL administrator login')
param sqlAdminLogin string = 'roomflowadmin'

@description('SQL administrator password')
@secure()
param sqlAdminPassword string

@description('Object ID of the GitHub Actions OIDC service principal')
param deployerPrincipalId string

var unique = uniqueString(resourceGroup().id)
var acrName = toLower(take('${prefix}${unique}', 50))
var sqlServerName = toLower(take('${prefix}-sql-${unique}', 63))
var swaName = toLower(take('${prefix}-swa-${unique}', 60))
var logAnalyticsName = take('${prefix}-logs-${unique}', 63)
var environmentName = take('${prefix}-env-${unique}', 60)
var identityName = take('${prefix}-aca-id-${unique}', 64)
var appInsightsName = take('${prefix}-ai-${unique}', 63)
var keyVaultName = toLower(take('kv${prefix}${unique}', 24))

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: 'francecentral'
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    serverName: sqlServerName
    databaseName: 'RoomFlow'
    entraAdminPrincipalId: deployerPrincipalId
    entraAdminLogin: 'github-oidc'
  }
}

module acr 'modules/acr.bicep' = {
  name: 'acr'
  params: {
    location: location
    acrName: acrName
    identityName: identityName
  }
}

module containerAppsEnvironment 'modules/containerAppsEnvironment.bicep' = {
  name: 'container-apps-environment'
  params: {
    location: location
    logAnalyticsName: logAnalyticsName
    environmentName: environmentName
  }
}

module appInsights 'modules/appInsights.bicep' = {
  name: 'app-insights'
  params: {
    location: location
    name: appInsightsName
    workspaceResourceId: containerAppsEnvironment.outputs.logAnalyticsWorkspaceId
  }
}

module swa 'modules/swa.bicep' = {
  name: 'swa'
  params: {
    location: 'eastus2'
    name: swaName
  }
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'key-vault'
  params: {
    location: location
    vaultName: keyVaultName
    identityPrincipalId: acr.outputs.identityPrincipalId
    deployerPrincipalId: deployerPrincipalId
  }
}

output acrName string = acr.outputs.acrName
output acrLoginServer string = acr.outputs.loginServer
output identityId string = acr.outputs.identityId
output identityClientId string = acr.outputs.identityClientId
output identityPrincipalId string = acr.outputs.identityPrincipalId
output identityName string = acr.outputs.identityName
output environmentId string = containerAppsEnvironment.outputs.environmentId
output appInsightsName string = appInsights.outputs.name
output sqlFqdn string = sql.outputs.fullyQualifiedDomainName
output sqlDatabaseName string = sql.outputs.databaseName
output sqlServerName string = sql.outputs.serverName
output sqlAdminLogin string = sqlAdminLogin
output swaName string = swa.outputs.name
output swaHostname string = swa.outputs.defaultHostname
output keyVaultName string = keyVault.outputs.vaultName
output keyVaultUri string = keyVault.outputs.vaultUri
