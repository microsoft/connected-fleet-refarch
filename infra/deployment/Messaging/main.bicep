// Location of the resource group, will be used for all resources going forward
param rgLocation string = resourceGroup().location

// Array that contains all of the devices
param deviceNames array = [
  'device01'
  'device02'
  'device03'
  'device04'
  'device05'
]

var rgUniqueString = uniqueString(resourceGroup().id)

// The Test CA Certificate, used to validate the devices connecting to the Event Grid Namespace
var caCert = trim(loadTextContent('../TelemetryPlatform/cert-gen/certs/azure-mqtt-test-only.intermediate.cert.pem'))

// Create an Event Grid Namespace with MQTT Enabled
resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2024-06-01-preview' = {
  name: 'vehicletelemetry-${rgUniqueString}'
  location: rgLocation
  tags: {
    environment: 'dev'
  }
  properties: {
    publicNetworkAccess: 'Enabled'    
    topicSpacesConfiguration: {
      state: 'Enabled'            
      clientAuthentication: {
        alternativeAuthenticationNameSources: [
          'ClientCertificateSubject'
        ]
      }
            
    }
    topicsConfiguration: {
      
    }  
  }
}

// Add the TestCA Certificate to the Event Grid Namespace
resource eventGridNamespaceCACertificate 'Microsoft.EventGrid/namespaces/caCertificates@2024-06-01-preview' = {
  name: 'TestCA'
  parent: eventGridNamespace
  properties: {    
    description: 'Generated, Self-Signed Test CA Certificate - Not for Production Use'
    encodedCertificate: caCert
  }
}

// Create a client group for all of the vehicles
resource CG_allvehicles 'Microsoft.EventGrid/namespaces/clientGroups@2024-06-01-preview' = {
  name: 'allvehicles'
  parent: eventGridNamespace
  properties: {
    description: 'Group for all of the vehicles'
    query: 'attributes.type = "vehicle"'
  }
}

// Create a topic space for publishing telemetry
resource TS_TelemetryPub 'Microsoft.EventGrid/namespaces/topicSpaces@2024-06-01-preview' = {
  name: 'telemetrypub'
  parent: eventGridNamespace
  properties: {
    description: 'Vehicles can publish status and events over this topic space'
    topicTemplates: [
      '\${client.authenticationName}/vehiclestatus'
      '\${client.authenticationName}/vehicleevent'
    ]
  }
}

// Create a permission binding for the allvehicles group to publish to the telemetry topic space
resource PB_pub_allvehicles 'Microsoft.EventGrid/namespaces/permissionBindings@2024-06-01-preview' = {
  name: 'pb-allvehicles-telemetrypub'
  parent: eventGridNamespace
  properties: {
    clientGroupName: 'allvehicles'
    description: 'Allows all vehicles to publish to all topics'
    permission: 'Publisher'
    topicSpaceName: 'telemetrypub'
  }
}

// Create an Event Grid Namespace Subscription for the telemetry topic, this allows routing to other Azure services
resource eventGridNamespaceTopic 'Microsoft.EventGrid/namespaces/topics@2024-06-01-preview' = {
  name: 'telemetryingestion'
  parent: eventGridNamespace
  properties: {    
    eventRetentionInDays: 7
    inputSchema: 'CloudEventSchemaV1_0'
    publisherType: 'Custom'
  }
}


// Create all the client devices
resource devices 'Microsoft.EventGrid/namespaces/clients@2024-06-01-preview' = [for devicename in deviceNames: { 
  name: devicename
  parent: eventGridNamespace
  properties: {
    attributes: {
      type: 'vehicle'
    }
    authenticationName: '${devicename}.mqtt.contoso.com'
    clientCertificateAuthentication: {
      validationScheme: 'SubjectMatchesAuthenticationName'
    }
    description: '${devicename} test'
    state: 'Enabled'
  }
}]





