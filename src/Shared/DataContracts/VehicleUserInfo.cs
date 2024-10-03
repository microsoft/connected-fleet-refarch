// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class VehicleUserInfo
{
    [JsonPropertyName("claims")]
    public Dictionary<string, Collection<string>>? Claims { get; set; }

    [JsonPropertyName("expiryTime")]
    public DateTimeOffset? ExpiryTime { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; set; }
}