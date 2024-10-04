// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.Azure.ConnectedFleet.CosmosDb;
using Microsoft.Azure.ConnectedFleet.DataContracts;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Validation;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

public class ClaimsProviderStore : IClaimsStore
{
    private CosmosClient _cosmosClient;
    private Database _database;
    private Container _container;

    public ClaimsProviderStore()
    {
        _cosmosClient = CosmosDbClientFactory.CreateClient(Environment.GetEnvironmentVariable("CosmosDbConnection__fullyQualifiedNamespace"), new DefaultAzureCredential());
        _database = _cosmosClient.GetDatabase(Environment.GetEnvironmentVariable("ClaimsDatabaseId"));
        _container = _database.GetContainer(Environment.GetEnvironmentVariable("ClaimsContainerId"));
    }

    public async Task<VehicleUserInfo> RefreshClaimsInfoAsync(string vehicleId, string userId, List<KeyValuePair<string, string>> claimsList)
    {
        List<StringClaim> claims = await this.LookupClaimsByVehicleAndUser(vehicleId, userId);
        
        return claims != null ? this.BuildVehicleUserInfo(vehicleId, userId, claims) : null;
    }

    public async Task<GroupVehicleInfo> RetrieveGroupVehicleInfoAsync(string groupId, string userId, string authToken, bool includeVehicles)
    {
        Requires.NotNull(groupId, nameof(groupId));
        Requires.NotNull(userId, nameof(userId));

        GroupVehicleInfo groupInfo = await this.LookupGroupVehicleAsync(groupId, userId, authToken, includeVehicles);

        return groupInfo;    
    }

    public async Task<List<StringClaim>> RetrieveClaimsByTypeAsync(ClaimTypeEnum claimType, string id)
    {
        Requires.NotNullOrEmpty(id, nameof(id));
        
        string documentId = null;
        string partitionKey = null;

        switch (claimType)
        {
            case ClaimTypeEnum.Vehicle:
                documentId = DocumentIdFactory.CreateClaimDocumentId(id, null, null);
                partitionKey = DocumentIdFactory.CreateClaimPartition(id, null, null);
                break;
            case ClaimTypeEnum.User:
                documentId = DocumentIdFactory.CreateClaimDocumentId(null, id, null);
                partitionKey = DocumentIdFactory.CreateClaimPartition(null, id, null);
                break;
            case ClaimTypeEnum.Service:
                documentId = DocumentIdFactory.CreateClaimDocumentId(null, null, id);
                partitionKey = DocumentIdFactory.CreateClaimPartition(null, null, id);
                break;
        }

        ClaimDocument entityRecord = await this._container.ReadItemAsync<ClaimDocument>(documentId, new PartitionKey(partitionKey));        
        if (entityRecord != null)
        {
            return entityRecord.Claims;
        }
        else
        {
            return null;
        }
    }

    public async Task<VehicleUserInfo> RetrieveVehicleUserInfoAsync(string vehicleId, string userId, string authToken)
    {
        List<StringClaim> claims = await this.LookupVehicleUserAsync(vehicleId, userId, authToken);

        return claims != null ? this.BuildVehicleUserInfo(vehicleId, userId, claims) : null;
    }

    public async Task AddClaimsAsync(string vehicleId, string userId, string serviceId, List<StringClaim> claims)
    {
        if (string.IsNullOrWhiteSpace(vehicleId) && string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(serviceId))
        {
            throw new ArgumentException("Must specify at least one of ServiceId, UserId, VehicleId");
        }

        vehicleId = this.VerifyAddClaimsInput(nameof(vehicleId), vehicleId);
        userId = this.VerifyAddClaimsInput(nameof(userId), userId);
        serviceId = this.VerifyAddClaimsInput(nameof(serviceId), serviceId);

        ClaimDocument document = new ClaimDocument(vehicleId, userId, serviceId, claims);
        await this._container.UpsertItemAsync(document, new PartitionKey(document.PartitionKey));   
    }

