@description('Azure region')
param location string

@description('SQL administrator login')
param administratorLogin string

@description('SQL administrator password')
@secure()
param administratorPassword string

@description('Globally unique SQL server name (lowercase)')
param serverName string

@description('Database name')
param databaseName string = 'RoomFlow'

resource server 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: server
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: server
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

output fullyQualifiedDomainName string = server.properties.fullyQualifiedDomainName
output databaseName string = database.name
