// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using Validation;

namespace Microsoft.Azure.ConnectedFleet.CosmosDb;

public static class CosmosDbClientFactory
{
    private static CosmosClient cosmosClient;

    public static CosmosClient CreateClient(string connectionString)
    {
        if (cosmosClient != null)
        {
            return cosmosClient;
        }
        
        // Configure JsonSerializerOptions
        var opt = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Configure CosmosSystemTextJsonSerializer
        var serializer = new CosmosSystemTextJsonSerializer(opt);

        // Configure CosmosClientOptions
        var clientOptions = new CosmosClientOptions()
        {
            Serializer = serializer
        };

        cosmosClient = new CosmosClient(connectionString, clientOptions);

        return cosmosClient;
    }
}