    public async Task RemoveClaimsAsync(string vehicleId, string userId, string serviceId)
    {
        vehicleId = this.VerifyAddClaimsInput(nameof(vehicleId), vehicleId);
        userId = this.VerifyAddClaimsInput(nameof(userId), userId);
        serviceId = this.VerifyAddClaimsInput(nameof(serviceId), serviceId);

        string documentId = DocumentIdFactory.CreateClaimDocumentId(vehicleId, userId, serviceId);
        string documentPartition = DocumentIdFactory.CreateClaimPartition(vehicleId, userId, serviceId);

        await this._container.DeleteItemAsync<ClaimDocument>(documentId, new PartitionKey(documentPartition));
    }
    
    private async Task<List<StringClaim>> LookupVehicleUserAsync(string vehicleId, string userId, string authToken)
    {
        Requires.NotNullOrEmpty(vehicleId, nameof(vehicleId));

        List<StringClaim> claimsList = new List<StringClaim>();
        bool isVehicleUserClaimsPresent = false;

        if (userId?.Length > 0)
        {
            // Aggregate the claims from Claims database for the VehicleId/UserId, VehicleId, UserId
            List<StringClaim> userVehicleClaims = await this.LookupClaimsByVehicleAndUser(vehicleId, userId);
            if (userVehicleClaims?.Count > 0)
            {
                isVehicleUserClaimsPresent = true;

                claimsList.AddRange(userVehicleClaims);
            }

            // If there are no claims at this point - there is no association of userId with the vehicleId
            if (claimsList.Count == 0)
            {
                return null;
            }

            // Claims associated with the user
            List<StringClaim> userClaims = await this.LookupClaimsByVehicleAndUser(null, userId);
            claimsList.AddRange(userClaims);
        }

        // Claims associated with the vehicle
        List<StringClaim> vehicleClaims = await this.LookupClaimsByVehicleAndUser(vehicleId, null);
        if (vehicleClaims?.Count > 0)
        {
            claimsList.AddRange(vehicleClaims);
        }
        else
        {
            // if there were no vehicle specific claims (either from Vehicle/User claims or Vehicle only claims), this means the Vehicle is not in the database so fail the request
            if ((vehicleClaims == null) && !isVehicleUserClaimsPresent)
            {
                return null;
            }
        }

        return claimsList;
    }    

