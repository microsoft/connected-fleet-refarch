// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ConnectedFleet.DataContracts;
using Microsoft.Azure.ConnectedVehicle.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Azure.ConnectedVehicle;

public static class VehicleStatusHandler
{
    private static readonly string SourceName = nameof(VehicleStatusHandler);

    [FunctionName("VehicleStatusHandler")]
    public static async Task Run(
        [EventGridTrigger]CloudEvent eventGridEvent, 
        [EventHub("vehiclestatus", Connection = "VehicleStatusEventHubConnectionString")]IAsyncCollector<string> vehicleStatusEvents,
        [EventHub("deadletter", Connection = "TelemetryPlatformEventHubConnectionString")]IAsyncCollector<string> deadLetterEvents,
        ILogger log)
    {
        log.LogInformation($"VehicleStatusHandler Function Started Processing Event");

        try
        {
            // Grab the event data
            string content = eventGridEvent.Data.ToString();

            string vehicleId = GetVehicleIdFromSubject(eventGridEvent.Subject);
            if (vehicleId == null)
            {
                throw new ApplicationException("Unable to parse VehicleId from Subject");
            }

            // Deserialize into a Telemetry Message to validate format
            TelemetryMessage telemetryMessage = JsonConvert.DeserializeObject<TelemetryMessage>(content);

            // Serialze back into Json to make sure we only get clean data
            if (telemetryMessage != null)
            {
                VehicleStatus vehicleStatus = CreateVehicleStatus(vehicleId, telemetryMessage, eventGridEvent);
               
                log.LogInformation("Sending Message to VehicleStatus Event Hub");
               
                await vehicleStatusEvents.AddAsync(JsonConvert.SerializeObject(vehicleStatus));            
            }
            else
            {
                log.LogInformation("Invalid message received, sending to deadletter");

                DeadLetterMessage deadLetterMessage = new DeadLetterMessage()
                {
                    Source = SourceName,
                    Message = "Unable to serialize MQTT Data",
                    Tag = "16b6a4f2-6706-4e61-b5ca-0e8963b1f259",
                    Content = content,
                    AdditionalProperties = eventGridEvent,
                    Timestamp = DateTime.UtcNow
                };

                await deadLetterEvents.AddAsync(JsonConvert.SerializeObject(deadLetterMessage));
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to process EventGrid Message");

            DeadLetterMessage deadLetterMessage = new DeadLetterMessage()
                {
                    Source = SourceName,
                    Tag = "3e70aacf-effe-43e1-a406-3c5c7b752f00",
                    Message = $"Failed to process EventGrid Message: {ex.Message}",
                    ExceptionStackTrace = ex.StackTrace,
                    AdditionalProperties = eventGridEvent,
                    Timestamp = DateTime.UtcNow
                };
            
            await deadLetterEvents.AddAsync(JsonConvert.SerializeObject(deadLetterMessage));
        }
    }

    private static string GetVehicleIdFromSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject))
            return null;

        int seperatorPosition = subject.IndexOf('/');
        if (seperatorPosition <= 0)
            return null;

        return subject.Substring(0, seperatorPosition);
    }

    private static VehicleStatus CreateVehicleStatus(string vehicleId, TelemetryMessage telemetryMessage, CloudEvent eventGridEvent)
    {
        VehicleStatus vehicleStatus = new VehicleStatus
        (
            vehicleId,
            DateTime.UtcNow, 
            new Version(1, 0),
            null
        );

        vehicleStatus.Signals = ConvertVSSValueToSignal(telemetryMessage.Values);
        vehicleStatus.ExtendedProperties = telemetryMessage;
        vehicleStatus.AdditionalProperties = eventGridEvent;

        return vehicleStatus;
    }

    private static List<ConnectedFleet.DataContracts.Signal> ConvertVSSValueToSignal(List<VSSValue> values)
    {        
        if(values == null || values.Count == 0)
            return null;

        List<ConnectedFleet.DataContracts.Signal> signals = new List<ConnectedFleet.DataContracts.Signal>();
    
        foreach(VSSValue value in values)
        {
            if (value.DataPoints == null)
                continue;

            foreach (DataPoints datapoint in value.DataPoints)
            {
                ConnectedFleet.DataContracts.Signal signal = new ConnectedFleet.DataContracts.Signal();
                signal.Name = value.Path;
                signal.Value = datapoint.Value;
                signal.Timestamp = datapoint.Timestamp;
                signals.Add(signal);
            }            
        }

        return signals;
    }
}

