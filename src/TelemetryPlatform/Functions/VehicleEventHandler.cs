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

public static class VehicleEventHandler
{
    private static readonly string SourceName = nameof(VehicleEventHandler);

    [FunctionName("VehicleEventHandler")]
    public static async Task Run(
        [EventGridTrigger]CloudEvent eventGridEvent, 
        [EventHub("vehiclestatus", Connection = "EventHubConnection")]IAsyncCollector<string> vehicleStatusEvents,
        [EventHub("vehicleevent", Connection = "EventHubConnection")]IAsyncCollector<string> vehicleEventEvents,
        [EventHub("deadletter", Connection = "EventHubConnection")]IAsyncCollector<string> deadLetterEvents,
        ILogger log)
    {
        log.LogInformation($"VehicleEventHandler Function Started Processing Event");

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
            EventMessage eventMessage = JsonConvert.DeserializeObject<EventMessage>(content);

            // Serialze back into Json to make sure we only get clean data
            if (eventMessage != null)
            {
                DateTime eventTime = DateTime.UtcNow;

                VehicleEvent vehicleEvent = CreateVehicleEvent(vehicleId, eventTime, eventMessage, eventGridEvent);
                VehicleStatus vehicleStatus = CreateVehicleStatus(vehicleId, eventTime, vehicleEvent.EventId, eventMessage, eventGridEvent);

                log.LogInformation("Sending Message to VehicleEvent Event Hub");
                log.LogInformation(JsonConvert.SerializeObject(vehicleEvent));
                await vehicleEventEvents.AddAsync(JsonConvert.SerializeObject(vehicleEvent));

                log.LogInformation("Sending Message to VehicleStatus Event Hub");
                log.LogInformation(JsonConvert.SerializeObject(vehicleStatus));
                await vehicleStatusEvents.AddAsync(JsonConvert.SerializeObject(vehicleStatus));
            }
            else
            {
                log.LogInformation("Invalid message received, sending to deadletter");

                DeadLetterMessage deadLetterMessage = new DeadLetterMessage()
                {
                    Source = SourceName,
                    Message = "Unable to serialize MQTT Data",
                    Tag = "mqtt-data-serialize-failed",
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
                    Tag = "eventgrid-message-process-failed",
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

    private static VehicleEvent CreateVehicleEvent(string vehicleId, DateTime eventTime, EventMessage eventMessage, CloudEvent eventGridMessage)
    {        
        Microsoft.Azure.ConnectedFleet.DataContracts.VehicleEvent vehicleEvent = 
            new Microsoft.Azure.ConnectedFleet.DataContracts.VehicleEvent
        (
            vehicleId,
            eventTime,             
            new Version(1, 0),
            null
        );

        vehicleEvent.DriverId = eventMessage.DriverId;
        vehicleEvent.EventType = eventMessage.Type;
        vehicleEvent.EventSubType = eventMessage.SubType;
        vehicleEvent.ExtendedProperties = eventMessage.ExtendedProperties;
        vehicleEvent.AdditionalProperties = eventGridMessage;

        return vehicleEvent;
    }   

    private static VehicleStatus CreateVehicleStatus(string vehicleId, DateTime eventTime, string eventId, EventMessage eventMessage, CloudEvent eventGridMessage)
    {        
        VehicleStatus vehicleStatus = new VehicleStatus(vehicleId, eventTime, new Version(1, 0), null);
        vehicleStatus.EventId = eventId;
        vehicleStatus.ExtendedProperties = eventMessage;
        vehicleStatus.AdditionalProperties = eventGridMessage;

        if (eventMessage.EventData?.Count > 0)
        {
            List<ConnectedFleet.DataContracts.Signal> signals = new List<ConnectedFleet.DataContracts.Signal>();

            foreach (EventData eventFrame in eventMessage.EventData)
            {
                if (eventFrame.Signals?.Count > 0)
                {
                    foreach (ConnectedVehicle.Models.Signal inputSignal in eventFrame.Signals)
                    {
                        ConnectedFleet.DataContracts.Signal signal = new ConnectedFleet.DataContracts.Signal();
                        signal.Name = inputSignal.SignalName;
                        signal.Value = inputSignal.SignalValue;
                        signal.Timestamp = eventFrame.Timestamp;
                        signals.Add(signal);
                    }
                }
            }

            if (signals.Count > 0)
                vehicleStatus.Signals = signals;
        }

        return vehicleStatus;
    }    
}

