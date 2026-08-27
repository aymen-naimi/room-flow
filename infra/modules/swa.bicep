@description('Azure region')
param location string

@description('Static Web App name')
param name string

resource swa 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    provider: 'None'
  }
}

output name string = swa.name
output defaultHostname string = swa.properties.defaultHostname
output resourceId string = swa.id
