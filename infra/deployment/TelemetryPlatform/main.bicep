param rgLocation string = resourceGroup().location

param eventGridName string = 'vehicletelemetry'

var rgUniqueString = uniqueString(resourceGroup().id)

module eventgrid './EventGrid.bicep' = {
  name: 'eventgrid'
  params: {
    rgLocation: rgLocation
    eventGridName: eventGridName
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
    eventGridName: eventGridName
    eventGridNamespaceName: eventgrid.outputs.eventGridNamespaceName
 }
}

module eventhub './EventHub.bicep' = {
  name: 'eventhub'
  params: {
    eventHubNamespaceName: 'eh-${rgUniqueString}'
    eventHubDeadletterName: 'deadletter'
    eventHubSku: 'Standard'
    eventHubLocation: rgLocation
  }
 }

 module appinsights './AppInsights.bicep' = {
  name: 'appinsights'
   params: {
     opsname: 'ops-${rgUniqueString}'
     appinsname: 'appins-${rgUniqueString}'
     location: rgLocation
   }
 }

 module azurefunc './AzureFunction.bicep' = {
  name: 'azurefunc'
  params: {
     appInsightsInstrumentationKey: appinsights.outputs.appInsightsInstrKey
     appName: 'functions-${rgUniqueString}'
     appPlanName: 'appplan-${rgUniqueString}'
     location: rgLocation
  }
 }
