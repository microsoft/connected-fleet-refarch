// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Validation;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

public class DocumentIdFactory
{
    public static string CreateClaimDocumentId(string vehicleId, string userId, string entityId)
    {
        return $"Claim|{vehicleId}|{userId}|{entityId}";
    }

    public static string CreateEntityClaimDocumentId(string entityId)
    {
        return CreateClaimDocumentId(null, null, entityId);
    }

    public static string CreateGroupDocumentId(string userId, string groupId)
    {
        Requires.NotNull(userId, nameof(userId));
        Requires.NotNull(groupId, nameof(groupId));

        return $"Group|{userId}|{groupId}";
    }

    public static string CreateClaimPartition(string vehicleId, string userId, string entityId)
    {
        if ((vehicleId == null) && (userId == null))
        {
            return CreateEntityPartition(entityId);
        }
        else
        {
            return CreateVehicleUserPartition(vehicleId, userId);
        }
    }

    public static string CreateVehicleUserPartition(string vehicleId, string userId)
    {
        return $"ClaimP|{vehicleId}|{userId}";
    }

    public static string CreateEntityPartition(string entityId)
    {
        Requires.NotNull(entityId, nameof(entityId));
        return $"ClaimP|{entityId}";
    }

    public static string CreateGroupPartition(string userId)
    {
        Requires.NotNull(userId, nameof(userId));
        return $"GroupP|{userId}";
    }
}
