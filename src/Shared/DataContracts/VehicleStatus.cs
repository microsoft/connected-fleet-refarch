// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class VehicleStatus : MessageBase
{
    public VehicleStatus(string vehicleId, DateTime timestamp, Version schemaVersion, GeoLocation? geoLocation) :
        base(vehicleId, timestamp, schemaVersion, geoLocation)
    { }

    /// <summary>
    /// Unique event ID if this vehicle status is associated to an event
    /// </summary>
    [JsonProperty("eventId", NullValueHandling = NullValueHandling.Ignore)]
    public string? EventId { get; set; }

    /// <summary>
    /// Contains a list of Signals and their associated values. The list of values depends on the protocols supported by the vehicle
    ///   - OBD-II
    ///   - J1939 / FMS
    ///   - Vehicle Signal Specification
    /// This data will be used for turn-key data analytics use cases
    /// </summary>
    /// <typeparam name="Signals">List of Signal Objects</typeparam>
    /// <returns></returns>
    [JsonProperty("signals", NullValueHandling = NullValueHandling.Ignore)]    
    public List<Signal>? Signals { get; set; }

    /// <summary>
    /// Contains additional values that are needed beyond the Signal list
    /// </summary>
    /// <typeparam name="Object">Object representing the extended properties</typeparam>
    /// <returns></returns>
    [JsonProperty("extendedProperties", NullValueHandling = NullValueHandling.Ignore)]
    public Object? ExtendedProperties { get; set; }
    
    /// <summary>
    /// Additional data from the hardware manufacturer / OEM. This data will be used by 3rd party applications.
    /// </summary>
    /// <typeparam name="Object">Object representing the additional properties</typeparam>
    /// <returns></returns>
    [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
    public Object? AdditionalProperties { get; set; }
}
