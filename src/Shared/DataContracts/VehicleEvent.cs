// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedFleet.DataContracts;

public class VehicleEvent : MessageBase
{
   public VehicleEvent(string vehicleId, DateTime timestamp, Version schemaVersion, GeoLocation? geoLocation) :
        base(vehicleId, timestamp, schemaVersion, geoLocation)
    { }

    /// <summary>
    /// Unique ID for the Event
    /// </summary>
    [JsonProperty("eventId", NullValueHandling = NullValueHandling.Ignore)]
    public string EventId { get; set; } = Guid.NewGuid().ToString();


    /// <summary>
    /// Name of event.  e.g. ThresholdEvent
    /// </summary>
    [JsonProperty("eventType", NullValueHandling = NullValueHandling.Ignore)]
    public string? EventType { get; set; }

    /// <summary>
    /// The Event sub type.  e.g. HarshBreaking
    /// </summary>
    [JsonProperty("eventSubType", NullValueHandling = NullValueHandling.Ignore)]
    public string? EventSubType { get; set; } 

    /// <summary>
    /// The Driver when the event happened
    /// </summary>
    [JsonProperty("driverId", NullValueHandling = NullValueHandling.Ignore)]
    public string? DriverId { get; set; }
    
    /// <summary>
    /// Contains additional values. The list of values depends on the protocols supported by the vehicle
    ///   - OBD-II
    ///   - J1939 / FMS
    ///   - Vehicle Signal Specification
    /// This data will be used for turn-key data analytics use cases
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
