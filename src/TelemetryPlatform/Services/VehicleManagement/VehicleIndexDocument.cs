using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedVehicle.Services.VehicleManagement;

public class VehicleIndexDocument : VehicleStoreDocument
{
    public VehicleIndexDocument(string vehicleId, string vehicleUuid)
        : base(GetVehicleDocumentId(vehicleUuid), vehicleUuid)
    {        
        this.VehicleId = vehicleId;
        this.VehicleUuid = vehicleUuid;
    }
    
    [JsonPropertyName("documentType")]
    public override string DocumentType => "idoc";

    [JsonPropertyName("vehicleUuid")]
    public string VehicleUuid { get; set; }

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; }

    [JsonPropertyName("_etag")]
    public string Etag { get; set; }

    internal static string GetVehicleDocumentId(string vehicleUuid) => $"i|{vehicleUuid}";
}
