
param eventGridName string

@description('The name of the function app that you wish to create.')
param appName string 

@description('The name of the App Service Plan that you wish to create.')
param appPlanName string 

@description('Storage Account type')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param storageAccountType string = 'Standard_LRS'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The language worker runtime to load in the function app.')
@allowed([
  'node'
  'dotnet'
  'java'
])
param runtime string = 'dotnet'

@description('The instrumentation key for application insights logging')
param appInsightsInstrumentationKey string

var functionAppName = appName
var hostingPlanName = appPlanName
var storageAccountName = 'stg${uniqueString(resourceGroup().id)}'
var functionWorkerRuntime = runtime

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

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
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
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
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
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionWorkerRuntime
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}


resource vehicleventhandler 'Microsoft.Web/sites/functions@2023-12-01' = {
  parent: functionApp
  name: 'VehicleEventHandler'    
  properties: {
    config: {
      disabled: false      
      bindings: [
        {
          name: 'eventGridEvent'
          type: 'EventGridTrigger'
          direction: 'in'
          authLevel: 'function'
        }
      ]
    }
    files: {
      'run.csx': loadTextContent('run.csx')
    }
  }
}

// Create the relevant functions
resource vehiclestatushandler 'Microsoft.Web/sites/functions@2023-12-01' = {
  parent: functionApp
  name: 'VehicleStatusHandler'  
  properties: {
    config: {
      disabled: false
      bindings: [
        {
          name: 'eventGridEvent'
          type: 'EventGridTrigger'
          direction: 'in'
          authLevel: 'function'
        }
      ]
    }
    files: {
      'run.csx': loadTextContent('run.csx')
    }
  }  
}

// Get a reference to the custom topic
resource vehicletelemetrycustomtopic 'Microsoft.EventGrid/topics@2024-06-01-preview' existing = {
  name: eventGridName
}



// Subscribe to the vehicle status topic
resource vehicleStatusTopicSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2024-06-01-preview' = {    
  name: 'vehiclestatus'
  parent: vehicletelemetrycustomtopic
  properties: {
      eventDeliverySchema: 'CloudEventSchemaV1_0'
      destination: {
        endpointType: 'AzureFunction'
        properties:{
          resourceId: vehiclestatushandler.id
        }
      }
      filter: {
        subjectEndsWith: 'vehiclestatus'
      }
  }
}

// Subscribe to the vehicle status topic
resource vehicleEventTopicSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2024-06-01-preview' = {    
  name: 'vehicleevent'
  parent: vehicletelemetrycustomtopic
  properties: {
      eventDeliverySchema: 'CloudEventSchemaV1_0'
      destination: {
        endpointType: 'AzureFunction'
        properties:{
          resourceId: vehiclestatushandler.id
        }
      }
      filter: {
        subjectEndsWith: 'vehicleevent'
      }
  }
}
