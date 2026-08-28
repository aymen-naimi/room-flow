@description('Azure region')
param location string = 'northeurope'

@description('Name prefix for resources')
param prefix string = 'roomflow'

@description('SQL administrator login')
param sqlAdminLogin string = 'roomflowadmin'

@description('SQL administrator password')
@secure()
param sqlAdminPassword string

var unique = uniqueString(resourceGroup().id)
var acrName = toLower(take('${prefix}${unique}', 50))
var sqlServerName = toLower(take('${prefix}-sql-${unique}', 63))
var swaName = toLower(take('${prefix}-swa-${unique}', 60))
var logAnalyticsName = take('${prefix}-logs-${unique}', 63)
var environmentName = take('${prefix}-env-${unique}', 60)
var identityName = take('${prefix}-aca-id-${unique}', 64)

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    serverName: sqlServerName
    databaseName: 'RoomFlow'
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

module swa 'modules/swa.bicep' = {
  name: 'swa'
  params: {
    location: location
    name: swaName
  }
}

output acrName string = acr.outputs.acrName
output acrLoginServer string = acr.outputs.loginServer
output identityId string = acr.outputs.identityId
output environmentId string = containerAppsEnvironment.outputs.environmentId
output sqlFqdn string = sql.outputs.fullyQualifiedDomainName
output sqlDatabaseName string = sql.outputs.databaseName
output sqlAdminLogin string = sqlAdminLogin
output swaName string = swa.outputs.name
output swaHostname string = swa.outputs.defaultHostname
