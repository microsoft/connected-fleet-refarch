// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class VehicleDevice
{
    [JsonPropertyName("deviceId")]    
    public string? DeviceId { get; set; }

    [JsonPropertyName("deviceName")]
    public string? DeviceName { get; set; }

    [JsonPropertyName("deviceSerialNumber")]
    public string? DeviceSerialNumber { get; set; }

    [JsonPropertyName("certificateCN")]
    public string? CertificateCN { get; set; }

    [JsonPropertyName("deviceVersionInformation")]
    public Dictionary<string, Version>? DeviceVersionInformation { get; set; }
}

