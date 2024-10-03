// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

// This structure is returned from the claims provider when querying for a Group operation
public class GroupVehicleInfo
{
    [JsonPropertyName("Claims")]
    public Dictionary<string, Collection<string>>? Claims;

    [JsonPropertyName("ExpiryTime")]
    public DateTimeOffset? ExpiryTime;

    [JsonPropertyName("GroupId")]
    public string? GroupId;

    [JsonPropertyName("UserId")]
    public string? UserId;

    // if vehicles is null - the request was for groupId and userId only - not the vehicle list
    // if vehicles is an empty collection - then the group expansion results in no vehicles in the list
    [JsonPropertyName("Vehicles")]
    public Collection<string>? Vehicles;

    public GroupVehicleInfo()
    {
        this.Claims = new Dictionary<string, Collection<string>>(StringComparer.OrdinalIgnoreCase);
    }
}