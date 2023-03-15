// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedFleet.DataContracts;

/// <summary>
///     Represents a geographical location that is determined by latitude and longitude coordinates and may also include
///     altitude
/// </summary>
public class GeoLocation
{
    private double latitude;

    private double longitude;

    /// <summary>
    ///     Gets or sets the Altitude of the GeoLocation in meters.
    /// </summary>
    [JsonProperty("altitude")]
    public double? Altitude { get; set; }

    /// <summary>
    ///     Gets or sets the Latitude of the GeoLocation.
    /// </summary>
    [JsonProperty("latitude")]
    public double Latitude
    {
        get { return this.latitude; }

        set
        {
            if (value < -90 && value > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            this.latitude = value;
        }
    }

    /// <summary>
    ///     Gets or sets the Longitude of the GeoLocation.
    /// </summary>
    [JsonProperty("longitude")]
    public double Longitude
    {
        get { return this.longitude; }

        set
        {
            if (value < -180 && value > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            this.longitude = value;
        }
    }
}
