param rgLocation string = resourceGroup().location

var rgUniqueString = uniqueString(resourceGroup().id)


// Create the App Insights and Ops Insights resources for monitoring
module appinsights './AppInsights.bicep' = {
  name: 'appinsightsDeploy'
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

// Create the Event Hub and ADX intstance and wire them together
module full './EventHubAndADX.bicep' = {
  name: 'EventHubAndADXDeploy'
  params: {
    eventHubVehicleStatusName: 'vehiclestatus'
    eventHubVehicleEventsName: 'vehicleevent'
    eventHubNamespaceName: 'eh-${rgUniqueString}'
    eventHubSku: 'Standard'
    eventHubLocation: rgLocation
    eventHubADXConsumerGroupName: 'adxvehiclestatuscg'
    eventHubAFConsumerGroupName: 'afvehicleeventscg'
    adxName: 'adx${rgUniqueString}'
    adxLocation: rgLocation
    adxSkuName: 'Dev(No SLA)_Standard_E2a_v4'
    adxSkuCapacity: 1
    adxSkuTier: 'Basic'
    adxIdentityType: 'SystemAssigned'
    adxVehicleStatusDBName: 'VehicleStatusDB'
    adxVehicleStatusTable: 'RawVehicleStatus'
  }
}
