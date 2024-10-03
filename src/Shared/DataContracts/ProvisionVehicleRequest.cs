// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class ProvisionVehicleRequest
{
    [JsonPropertyName("vehicleUuid")]
    public string? VehicleUuid { get; set; }

    [JsonPropertyName("VehicleMetadata")]
    public Dictionary<string,string>? VehicleMetadata { get; set; }

    [JsonPropertyName("devices")]
    public IList<VehicleDevice>? Devices { get; set; }

    [JsonPropertyName("claims")]
    public List<StringClaim>? Claims { get; set; }
}
