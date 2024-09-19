@description('The name of the function app that you wish to create.')
param appName string 

@description('The name of the App Service Plan that you wish to create.')
param appPlanName string 

param evhnsTelemetryPlatformNamespaceName string
param rgTelemetryPlatform string

@description('Storage Account type')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param storageAccountType string = 'Standard_LRS'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The instrumentation key for application insights logging')
param appInsightsInstrumentationKey string

param eventHubAFConsumerGroupName string
param eventHubVehicleEventsName string

var functionAppName = appName
var hostingPlanName = appPlanName
var storageAccountName = 'stg${uniqueString(resourceGroup().id)}'



// Get the Telemetry Platform event hub
resource evhnsTelemetryPlatformNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: evhnsTelemetryPlatformNamespaceName
  scope: resourceGroup(rgTelemetryPlatform)
}


resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'Storage'
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }  
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_INPROC_NET8_ENABLED'
          value: '1'
        } 
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~14'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'EventHubConnection__fullyQualifiedNamespace'
          value: '${evhnsTelemetryPlatformNamespace.name}.servicebus.windows.net'
        }        
        {
          name: 'EventHubConnection__credential'
          value: 'managedidentity'
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }

  // Create the dummy event processing function
  resource vehicleeventfunction 'functions' = {
    name: 'VehicleEventHandler'
    properties: {
      config: {
        disabled: false      
        bindings: [
          {
            name: 'vehicleEvents'
            type: 'eventHubTrigger'
            direction: 'in'
            eventHubName: eventHubVehicleEventsName
            connection: 'EventHubConnection'       
            consumerGroup: eventHubAFConsumerGroupName
            datatype: 'string'
          }
        ]
      }
      files: {
        'run.csx': loadTextContent('vehicleeventfunction-dummy.csx')
      }    
    }
  }
}
