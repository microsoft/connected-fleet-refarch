param rgLocation string

param eventGridNamespaceName string

param eventGridName string

// Creation of a custom topic
resource vehicletelemetrycustomtopic 'Microsoft.EventGrid/topics@2024-06-01-preview' = {
  name: eventGridName
  location: rgLocation
  properties: {
    publicNetworkAccess: 'Enabled'
    inputSchema: 'CloudEventSchemaV1_0'
  }
}


// Create an Event Grid Namespace with MQTT Enabled
// The Event Grid has a System assigned identity to enable routing
resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2024-06-01-preview' = {
  name: eventGridNamespaceName
  location: rgLocation
  tags: {
    environment: 'dev'
  }
  properties: {
    publicNetworkAccess: 'Enabled'    
    topicSpacesConfiguration: {
      state: 'Enabled'            
      routeTopicResourceId: vehicletelemetrycustomtopic.id
      routingIdentityInfo: {
        type: 'SystemAssigned'
      }
      clientAuthentication: {
        alternativeAuthenticationNameSources: [
          'ClientCertificateSubject'
        ]
      }
    }
  }  
  identity: {
    type: 'SystemAssigned'
  }
}