    private async Task<List<StringClaim>> LookupClaimsByVehicleAndUser(string vehicleId, string userId)
    {
        try
        {
            string queryPartitionKey = DocumentIdFactory.CreateVehicleUserPartition(vehicleId, userId);
            string queryStatement = $"SELECT * FROM root r WHERE r.partitionKey = \"{queryPartitionKey}\"";

            List<StringClaim> claimsList = new List<StringClaim>();
        
            // Query multiple items from container
            FeedIterator<ClaimDocument> filteredFeed = this._container.GetItemQueryIterator<ClaimDocument>(queryStatement);

            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<ClaimDocument> response = await filteredFeed.ReadNextAsync();

                // Iterate query results
                foreach (ClaimDocument item in response)
                {
                    // If there is a entityId associated with this vehicle/user pair - add that entity name
                    if (item.EntityId != null)
                    {
                        List<StringClaim> entityClaimList = await this.LookupClaimsByEntityId(item.EntityId);
                        if (entityClaimList?.Count > 0)
                        {
                            claimsList.AddRange(entityClaimList);
                        }
                    }

                    // If there are additional claims, add them into the list
                    if (item.Claims?.Count > 0)
                    {
                        claimsList.AddRange(item.Claims);
                    }
                }
            }

            return claimsList.Count > 0 ? claimsList : null;
        }
        catch
        {
            //Instrument.Logger.LogException("00B311C6-ED83-42C9-98CE-CA2842588329", $"Error retrieving claims for user '{userId}' vehicle '{vehicleId}'", e);
            return null;
        }
    }    

    private async Task<GroupVehicleInfo> LookupGroupVehicleAsync(string groupId, string userId, string authToken, bool includeVehicles)
    {
        // Aggregate the claims from Claims database for the UserId/GroupId and authToken
        string documentId = DocumentIdFactory.CreateGroupDocumentId(userId, groupId);
        string partitionKey = DocumentIdFactory.CreateGroupPartition(userId);

        GroupDocument groupRecord = await this._container.ReadItemAsync<GroupDocument>(documentId, new PartitionKey(partitionKey));
        if (groupRecord == null)
        {
            return null;   // There is no group with the specified groupId for the userId
        }

        // Add additional claims
        // Claims associated with the user
        if (userId?.Length > 0)
        {
            List<StringClaim> userClaimsList = await this.LookupClaimsByVehicleAndUser(
                null,
                userId);
            if (userClaimsList?.Count > 0)
            {
                groupRecord.Claims.AddRange(userClaimsList);
            }
        }

        DateTimeOffset expiryTime = DateTimeOffset.UtcNow.AddMonths(1);   // Set to some extended period
        GroupVehicleInfo groupVehicleInfo = this.BuildGroupVehicleInfo(groupRecord, expiryTime, includeVehicles);

        return groupVehicleInfo;
    }
    
    private async Task<List<StringClaim>> LookupClaimsByEntityId(string entityId)
    {
        string documentId = DocumentIdFactory.CreateEntityClaimDocumentId(entityId);
        string partitionKey = DocumentIdFactory.CreateEntityPartition(entityId);

        ClaimDocument entityRecord = await this._container.ReadItemAsync<ClaimDocument>(documentId, new PartitionKey(partitionKey));        
        if (entityRecord != null)
        {
            return entityRecord.Claims;
        }
        else
        {
            return null;
        }
    }

    private GroupVehicleInfo BuildGroupVehicleInfo(GroupDocument groupRecord, DateTimeOffset expiryTime, bool includeVehicles)
    {
        GroupVehicleInfo groupVehicleInfo = new GroupVehicleInfo();
        groupVehicleInfo.UserId = groupRecord.UserId;
        groupVehicleInfo.GroupId = groupRecord.GroupId;
        groupVehicleInfo.ExpiryTime = expiryTime;
        groupVehicleInfo.Claims = groupRecord.Claims.ToDictionary(x => x.Name, y => new Collection<string>(y.Values.Select(z => z.Value).ToList()));
        groupVehicleInfo.Vehicles = new Collection<string>();
        if (includeVehicles)
        {
            foreach (string groupVehicle in groupRecord.Vehicles)
            {
                groupVehicleInfo.Vehicles.Add(groupVehicle);
            }
        }
        return groupVehicleInfo;
    }
    
    private VehicleUserInfo BuildVehicleUserInfo(string vehicleId, string userId, List<StringClaim> claims)
    {
        VehicleUserInfo vehicleUser = new VehicleUserInfo();
        vehicleUser.UserId = userId;
        vehicleUser.VehicleId = vehicleId;
        vehicleUser.ExpiryTime = DateTimeOffset.UtcNow.AddMonths(1);
        vehicleUser.Claims = this.ConvertToCollection(claims);
        return vehicleUser;
    }

    private Dictionary<string, Collection<string>> ConvertToCollection(List<StringClaim> allClaims)
    {
        Dictionary<string, Collection<string>> dictionaryClaims = new Dictionary<string, Collection<string>>();

        foreach (StringClaim claim in allClaims)
        {
            if (!dictionaryClaims.ContainsKey(claim.Name))
            {
                Collection<string> value = new Collection<string>();
                dictionaryClaims.Add(claim.Name, value);
            }
            foreach (ClaimValue cv in claim.Values)
            {
                dictionaryClaims[claim.Name].Add(cv.Value);
            }
        }

        return dictionaryClaims;
    }

    private string VerifyAddClaimsInput(string paramName, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return input;
    }
}
