param rgLocation string = resourceGroup().location

var rgUniqueString = uniqueString(resourceGroup().id)

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
