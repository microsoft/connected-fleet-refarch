param name string
param location string

resource symbolicname 'Microsoft.Cache/redis@2022-06-01' = {
  name: name
  location: location
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    sku: {
      capacity: 1
      family: 'C'
      name: 'Basic'
    }
  }
}
