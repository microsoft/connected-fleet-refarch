// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedVehicle.Models;

public class DataPoints
{
    /// <summary>
    /// The VSS attribute value
    /// </summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public string Value { get; set; }

    /// <summary>
    /// The UTC timestamp of the VSS data point value
    /// </summary>
    [JsonProperty("ts", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime Timestamp { get; set; }
}
