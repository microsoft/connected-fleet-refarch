param evhnsTelemetryPlatformNamespaceName string
param eventHubVehicleStatusName string
param eventHubADXConsumerGroupName string
param rgFleetIntegration string
param adxClusterName string


// Retrieve the event hubs from the telemetry platform
resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: evhnsTelemetryPlatformNamespaceName

  resource vehicleStatusEH 'eventhubs' existing = {
    name: eventHubVehicleStatusName
  }
}

// Get the ADX cluster created in the previous step
resource adxCluster 'Microsoft.Kusto/clusters@2022-12-29' existing = {
  name: adxClusterName
  scope: resourceGroup(rgFleetIntegration)
}

// Create a consumer group for Azure Data Explorer
resource adxconsumergroupstatus 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-05-01-preview' = {
  name: eventHubADXConsumerGroupName
  parent: eventHubNamespace::vehicleStatusEH
}

//  We need to authorize the cluster to read the event hub by assigning the role
//  "Azure Event Hubs Data Receiver"
//  Role list:  https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var dataReceiverId = 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'
var fullDataReceiverId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', dataReceiverId)
var eventHubRoleAssignmentName = '${resourceGroup().id}${adxCluster.name}${dataReceiverId}${eventHubNamespace::vehicleStatusEH.name}'
var roleAssignmentName = guid(eventHubRoleAssignmentName, eventHubVehicleStatusName, dataReceiverId, adxCluster.name)

resource clusterEventHubAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  //  See https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/scope-extension-resources
  //  for scope for extension
  scope: eventHubNamespace::vehicleStatusEH  
  properties: {
    description: 'Give "Azure Event Hubs Data Receiver" to the cluster'
    principalId: adxCluster.identity.principalId
    //  Required in case principal not ready when deploying the assignment
    principalType: 'ServicePrincipal'
    roleDefinitionId: fullDataReceiverId
  }
}

