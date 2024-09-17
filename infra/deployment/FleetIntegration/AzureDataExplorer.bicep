//param eventHubSku string
//param eventHubNamespaceName string


//param rgTelemetryPlatform string = 'telemetryplatform'
//param evhnsTelemetryPlatformNamespaceName string

//param eventHubVehicleStatusName string
//param eventHubVehicleEventsName string
//param eventHubADXConsumerGroupName string
//param eventHubAFConsumerGroupName string

param adxClusterName string 
param adxLocation string
param adxSkuName string 
param adxSkuCapacity int  
param adxSkuTier string
param adxIdentityType string

resource adxCluster 'Microsoft.Kusto/clusters@2022-12-29' = {
  name: adxClusterName
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
}

