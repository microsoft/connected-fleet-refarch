// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class Signal
{
    public Signal()
    {

    }

    public Signal(string name, string value, DateTime timestamp)
    {
        this.Name = name;
        this.Value = value;
        this.Timestamp = timestamp;
    }

    [JsonProperty("name")]
    public string? Name { get; set; } 

    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

}