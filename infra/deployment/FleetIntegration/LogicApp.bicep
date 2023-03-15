param appServicePlanId string
param logicAppName string
param location string
param appInsightsInstrKey string
param appInsightsEndpoint string
param storageConnectionString string

var logicAppEnabledState = true

resource logicApp 'Microsoft.Web/sites@2021-02-01' = {
  name: logicAppName
  location: location
  kind: 'functionapp,workflowapp'
  identity: {
      type: 'SystemAssigned'
  }
  properties: {
    enabled: logicAppEnabledState
    serverFarmId: appServicePlanId
    reserved: false
    hostNameSslStates: [
      {
          name: '${logicAppName}.azurewebsites.net'
          sslState: 'Disabled'
          hostType: 'Standard'
      }
      {
          name: '${logicAppName}.scm.azurewebsites.net'
          sslState: 'Disabled'
          hostType: 'Repository'
      }
    ]
    siteConfig: {
      alwaysOn: false
      appSettings: [
        {
          name: 'APP_KIND'
          value: 'workflowApp'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsEndpoint
        }
        {
          name: 'AzureFunctionsJobHost__extensionBundle__id'
          value: 'Microsoft.Azure.Functions.ExtensionBundle.Workflows'
        }
        {
          name: 'AzureFunctionsJobHost__extensionBundle__version'
          value: '[1.*, 2.0.0)'
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageConnectionString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~14'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'node'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: logicAppName
        }
      ]
    }
  }
}

output LogicAppName string = logicApp.name
