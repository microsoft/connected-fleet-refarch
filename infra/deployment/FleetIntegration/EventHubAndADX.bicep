param eventHubSku string
param eventHubNamespaceName string
param eventHubVehicleStatusName string
param eventHubVehicleEventsName string
param eventHubLocation string
param eventHubADXConsumerGroupName string
param eventHubAFConsumerGroupName string

param adxName string 
param adxLocation string
param adxSkuName string 
param adxSkuCapacity int  
param adxSkuTier string
param adxIdentityType string
param adxVehicleStatusDBName string
param adxVehicleStatusTable string

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

  resource vehicleEventsEH 'eventhubs' = {
    name: eventHubVehicleEventsName
    properties: {
      messageRetentionInDays: 7
      partitionCount: 1
    }
    
    resource adxconsumergroup 'consumergroups' = {
      name: eventHubAFConsumerGroupName
    }
  }

  resource vehicleStatusEH 'eventhubs' = {
    name: eventHubVehicleStatusName
    properties: {
      messageRetentionInDays: 7
      partitionCount: 1
    }

    resource adxconsumergroup 'consumergroups' = {
      name: eventHubADXConsumerGroupName
    }
  }

}

resource adxCluster 'Microsoft.Kusto/clusters@2022-12-29' = {
  name: adxName
  location: adxLocation
  sku: {
    capacity: adxSkuCapacity
    name: adxSkuName
    tier: adxSkuTier
  }
  identity: {
    type: adxIdentityType
  }
  properties: {
    acceptedAudiences: []
    allowedFqdnList: []
    allowedIpRangeList: []
    enableAutoStop: true
    enableDiskEncryption: false
    enableDoubleEncryption: false
    enablePurge: false
    enableStreamingIngest: true
    engineType: 'V3'
    publicIPType: 'IPv4'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    trustedExternalTenants: []
  }

  resource adxVehicleStatusDB 'databases' = {
    name: adxVehicleStatusDBName
    location: adxLocation
    kind:'ReadWrite'
    properties: {
      softDeletePeriod: 'P7D'
      hotCachePeriod: 'P7D'
    }

    resource adxInitDB 'scripts' = {
      name: 'initDB'
      properties: {
        continueOnErrors: true
        scriptContent: loadTextContent('./init.kql')
      }
    }
    
    resource adxDataConnection 'dataConnections' = {
      name: 'vehicleStatusIngestion'
      location: adxLocation
      kind: 'EventHub'
      dependsOn: [ 
        adxInitDB
        clusterEventHubAuthorization
      ]
      properties: {
        eventHubResourceId: eventHubNamespace::vehicleStatusEH.id
        consumerGroup: eventHubADXConsumerGroupName
        tableName: adxVehicleStatusTable
        dataFormat: 'MULTIJSON'
        compression: 'None'
        managedIdentityResourceId: adxCluster.id
        databaseRouting: 'Single'
      }
    }
  }
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

