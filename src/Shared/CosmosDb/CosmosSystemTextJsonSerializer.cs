// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;

namespace Microsoft.Azure.ConnectedFleet.CosmosDb;

public class CosmosSystemTextJsonSerializer : CosmosSerializer
{
    private readonly JsonObjectSerializer systemTextJsonSerializer;

    public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        this.systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek
                    && stream.Length == 0)
            {
                return default;
            }

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            return (T)this.systemTextJsonSerializer.Deserialize(stream, typeof(T), default);
        }
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream streamPayload = new MemoryStream();
        this.systemTextJsonSerializer.Serialize(streamPayload, input, typeof(T), default);
        streamPayload.Position = 0;
        return streamPayload;
    }
}
