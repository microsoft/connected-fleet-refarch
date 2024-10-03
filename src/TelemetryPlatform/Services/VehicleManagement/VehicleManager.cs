using System.Diagnostics;
using System.Text.Json;
using Microsoft.Azure.ConnectedFleet.CosmosDb;
using Microsoft.Azure.ConnectedFleet.DataContracts;
using Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.EventGrid;
using Azure.ResourceManager.EventGrid.Models;

namespace Microsoft.Azure.ConnectedVehicle.Services.VehicleManagement;

public class VehicleManager
{
    private CosmosClient _cosmosClient;
    private Database _database;
    private Container _container;

    public VehicleManager()
    {
        _cosmosClient = CosmosDbClientFactory.CreateClient(Environment.GetEnvironmentVariable("CosmosDbConnectionString"));
        _database = _cosmosClient.GetDatabase(Environment.GetEnvironmentVariable("VehicleDatabaseId"));
        _container = _database.GetContainer(Environment.GetEnvironmentVariable("VehicleContainerId"));
    }

    public async Task<Vehicle> ProvisionVehicleAsync(ProvisionVehicleRequest request)
    {       
        // Step 1 - Create the vehicle in the database
        Vehicle vehicle = await CreateVehicleInDB(request);

        // Step 2 - Create the vehicle in the broker
        CreateVehicleInBroker(vehicle);

        // Step 3 - Create vehicle claims
        await CreateVehicleClaims(vehicle.VehicleId, request.Claims);

        // Step 6 - Put the claims into vehicle object
        vehicle.Claims = request.Claims;

        return vehicle;
    }

    public async Task<Vehicle> GetVehicleAsync(string vehicleId)
    {
        // Step 1 - Retrieve the vehicle document
        var vehicleDocumentId = VehicleDocument.GetVehicleDocumentId(vehicleId);
        var response = await _container.ReadItemAsync<VehicleDocument>(vehicleDocumentId, new PartitionKey(vehicleId));

        // Step 2 - Retrieve the claims for the vehicle
        ClaimsProvisioner provisioner = new ClaimsProvisioner();
        List<StringClaim> claims = await provisioner.RetrieveClaimsByTypeAsync(ClaimTypeEnum.Vehicle, vehicleId);

        // Step 3 - Embed the claims in the vehicle object
        response.Resource.Vehicle.Claims = claims;

        return response.Resource.Vehicle;
    }

    public async Task<Vehicle> GetVehicleByUuidAsync(string vehicleUuuid)
    {        
        var vehicleIndexDocumentId = VehicleIndexDocument.GetVehicleDocumentId(vehicleUuuid);
        var vehicleIndexResponse = await _container.ReadItemAsync<VehicleIndexDocument>(vehicleIndexDocumentId, new PartitionKey(vehicleUuuid));

        string vehicleId = vehicleIndexResponse.Resource.VehicleId;
        return await GetVehicleAsync(vehicleId);
    }

