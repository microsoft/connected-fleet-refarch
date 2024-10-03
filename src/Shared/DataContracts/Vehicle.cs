// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class Vehicle
{
    public Vehicle(string vehicleId, IList<VehicleDevice> devices)
    {
        this.VehicleId = vehicleId;
        this.Devices = devices;
    }

    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; set; }

    [JsonPropertyName("vehicleUuid")]
    public string? VehicleUuid { get; set; }

    [JsonPropertyName("vehicleMetadata")]
    public Dictionary<string,string>? VehicleMetadata { get; set; }

    [JsonPropertyName("devices")]
    public IList<VehicleDevice>? Devices { get; }

    [JsonPropertyName("claims")]
    public List<StringClaim>? Claims { get; set; }
}
