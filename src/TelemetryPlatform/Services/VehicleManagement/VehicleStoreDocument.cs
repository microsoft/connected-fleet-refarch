using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedVehicle.Services.VehicleManagement;

public abstract class VehicleStoreDocument
{
    protected VehicleStoreDocument(string id, string partitionKey)
    {
        this.Id = id;
        this.PartitionKey = partitionKey;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; }

    [JsonPropertyName("documentType")]
    public abstract string DocumentType { get; }
}