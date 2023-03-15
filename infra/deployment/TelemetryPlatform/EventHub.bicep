param eventHubSku string
param eventHubNamespaceName string
param eventHubDeadletterName string
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
}
