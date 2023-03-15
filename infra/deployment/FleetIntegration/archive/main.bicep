param rgName string ='FleetAnalyticsRG'
param rgLocation string = 'eastus'

targetScope = 'subscription'

// Create root resource group to hold all other resources
resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: rgName
  location: rgLocation
}

module full './FleetIntegrationLayer.bicep' = {
  name: 'fullDeployment'
  scope: rg
  params: {
    eventHubName: 'VehicleStatusEH'
    eventHubNamespaceName: 'fleetanalyticsns'
    eventHubSku: 'Standard'
    eventHubLocation: rgLocation
    eventHubADXConsumerGroupName: 'adxvehiclestatuscg'
    eventHubAFConsumerGroupName: 'afvehiclestatuscg'
    adxName: 'FleetAnalytics'
    adxLocation: rgLocation
    adxSkuName: 'Dev(No SLA)_Standard_E2a_v4'
    adxSkuCapacity: 1
    adxSkuTier: 'Basic'
    adxIdentityType: 'SystemAssigned'
    adxVehicleStatusDBName: 'VehicleStatusDB'
    adxVehicleStatusTable: 'vehicleStatus'
  }
}

// module cosmos './CosmosDb.bicep' = {
//  name: 'FleetCosmosDB'
//   params: {
//     primaryRegion: rgLocation
//     location: rgLocation
//   }
//    scope: rg
// }

// module redis './Redis.bicep' = {
//   name: 'fleetredis'
//    params: {
//      name: 'fleetanalyticsredis'
//      location: rgLocation
//    }
//     scope: rg
//  }

//  module appinsights './AppInsights.bicep' = {
//   name: 'appinsights'
//    params: {
//      opsname: 'fleetanalyticsops'
//      appinsname: 'fleetanalyticsappins'
//      location: rgLocation
//    }
//     scope: rg
//  }

//  module azurefunc './AzureFunction.bicep' = {
//   name: 'azurefunc'
//   params: {
//      appInsightsInstrumentationKey: appinsights.outputs.instrumentationkey
//      appName: 'FleetAnalyticsFunctions'
//      location: rgLocation
//   }
//   scope: rg
//  }
