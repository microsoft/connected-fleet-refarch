
param eventGridTopicName string

param eventHubNamespaceName string

@description('The name of the function app that you wish to create.')
param appName string 

@description('The name of the App Service Plan that you wish to create.')
param appPlanName string 

@description('Storage Account type')
param storageAccountType string = 'Standard_LRS'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The instrumentation key for application insights logging')
param appInsightsInstrumentationKey string

var storageAccountName = 'stg${uniqueString(resourceGroup().id)}'

// Get the event hub where we will send the data
resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: eventHubNamespaceName
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
  name: appPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: appName
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
          value: toLower(appName)
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
          value: '${eventHubNamespace.name}.servicebus.windows.net'
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
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
      'run.csx': loadTextContent('vehiclestatus-dummy.csx')
    }
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
      'run.csx': loadTextContent('vehicleevents-dummy.csx')
    }
  }
}

// Get a reference to the custom topic
resource vehicletelemetrycustomtopic 'Microsoft.EventGrid/topics@2024-06-01-preview' existing = {
  name: eventGridTopicName
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
          resourceId: vehicleventhandler.id
        }
      }
      filter: {
        subjectEndsWith: 'vehicleevent'
      }
  }
}

@description('This is the built-in Event Hub Data Sender role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#analytics')
resource eventHubDataSenderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  scope: subscription()
  name: '2b629674-e913-4c01-ae53-ef4638d8f975'
}

resource azurefunctionroleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().id, vehicletelemetrycustomtopic.id, eventHubDataSenderRoleDefinition.id) // Name must be deterministic
  scope: eventHubNamespace
  properties: {
    roleDefinitionId: eventHubDataSenderRoleDefinition.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
