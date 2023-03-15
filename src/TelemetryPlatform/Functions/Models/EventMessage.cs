// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedVehicle.Models;

    public class EventData
    {
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("signals", NullValueHandling = NullValueHandling.Ignore)]
        public List<Signal> Signals { get; set; }
    }

    public class EventMessage
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("subType", NullValueHandling = NullValueHandling.Ignore)]
        public string SubType { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("driverId", NullValueHandling = NullValueHandling.Ignore)]
        public string DriverId { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("eventData", NullValueHandling = NullValueHandling.Ignore)]
        public List<EventData> EventData { get; set; }

        [JsonProperty("extendedProperties", NullValueHandling = NullValueHandling.Ignore)]
        public List<ExtendedProperties> ExtendedProperties { get; set; }
    }

    public class Signal
    {
        [JsonProperty("signal", NullValueHandling = NullValueHandling.Ignore)]
        public string SignalName { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string SignalValue { get; set; }
    }

    public class ExtendedProperties
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

