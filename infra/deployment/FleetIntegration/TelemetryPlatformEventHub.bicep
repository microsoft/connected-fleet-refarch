param evhnsTelemetryPlatformNamespaceName string
param eventHubVehicleStatusName string
param eventHubVehicleEventsName string
param eventHubADXConsumerGroupName string
param eventHubAFConsumerGroupName string
param rgFleetIntegration string

param adxClusterName string
param azFunctionsName string


// Retrieve the event hubs from the telemetry platform
resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: evhnsTelemetryPlatformNamespaceName

  resource vehicleStatusEH 'eventhubs' existing = {
    name: eventHubVehicleStatusName
  }

  resource vehicleEventsEH 'eventhubs' existing = {
    name: eventHubVehicleEventsName
  }
}

// Get the ADX cluster created in the previous step
resource adxCluster 'Microsoft.Kusto/clusters@2022-12-29' existing = {
  name: adxClusterName
  scope: resourceGroup(rgFleetIntegration)
}

// Create a consumer group for Azure Data Explorer
resource adxconsumergroup  'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-05-01-preview' = {
  name: eventHubADXConsumerGroupName
  parent: eventHubNamespace::vehicleStatusEH
}

// Get the azure functionscreated in the previous step
resource azFunc 'Microsoft.Web/sites@2023-12-01' existing = {
  name: azFunctionsName
  scope: resourceGroup(rgFleetIntegration)
}

// Create a consumer group for the Azure function that connects to Dataverse
resource adxconsumergroupstatus 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-05-01-preview' = {
  name: eventHubAFConsumerGroupName
  parent: eventHubNamespace::vehicleEventsEH
}

//  We need to authorize the cluster and the azure functions to read from the event hubs by assigning the role "Azure Event Hubs Data Receiver"
@description('This is the built-in Event Hub Data Receiver role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#analytics')
resource eventHubDataReceiverRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  scope: subscription()
  name: 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'
}

// Lets assing Event Hub Data Receiver to ADX, to read from the vehicle status hub
resource clusterEventHubAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventHubNamespace::vehicleStatusEH.id, adxCluster.id, eventHubDataReceiverRoleDefinition.id)
  scope: eventHubNamespace::vehicleStatusEH  
  properties: {
    description: 'Give "Azure Event Hubs Data Receiver" to the cluster'
    principalId: adxCluster.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: eventHubDataReceiverRoleDefinition.id
  }
}

// Lets assing Event Hub Data Receiver to Fleet Integration Azure Functions, to read from the vehicle events hub
resource azureFunctionsEventHubAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventHubNamespace::vehicleEventsEH.id, azFunc.id, eventHubDataReceiverRoleDefinition.id)
  scope: eventHubNamespace::vehicleEventsEH  
  properties: {
    description: 'Give "Azure Event Hubs Data Receiver" to the azure functions'
    principalId: azFunc.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: eventHubDataReceiverRoleDefinition.id
  }
}
