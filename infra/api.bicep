@description('Azure region')
param location string = 'northeurope'

@description('Name prefix for the Container App')
param prefix string = 'roomflow'

@description('Container Apps environment resource ID')
param environmentId string

@description('ACR login server')
param acrLoginServer string

@description('User-assigned identity resource ID with AcrPull')
param identityId string

@description('Full image reference including tag')
param apiImage string

@description('JWT signing key (>= 32 bytes)')
@secure()
param jwtSigningKey string

@description('Azure SQL fully qualified domain name')
param sqlFqdn string

@description('Azure SQL database name')
param sqlDatabaseName string

@description('SQL administrator login')
param sqlAdminLogin string

@description('SQL administrator password')
@secure()
param sqlAdminPassword string

@description('Allowed CORS origin (Static Web App HTTPS origin)')
param corsOrigin string

var unique = uniqueString(resourceGroup().id)
var containerAppName = take('${prefix}-api-${unique}', 32)
var sqlConnectionString = 'Server=tcp:${sqlFqdn},1433;Initial Catalog=${sqlDatabaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;Connection Timeout=30;'

module containerApp 'modules/containerApp.bicep' = {
  name: 'container-app'
  params: {
    location: location
    name: containerAppName
    environmentId: environmentId
    acrLoginServer: acrLoginServer
    identityId: identityId
    apiImage: apiImage
    jwtSigningKey: jwtSigningKey
    sqlConnectionString: sqlConnectionString
    corsOrigin: corsOrigin
  }
}

output apiFqdn string = containerApp.outputs.fqdn
output containerAppName string = containerApp.outputs.name
