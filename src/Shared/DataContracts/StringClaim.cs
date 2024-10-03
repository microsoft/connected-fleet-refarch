// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.ConnectedFleet.DataContracts;

//This class represents a claim in a simplified format
public class StringClaim
{
    public StringClaim(string name, List<ClaimValue> values)
    {
        this.Name = name;
        this.Values = values;
    }

    public StringClaim(string name, string value)
    {
        this.Name = name;
        this.Values = new List<ClaimValue>();
        this.Values.Add(new ClaimValue(value));
    }

    public StringClaim()
    {
        this.Values = new List<ClaimValue>();
    }

    /// <summary>
    ///     The Name of the claim (sometimes referred to as Type)
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     The Value of the claim represented by Name (multi-valued claims supported for compactness)
    /// </summary>
    [JsonPropertyName("values")]
    public List<ClaimValue>? Values { get; set; }
}

public class ClaimValue
{
    public ClaimValue(string value)
    {
        this.Value = value;
    }

    public ClaimValue()
    {
    }

    /// <summary>
    ///     The value of the claim
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}