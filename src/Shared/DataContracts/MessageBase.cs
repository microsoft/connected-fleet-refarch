// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public abstract class MessageBase
{
    public MessageBase(string vehicleId, DateTime timestamp, Version schemaVersion, GeoLocation? geoLocation)
    {
        this.VehicleId = vehicleId;
        this.Timestamp = timestamp;
        this.SchemaVersion = schemaVersion;
        this.GeoLocation = geoLocation;
    }

    [JsonProperty("vehicleId")]
    public string? VehicleId { get; set; } 

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("schemaVersion")]
    public Version? SchemaVersion {get; set; }

    [JsonProperty("geoLocation")]
    public GeoLocation? GeoLocation { get; set; }
}