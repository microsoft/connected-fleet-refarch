// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;
    
namespace Microsoft.Azure.ConnectedVehicle.Models;

public class DeadLetterMessage
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source of the DeadLetter Message
    /// </summary>
    [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
    public string Source { get; set; }

    
    /// <summary>
    /// Unique tag identifing source location of deadletter message
    /// </summary>
    [JsonProperty("tag", NullValueHandling = NullValueHandling.Ignore)]
    public string Tag { get; set; }

    /// <summary>
    /// Message Describing the reason for deadlettering message
    /// </summary>
    [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
    public string Message { get; set; }

    /// <summary>
    /// Content that was the source of the dead letter message
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public Object Content { get; set; }

    /// <summary>
    /// The exception stack tracke
    /// </summary>
    [JsonProperty("exceptionStackTrace", NullValueHandling = NullValueHandling.Ignore)]
    public string ExceptionStackTrace { get; set; }

    
    /// <summary>
    /// Content that was the source of the dead letter message
    /// </summary>
    [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
    public Object AdditionalProperties { get; set; }
}

