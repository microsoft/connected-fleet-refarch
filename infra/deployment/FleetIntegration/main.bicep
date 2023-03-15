param rgLocation string = resourceGroup().location

var rgUniqueString = uniqueString(resourceGroup().id)

// Create App Service Plan that will hold the Logic App
// module appplan './AppServicePlan.bicep' = {
//   name: 'AppServicePlanDeploy'
//   params: {
//     appServiceName: 'asp-${rgUniqueString}'
//     location: rgLocation
//   }
// }

// Create the App Insights and Ops Insights resources for monitoring
module appinsights './AppInsights.bicep' = {
  name: 'appinsightsDeploy'
   params: {
     opsname: 'ops-${rgUniqueString}'
     appinsname: 'appins-${rgUniqueString}'
     location: rgLocation
   }
 }
 
// Create a Storage Account
// module stgModule './Storage.bicep' = {
//   name: 'storageDeploy'
//   params: {
//     storageName: 'stg${rgUniqueString}'
//     location: rgLocation
//     fileShareName: 'la-${rgUniqueString}'
//   }
// }

// Create the logic app host
// module logicapp './LogicApp.bicep' = {
//   name: 'logicappDeploy'
//   params: {
//     appServicePlanId: appplan.outputs.appServicePlanId
//     logicAppName: 'logapp-${rgUniqueString}'
//     location: rgLocation
//     appInsightsEndpoint: appinsights.outputs.appInsightsEndpoint
//     appInsightsInstrKey: appinsights.outputs.appInsightsInstrKey
//     storageConnectionString: stgModule.outputs.storageConnectionString
//   }
//   dependsOn: [
//     stgModule
//     appplan
//     appinsights
//   ]
// }

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
