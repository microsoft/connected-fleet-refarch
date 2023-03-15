// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedVehicle.Models;

public class VSSValue
{
    /// <summary>
    /// VSS Dotnotated path to the attribute
    /// </summary>
    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string Path { get; set; }

    /// <summary>
    /// An array of values for the VSS attributed
    /// </summary>
    [JsonProperty("dp", NullValueHandling = NullValueHandling.Ignore)]
    public List<DataPoints> DataPoints { get; set; }
}

