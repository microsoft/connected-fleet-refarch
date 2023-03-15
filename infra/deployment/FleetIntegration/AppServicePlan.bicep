param appServiceName string
param location string


resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServiceName
  location: location
  properties: {
    reserved: false
    maximumElasticWorkerCount:20
  }
  sku: {
    name: 'WS1'
    tier: 'WorkflowStandard'
    size: 'WS1'
    family: 'WS'
    capacity: 1
  }
  kind: 'elastic'
}

output appServicePlanId string = appServicePlan.id
