param eventHubSku string
param eventHubNamespaceName string
param eventHubDeadletterName string = 'deadletter'
param eventHubVehicleStatusName string = 'vehiclestatus'
param eventHubVehicleEventsName string = 'vehicleevent'
param eventHubLocation string = resourceGroup().location

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: eventHubNamespaceName
  location: eventHubLocation
  sku: {
    name: eventHubSku
    tier: eventHubSku
    capacity: 1
  }
  properties: {
    isAutoInflateEnabled: false
    maximumThroughputUnits: 0
  }

  resource deadletterEH 'eventhubs' = {
    name: eventHubDeadletterName
    properties: {
      messageRetentionInDays: 7
      partitionCount: 1
    }
  }

  resource vehicleStatusEH 'eventhubs' = {
    name: eventHubVehicleStatusName
    properties: {
      messageRetentionInDays: 7
      partitionCount: 1
    }
  }

  resource vehicleEventsEH 'eventhubs' = {
    name: eventHubVehicleEventsName
    properties: {
      messageRetentionInDays: 7
      partitionCount: 1
    }
  }
}
