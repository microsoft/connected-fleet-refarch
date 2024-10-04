param rgLocation string = resourceGroup().location

@description('The short name of the telemetry platform workload.')
@maxLength(3)
param resWorkload string = 'tlp'

@description('The name for the custom event grid topic')
param eventGridTopicName string = 'vehicletelemetry'

var rgUniqueString = uniqueString(resourceGroup().id)

var eventGridNamespaceName =  'evgns-${resWorkload}-${rgUniqueString}'

var eventHubNamespaceName = 'evh-${resWorkload}-${rgUniqueString}'

var cosmosDbName = 'telemetrydb'

module eventgrid './EventGrid.bicep' = {
  name: 'eventgrid'
  params: {
    rgLocation: rgLocation
    eventGridNamespaceName: eventGridNamespaceName
    eventGridTopicName: eventGridTopicName
    deviceNames: [
      'device01'
      'device02'
      'device03'
      'device04'
      'device05'
    ]
  }
 }
 
 module permissions './Permissions.bicep' = {
  name: 'permissions'
  dependsOn: [
    eventgrid
  ]
  params: {
    rgLocation: rgLocation
    eventGridTopicName: eventGridTopicName
    eventGridNamespaceName: eventGridNamespaceName
 }
}

module eventhub './EventHub.bicep' = {
  name: 'eventhub'
  params: {
    eventHubNamespaceName: eventHubNamespaceName
    eventHubDeadletterName: 'deadletter'
    eventHubVehicleEventsName: 'vehicleevent'
    eventHubVehicleStatusName: 'vehiclestatus'
    eventHubSku: 'Standard'
    eventHubLocation: rgLocation
  }
 }

 module appinsights './AppInsights.bicep' = {
  name: 'appinsights'
   params: {
     opsname: 'ops-${resWorkload}-${rgUniqueString}'
     appinsname: 'appi-${resWorkload}-${rgUniqueString}'
     location: rgLocation
   }
 }

 // Create the Cosmos DB account to hold telemetry layer metadata
module cosmosdb './CosmosDb.bicep' = {
  name: 'cosmosdb' 
  params: {
    accountName: 'cosmos-${resWorkload}-${rgUniqueString}'
    containerNames: [
      'vehicledb'
      'claimsdb'
      'servicedb'
      'userdb'
    ]
    databaseName: cosmosDbName
    primaryRegion: rgLocation 
    location: rgLocation
  }
 }
 module azurefunc './AzureFunction.bicep' = {
  name: 'azurefunctions'
  dependsOn: [
    eventgrid
    cosmosdb
  ]  
  params: {
     eventGridName: eventGridNamespaceName
     eventGridTopicName: eventGridTopicName
     cosmosDbName: 'cosmos-${resWorkload}-${rgUniqueString}'
     eventHubNamespaceName: 'evh-${resWorkload}-${rgUniqueString}'
     appInsightsInstrumentationKey: appinsights.outputs.appInsightsInstrKey
     appName: 'func-${resWorkload}-${rgUniqueString}'
     appPlanName: 'asp-${resWorkload}-${rgUniqueString}'
     location: rgLocation
  }
 }

 


