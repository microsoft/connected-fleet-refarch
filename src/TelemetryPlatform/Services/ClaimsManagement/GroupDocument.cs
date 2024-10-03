// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.Azure.ConnectedFleet.DataContracts;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

//This class represents the data stored for a GroupId, the creator (UserId), and the list of VehicleIds in the group
public class GroupDocument : ClaimsProviderDocument
{
    public const string RecordDocumentType = "Group";

    [JsonConstructor]
    public GroupDocument(string userId, string groupId, List<string> vehicles, List<StringClaim> claims)
            : base(DocumentIdFactory.CreateGroupDocumentId(userId, groupId), DocumentIdFactory.CreateGroupPartition(userId))
    {
        this.UserId = userId;
        this.GroupId = groupId;
        this.Vehicles = vehicles;
        this.Claims = claims;
    }

    /// <summary>
    ///     The principal identifier for the owner of the group list.
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    /// <summary>
    ///     The principal identifier for the Group list
    /// </summary>
    [JsonPropertyName("groupId")]
    public string GroupId { get; set; }

    /// <summary>
    ///      The claims list for this specific user's group list.
    /// </summary>
    [JsonPropertyName("vehicles")]
    public List<string> Vehicles { get; set; }

    /// <summary>
    ///      The claims list for this specific UserId/GroupId.
    /// </summary>
    [JsonPropertyName("claims")]
    public List<StringClaim> Claims { get; set; }

    public override string DocumentType => RecordDocumentType;
}