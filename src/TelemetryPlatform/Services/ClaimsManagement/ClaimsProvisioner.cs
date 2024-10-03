// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.ConnectedFleet.DataContracts;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

public class ClaimsProvisioner()
{
    private IClaimsStore _claimsStore = new ClaimsProviderStore();

    public async Task<VehicleUserInfo> RetrieveClaimsInfo(string authToken, string vehicleId, string userId)
    {
        return await _claimsStore.RetrieveVehicleUserInfoAsync(vehicleId, userId, authToken);
    }

    public async Task<VehicleUserInfo> RetrieveClaimsInfo(string authToken, string vehicleId)
    {
        return await _claimsStore.RetrieveVehicleUserInfoAsync(vehicleId, null, authToken);
    }

    public async Task<List<StringClaim>> RetrieveClaimsByTypeAsync(ClaimTypeEnum claimType, string id)
    {
        return await _claimsStore.RetrieveClaimsByTypeAsync(claimType, id);
    }

    public async Task AddClaimsAsync(string vehicleId, string userId, string serviceId, List<StringClaim> claims)
    {
        await _claimsStore.AddClaimsAsync(vehicleId, userId, serviceId, claims);
    }

    public async Task RemoveClaimsAsync(string vehicleId, string userId, string serviceId)
    {
        await _claimsStore.RemoveClaimsAsync(vehicleId, userId, serviceId);
    }

}