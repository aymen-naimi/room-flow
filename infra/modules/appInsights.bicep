@description('Azure region')
param location string

@description('Application Insights component name')
param name string

@description('Log Analytics workspace resource ID')
param workspaceResourceId string

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceResourceId
    IngestionMode: 'LogAnalytics'
  }
}

output name string = appInsights.name
