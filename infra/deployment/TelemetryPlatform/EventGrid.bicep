param rgLocation string = resourceGroup().location

// The name that will be assigned all resources related to the event grid
param eventGridName string = 'vehicletelemetry'

// Array that contains the devices that will be registered to the event hub
param deviceNames array = [
  'device01'
  'device02'
  'device03'
  'device04'
  'device05'
]

// A unique string based on the resource group name
var rgUniqueString = uniqueString(resourceGroup().id)

// The Intermediate Test CA Certificate, used to validate the devices connecting to the Event Grid Namespace
var caCert = trim(loadTextContent('../TelemetryPlatform/cert-gen/certs/azure-mqtt-test-only.intermediate.cert.pem'))

// To process the MQTT telemetry messages, we will route all messages to a custom topic
// See https://learn.microsoft.com/en-us/azure/event-grid/mqtt-routing-to-azure-functions-portal as a reference

// We will create two resources a custom topic tor eceive the messages and an event grid namespace with the MQTT broker functionalily enabled.

// Creation of a custom topic
resource vehicletelemetrycustomtopic 'Microsoft.EventGrid/topics@2024-06-01-preview' = {
  name: eventGridName
  location: rgLocation  
  properties: {
    publicNetworkAccess: 'Enabled'
    inputSchema: 'CloudEventSchemaV1_0'
  }
}

@description('The name of the Event Grid custom topic. ')
output vehicleTelemetryCustomTopicName string = vehicletelemetrycustomtopic.name

// Create an Event Grid Namespace with MQTT Enabled
// The Event Grid has a System assigned identity to enable routing
resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2024-06-01-preview' = {
  name: '${eventGridName}-${rgUniqueString}'
  location: rgLocation
  tags: {
    environment: 'dev'
  }
  properties: {
    publicNetworkAccess: 'Enabled'    
    topicSpacesConfiguration: {
      state: 'Enabled'            
      /**
      routeTopicResourceId: vehicleteleemetrycustomtopic.id
      routingIdentityInfo: {
        type: 'SystemAssigned'
      }
      */
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

// Enable managed identity for the namespace
// https://learn.microsoft.com/en-us/azure/event-grid/mqtt-routing-to-azure-functions-portal#enable-managed-identity-for-the-namespace

// Create a reference to the Event Grid Data sender role definition
@description('This is the built-in Event Grid Data Sender role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles')
resource eventGridDataSenderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  scope: subscription()
  name: 'd5a91429-5739-47e2-a06b-3470a27159e7'
}

// Assign the data sender role to event grid to allow it to send events to the topic
resource vehicleteleemetrycustomtopicroleassignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().id, vehicletelemetrycustomtopic.id, eventGridDataSenderRoleDefinition.id) // Name must be deterministic
  scope: vehicletelemetrycustomtopic
  properties: {
    roleDefinitionId: eventGridDataSenderRoleDefinition.id
    principalId: eventGridNamespace.identity.principalId
    principalType: 'ServicePrincipal'
  }
}


output eventGridNamespaceName string = eventGridNamespace.name

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
// This topic space enables vehicle devices to publish vehicle status and vehicle events, using the device name as the topic.
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
// This permission binding allows vehicles with the type attribute set to vehicle to publish to the telemetry topic space
resource PB_pub_allvehicles 'Microsoft.EventGrid/namespaces/permissionBindings@2024-06-01-preview' = {
  name: 'pb-allvehicles-telemetrypub'
  parent: eventGridNamespace
  dependsOn: [
    CG_allvehicles
    TS_TelemetryPub
  ]
  properties: {
    clientGroupName: 'allvehicles'
    description: 'Allows all vehicles to publish to all topics'
    permission: 'Publisher'
    topicSpaceName: 'telemetrypub'
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