    public async Task<Vehicle> RollVehicleIdAsync(string vehicleUuid)
    {
        // Get the current vehicle index document to reverse lookup the current vehicleId
        var vehicleIndexDocumentId = VehicleIndexDocument.GetVehicleDocumentId(vehicleUuid);
        var vehicleIndexResponse = await _container.ReadItemAsync<VehicleIndexDocument>(vehicleIndexDocumentId, new PartitionKey(vehicleUuid));

        // Get the current vehicle document
        string vehicleId = vehicleIndexResponse.Resource.VehicleId;
        var vehicleDocumentId = VehicleDocument.GetVehicleDocumentId(vehicleId);
        var vehicleDocumentResponse = await _container.ReadItemAsync<VehicleDocument>(vehicleDocumentId, new PartitionKey(vehicleId));

        // Get the claims for the current vehicle
        ClaimsProvisioner provisioner = new ClaimsProvisioner();
        List<StringClaim> claims = await provisioner.RetrieveClaimsByTypeAsync(ClaimTypeEnum.Vehicle, vehicleId);   
        
        // Create a new vehicle object with a new vehicleId
        Vehicle vehicle = vehicleDocumentResponse.Resource.Vehicle;
        vehicle.VehicleId = Guid.NewGuid().ToString("N");

        // Save the new vehicle document
        VehicleDocument vehicleDocument = new VehicleDocument(vehicle);
        var response = await _container.CreateItemAsync<VehicleDocument>(vehicleDocument, new PartitionKey(vehicleDocument.PartitionKey));

        // Update the index document for reverse lookup
        VehicleIndexDocument indexDocument = new VehicleIndexDocument(vehicle.VehicleId, vehicle.VehicleUuid);
        var indexResponse = await _container.UpsertItemAsync<VehicleIndexDocument>(indexDocument, new PartitionKey(indexDocument.PartitionKey));

        // Add the vehicle claims for the new vehicle 
        await CreateVehicleClaims(vehicle.VehicleId, claims);

        // Put the claims into vehicle object
        vehicle.Claims = claims;

        return vehicle;
    }

    private async Task<Vehicle> CreateVehicleInDB(ProvisionVehicleRequest request)
    {
        // Create the new vehicle object from the input request
        Vehicle vehicle = new Vehicle(Guid.NewGuid().ToString("N"), request.Devices);
        vehicle.VehicleUuid = request.VehicleUuid;
        vehicle.VehicleMetadata = request.VehicleMetadata;
        vehicle.Claims = null;

        // Embed the vehicle object into a document
        VehicleDocument vehicleDocument = new VehicleDocument(vehicle);

        // Save the document to Cosmos DB
        var response = await _container.CreateItemAsync<VehicleDocument>(vehicleDocument, new PartitionKey(vehicleDocument.PartitionKey));

        // Save the index document for reverse lookup
        VehicleIndexDocument indexDocument = new VehicleIndexDocument(vehicle.VehicleId, vehicle.VehicleUuid);
        var indexResponse = await _container.UpsertItemAsync<VehicleIndexDocument>(indexDocument, new PartitionKey(indexDocument.PartitionKey));

        // Return the response
        return vehicle;
    }

    private void CreateVehicleInBroker(Vehicle vehicle)
    {
        ClientSecretCredential credential = new ClientSecretCredential(
            Environment.GetEnvironmentVariable("TenantId"),
            Environment.GetEnvironmentVariable("ClientId"),
            Environment.GetEnvironmentVariable("ClientSecret"));

        ArmClient client = new ArmClient(credential);

        string egResourceId = Environment.GetEnvironmentVariable("EventGridResourceId");
        
        ResourceIdentifier egResource = new ResourceIdentifier(egResourceId);
        EventGridNamespaceResource eventGridNamespace = client.GetEventGridNamespaceResource(egResource);
        EventGridNamespaceClientCollection clients = eventGridNamespace.GetEventGridNamespaceClients();
        
        foreach(var device in vehicle.Devices)
        {
            EventGridNamespaceClientData clientData = new EventGridNamespaceClientData()
            {
                AuthenticationName = device.CertificateCN,
                State = "Enabled",
                Description = "",
                ClientCertificateAuthentication = new ClientCertificateAuthentication()
                {
                    ValidationScheme = Environment.GetEnvironmentVariable("ValidationScheme")
                }
            };

            //var attributes = new DeviceAttributes() { type = "vehicle" };
            clientData.Attributes.Add("type", BinaryData.FromString("\"vehicle\""));            

            ArmOperation<EventGridNamespaceClientResource> mqttClient = clients.CreateOrUpdate(0, device.CertificateCN, clientData);
        }
    }

    private async Task CreateVehicleClaims(string vehicleId, List<StringClaim> claims)
    {
        // Call the claims provisioner to add the claims
        ClaimsProvisioner provisioner = new ClaimsProvisioner();
        await provisioner.AddClaimsAsync(vehicleId, null, null, claims);
    }    
}
