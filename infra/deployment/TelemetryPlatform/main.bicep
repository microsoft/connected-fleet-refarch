param rgLocation string = resourceGroup().location

param resWorkload string = 'telplat'

param eventGridTopicName string = 'vehicletelemetry'

var eventGridNamespaceName =  'evgns-${resWorkload}-${rgUniqueString}'

var eventHubNamespaceName = 'evh-${resWorkload}-${rgUniqueString}'

var rgUniqueString = uniqueString(resourceGroup().id)


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
    eventHubVehicleEventsName: 'vehicleevents'
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

 module azurefunc './AzureFunction.bicep' = {
  name: 'azurefunctions'
  params: {
     eventGridTopicName: eventGridTopicName
     eventHubNamespaceName: 'evh-${resWorkload}-${rgUniqueString}'
     appInsightsInstrumentationKey: appinsights.outputs.appInsightsInstrKey
     appName: 'func-${resWorkload}-${rgUniqueString}'
     appPlanName: 'asp-${resWorkload}-${rgUniqueString}'

     location: rgLocation
  }
 }
