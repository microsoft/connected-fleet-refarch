// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.Azure.ConnectedFleet.DataContracts;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

//This class represents the data stored for details on a User, Vehicle, and Entity
public class ClaimDocument : ClaimsProviderDocument
{
    public const string RecordDocumentType = "Claim";

    public ClaimDocument(string vehicleId, string userId, string entityId, List<StringClaim> claims)
        : base(DocumentIdFactory.CreateClaimDocumentId(vehicleId, userId, entityId), DocumentIdFactory.CreateClaimPartition(vehicleId, userId, entityId))
    {
        this.VehicleId = vehicleId;
        this.UserId = userId;
        this.EntityId = entityId;
        this.Claims = claims;
    }

    /// <summary>
    ///     The principal identifier for the vehicle.
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; }

    /// <summary>
    ///     The principal identifier for the user.
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    /// <summary>
    ///     The principal identifier for the entity object.
    /// </summary>
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; }

    /// <summary>
    ///      The claims list for this specific UserId/VehicleId.
    /// </summary>
    [JsonPropertyName("claims")]
    public List<StringClaim> Claims { get; set; }

    [JsonPropertyName("documentType")]
    public override string DocumentType => RecordDocumentType;
}
