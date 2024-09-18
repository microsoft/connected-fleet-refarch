param adxClusterName string 
param evhnsTelemetryPlatformNamespaceName string
param eventHubVehicleStatusName string
param eventHubADXConsumerGroupName string
param adxVehicleStatusDBName string
param adxVehicleStatusTable string
param rgTelemetryPlatform string
param rgLocation string


// Retrieve the event hub namespace, event hub and consumer group from the telemetry platform
resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: evhnsTelemetryPlatformNamespaceName
  scope: resourceGroup(rgTelemetryPlatform)

  resource vehicleStatusEH 'eventhubs' existing = {
    name: eventHubVehicleStatusName
  }
}

// Get the ADX cluster created in the previous step and add the data connection
resource adxCluster 'Microsoft.Kusto/clusters@2022-12-29' existing = {
  name: adxClusterName

  resource adxVehicleStatusDB 'databases' = {
    name: adxVehicleStatusDBName
    location: rgLocation
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
      location: rgLocation
      kind: 'EventHub'
      dependsOn: [ 
        adxInitDB
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


