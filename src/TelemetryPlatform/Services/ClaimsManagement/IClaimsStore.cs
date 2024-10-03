// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.ConnectedFleet.DataContracts;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

public interface IClaimsStore
{
    Task<VehicleUserInfo> RetrieveVehicleUserInfoAsync(string vehicleId, string userId, string authToken);

    Task<VehicleUserInfo> RefreshClaimsInfoAsync(string vehicleId, string userId, List<KeyValuePair<string, string>> claimsList);

    Task<GroupVehicleInfo> RetrieveGroupVehicleInfoAsync(string groupId, string userId, string authToken, bool includeVehicles);

    Task<List<StringClaim>> RetrieveClaimsByTypeAsync(ClaimTypeEnum claimType, string id);

    Task AddClaimsAsync(string vehicleId, string userId, string serviceId, List<StringClaim> claims);

    Task RemoveClaimsAsync(string vehicleId, string userId, string serviceId);
}