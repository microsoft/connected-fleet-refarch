param rgLocation string = resourceGroup().location

@description('The short name of the fleet integration workload.')
@maxLength(3)
param resWorkload string = 'fti'

@description('The resource group where the telemetry platform is installed.')
param rgTelemetryPlatform string = 'telemetryplatform'

@description('The name of the telemetry platform event hub namespace. Usually evg-telplat-<uniqueid>')
param evhnsTelemetryPlatformNamespaceName string

param adxSkuName string = 'Dev(No SLA)_Standard_D11_v2'

var rgUniqueString = uniqueString(resourceGroup().id)

var azFuncName = 'func-${resWorkload}-${rgUniqueString}'
var adxClusterName = 'dec-${resWorkload}-${rgUniqueString}'

var eventHubVehicleStatusName = 'vehiclestatus'
var eventHubVehicleEventsName = 'vehicleevent'
var eventHubADXConsumerGroupName = 'adxvehiclestatuscg'
var eventHubAFConsumerGroupName = 'afvehicleeventscg'

// Create the App Insights and Ops Insights resources for monitoring
module appinsights './AppInsights.bicep' = {
  name: 'appinsightsDeploy'
   params: {
     opsname: 'ops-${resWorkload}-${rgUniqueString}'
     appinsname: 'appi-${resWorkload}-${rgUniqueString}'
     location: rgLocation
   }
 }

module azurefunc './AzureFunction.bicep' = {
  name: 'azurefunc'
  params: {
     appInsightsInstrumentationKey: appinsights.outputs.appInsightsInstrKey
     appName: azFuncName
     appPlanName: 'asp-${resWorkload}-${rgUniqueString}'
     location: rgLocation
     rgTelemetryPlatform: rgTelemetryPlatform
     evhnsTelemetryPlatformNamespaceName: evhnsTelemetryPlatformNamespaceName
     eventHubVehicleEventsName: eventHubVehicleEventsName
     eventHubAFConsumerGroupName: eventHubAFConsumerGroupName
  }
}

// Create the ADX Cluster Index
module adxcluster './AzureDataExplorer.bicep' = {
  name: 'azuredataexplorer'
  params: {
    adxClusterName: adxClusterName
    adxLocation: rgLocation
    adxSkuName: adxSkuName
    adxSkuCapacity: 1
    adxSkuTier: 'Basic'
    adxIdentityType: 'SystemAssigned'
  }
}

// Modify the event hub in the telemetry platform
// Creates the necessary consumer groups and assigns permissions to the cluster and the azure functions
module eventhub './TelemetryPlatformEventHub.bicep' = {
  name: 'telemetryplatformeventhub'
  scope: resourceGroup(rgTelemetryPlatform)
  dependsOn: [
    adxcluster, azurefunc
  ]
  params: {
    rgFleetIntegration: resourceGroup().name
    eventHubVehicleStatusName: eventHubVehicleStatusName
    eventHubVehicleEventsName: eventHubVehicleEventsName
    eventHubADXConsumerGroupName: eventHubADXConsumerGroupName
    eventHubAFConsumerGroupName: eventHubAFConsumerGroupName
    evhnsTelemetryPlatformNamespaceName: evhnsTelemetryPlatformNamespaceName
    azFunctionsName: azFuncName
    adxClusterName: adxClusterName
  }
}

module adxDatabase './AzureDataExplorerDatabase.bicep' = {
    name: 'adxdatabase'
    dependsOn: [
      eventhub
    ]
    params: {
      adxClusterName: adxClusterName
      adxVehicleStatusDBName: 'VehicleStatusDB'
      adxVehicleStatusTable: 'RawVehicleStatus'
      eventHubADXConsumerGroupName: eventHubADXConsumerGroupName
      eventHubVehicleStatusName: eventHubVehicleStatusName
      evhnsTelemetryPlatformNamespaceName: evhnsTelemetryPlatformNamespaceName
      rgTelemetryPlatform: rgTelemetryPlatform
      rgLocation: rgLocation
    }

}
