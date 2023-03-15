param opsname string
param appinsname string
param location string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: opsname
  location: location
  identity: {
    type: 'None'
  }
  properties: {
    sku: {
      name: 'pergb2018'
    }
  }
}

resource appinsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appinsname
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    RetentionInDays: 30
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

output appInsightsName string = appinsights.name
output appInsightsId string = appinsights.id
output appInsightsInstrKey string = appinsights.properties.InstrumentationKey
output appInsightsEndpoint string = appinsights.properties.ConnectionString
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
