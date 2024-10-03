// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedVehicle.Services.ClaimsManagement;

public abstract class ClaimsProviderDocument
{
    protected ClaimsProviderDocument(string id, string partitionKey)
    {
        this.Id = id;
        this.PartitionKey = partitionKey;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; }

    [JsonPropertyName("documentType")]
    public abstract string DocumentType { get; }
}

