param storageName string
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GZRS'
  'Standard_RAGZRS'
])
param storageSKU string = 'Standard_LRS'
param location string
param fileShareName string

resource stg 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageName
  location: location
  sku: {
    name: storageSKU
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2021-04-01' = {
  name: '${stg.name}/default/${fileShareName}'
}

output storageName string = stg.name
output storageEndpoint string = stg.properties.primaryEndpoints.blob
var blobStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${stg.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(stg.id, stg.apiVersion).keys[0].value}'
output storageConnectionString string = blobStorageConnectionString
