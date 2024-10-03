using System.Text.Json.Serialization;
using Microsoft.Azure.ConnectedFleet.DataContracts;

namespace Microsoft.Azure.ConnectedVehicle.Services.VehicleManagement;

public class VehicleDocument : VehicleStoreDocument
{
    public VehicleDocument(Vehicle vehicle)
        : base(GetVehicleDocumentId(vehicle.VehicleId), vehicle.VehicleId)
    {        
        this.Vehicle = vehicle;
    }
    
    [JsonPropertyName("documentType")]
    public override string DocumentType => "vdoc";

    [JsonPropertyName("vehicle")]
    public Vehicle Vehicle { get; set; }

    internal static string GetVehicleDocumentId(string vehicleId) => $"v|{vehicleId}";
}