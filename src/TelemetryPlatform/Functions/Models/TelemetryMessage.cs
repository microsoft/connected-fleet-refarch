// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedVehicle.Models;

public class TelemetryMessage
{
    /// <summary>
    /// An Array of VSS data points
    /// </summary>
    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    public List<VSSValue> Values { get; set; }
}