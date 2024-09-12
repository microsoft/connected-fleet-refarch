param rgLocation string

param eventGridNamespaceName string

param eventGridName string

// Get a reference to the custom topic
resource vehicletelemetrycustomtopic 'Microsoft.EventGrid/topics@2024-06-01-preview' existing = {
  name: eventGridName
}


// Create an Event Grid Namespace with MQTT Enabled
// The Event Grid has a System assigned identity to enable routing
resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2024-06-01-preview' = {
  name: eventGridNamespaceName
  location: rgLocation
  
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
