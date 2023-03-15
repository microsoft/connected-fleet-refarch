// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.ConnectedFleet.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Microsoft.Azure.ConnectedFleet;

public class VehicleEventHandler
{
    private readonly string SourceName = nameof(VehicleEventHandler);

    private ServiceClient svc;
    private ILogger log;

    public VehicleEventHandler(ServiceClient serviceClient)
    {
        this.svc = serviceClient;
    }

    [FunctionName("VehicleEventHandler")]
    public async Task Run(
        [EventHubTrigger("vehicleevent",
            ConsumerGroup = "afvehicleeventscg",
            Connection = "VehicleEventEHConnectionString")]
        EventData[] vehicleEvents,
        ILogger log)
    {
        this.log = log;

        LogInformation("b828353e-fed2-43c3-9aeb-b1103944eecf", $"Vehicle Event Function Started Processing {vehicleEvents.Length.ToString()} Events");

        List<Task> events = new List<Task>();        
        foreach (EventData eventData in vehicleEvents)
        {
            events.Add(ProcessEvent(eventData));
        }
        await Task.WhenAll(events.ToArray());

        LogInformation("e46c67ad-b2e0-473b-9fe6-40d931ef297a", $"Vehicle Event Function Finished Processing {vehicleEvents.Length.ToString()} Events");
    }

    private async Task ProcessEvent(EventData eventData)
    {
        string content = Encoding.UTF8.GetString(eventData.EventBody);
        LogInformation("329d3a3a-ca9d-4fa5-9424-4df57dd1d3ee", content);

        var vehicleEvent = JsonConvert.DeserializeObject<VehicleEvent>(content);

        if (string.IsNullOrEmpty(vehicleEvent?.EventSubType))
        {
            LogWarning("d47ead71-b1cb-4b47-ada3-d3c084890d45", $"Invalid Event or SubType not found");
            return;
        }

        // Grab all the foreign keys necessary to create an event
        Task<Guid> eventTask = LookupEventTypeId(vehicleEvent.EventSubType);
        Task<Guid> deviceTask = LookupDeviceId(vehicleEvent.VehicleId);
        Task<Guid> driverTask = LookupDriverId(vehicleEvent.DriverId);
        
        await Task.WhenAll(eventTask, deviceTask, driverTask);

        Guid eventTypeId = eventTask.Result;
        Guid deviceId = deviceTask.Result;
        Guid driverId = driverTask.Result;

        // If any of the FKs are missing, exit
        if (eventTypeId == Guid.Empty ||
            deviceId == Guid.Empty ||
            driverId == Guid.Empty)
        {
            LogWarning("a453ed57-11f1-4c9a-9059-8f25b3632766", $"Failed to retrieve the required foreign keys");
            return;
        }

        try
        {
            // Create Event Entity
            Entity eventEntity = new Entity(Settings.EventEntityName);
            eventEntity[Settings.EventEntityDriver] = new EntityReference(Settings.DriverEntityName, driverId);
            eventEntity[Settings.EventEntityDevice] = new EntityReference(Settings.DeviceEntityName, deviceId);
            eventEntity[Settings.EventEntityEventType] = new EntityReference(Settings.EventTypeEntityName, eventTypeId);
            eventEntity[Settings.EventEntityEventTime] = vehicleEvent.Timestamp;
            eventEntity[Settings.EventEntityEventData] = vehicleEvent.EventId;

            if (vehicleEvent.AdditionalProperties != null)
                eventEntity[Settings.EventEntityAdditionalProperties] = JsonConvert.SerializeObject(vehicleEvent.AdditionalProperties);

            if (vehicleEvent.ExtendedProperties != null)
                eventEntity[Settings.EventEntityEventDetails] = JsonConvert.SerializeObject(vehicleEvent.ExtendedProperties);

            Guid eventId = svc.Create(eventEntity);
            LogInformation("55156ce9-d277-4566-8745-d80c8c0b90ac", $"Successfully created Event with DataVerse EventId: {eventId.ToString()}");
        }
        catch (Exception ex)
        {
            LogError(ex, "5f848711-a247-49bf-a789-243d7b9dc73f", "Failed to create Entity record");
        }
    }

    private async Task<Guid> LookupEventTypeId(string eventTypeName)
    {
        string entityName = Settings.EventTypeEntityName;
        string queryColumn = Settings.EventTypeEntityQueryColumn;
        string entityKey = Settings.EventTypeEntityKey;

        LogInformation("d85221cc-f14c-4718-937f-dc5d02f2bb44", $"Looking up Event SubType: {eventTypeName}");

        if (string.IsNullOrEmpty(eventTypeName))
        {
            LogWarning("0206d81f-a62b-4734-acea-211db68ae072", "EventTypeName field is empty");
            return Guid.Empty;
        }

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, eventTypeName, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("307b1381-bf2c-419a-8c82-563dd76fc94c", $"Could not find Event SubType: {eventTypeName}, creating new entity.");

            Entity eventTypeEntity = new Entity(entityName);
            eventTypeEntity[queryColumn] = eventTypeName;

            return svc.Create(eventTypeEntity);
        }
        else
        {
            LogInformation("8ae3feb4-9912-4eb4-96ea-166badd92909", $"Found Event SubType: {eventTypeName} with ID: {lookupEntity.Id.ToString()}");
            return lookupEntity.Id;
        }
    }

    private async Task<Guid> LookupDeviceId(string deviceId)
    {
        string entityName = Settings.DeviceEntityName;
        string queryColumn = Settings.DeviceEntityQueryColumn;
        string entityKey = Settings.DeviceEntityKey;

        LogInformation("2ef53cf5-4d98-4eda-9b7f-2ca259583454", $"Looking up Device ID: {deviceId}");

        if (string.IsNullOrEmpty(deviceId))
        {
            LogWarning("91a0f494-d0fc-4eb2-8161-8ce658e920f8", "DeviceId field is empty");
            return Guid.Empty;
        }

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, deviceId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("260ca992-3384-41c6-b03e-b8ab16ad0de6", $"Could not find Device ID: {deviceId}, creating new entity.");

            Entity deviceEntity = new Entity(entityName);
            deviceEntity[queryColumn] = deviceId;

            return svc.Create(deviceEntity);
        }
        else
        {
            LogInformation("d1162925-2c2d-4155-b781-97cb5fe209b1", $"Found Device ID: {deviceId} with ID: {lookupEntity.Id.ToString()}");
            return lookupEntity.Id;
        }
    }

    private async Task<Guid> LookupDriverId(string driverId)
    {
        string entityName = Settings.DriverEntityName;
        string queryColumn = Settings.DriverEntityQueryColumn;
        string entityKey = Settings.DriverEntityKey;
        string driverName = Settings.DriverEntityDriverName;

        LogInformation("3db379e4-7b99-43e8-8496-ae4ea5ee8ff4", $"Looking up Driver ID: {driverId}");

        if (string.IsNullOrEmpty(driverId))
        {
            LogWarning("735ccaef-93e6-4d07-b5d2-7892b61f314e", "DriverId field is empty");
            return Guid.Empty;
        }

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, driverId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("de14ab07-773a-4d39-8da1-c3988a2800e4", $"Could not find Driver ID: {driverId}, creating new entity.");

            Entity driverEntity = new Entity(entityName);
            driverEntity[queryColumn] = driverId;
            driverEntity[driverName] = driverId;

            return svc.Create(driverEntity);            
        }
        else
        {
            LogInformation("a7d5517f-b52a-4180-b3f7-3b66a6358335", $"Found Driver ID: {driverId} with ID: {lookupEntity.Id.ToString()}");
            return lookupEntity.Id;
        }
    }

    private async Task<Entity> LookupEntity(string entityName, string columnName, string columnValue, string entityColumnId)
    {
        ConditionExpression conditionExpression = new ConditionExpression(
            columnName, 
            ConditionOperator.Equal, 
            new string[] { columnValue }
        );

        FilterExpression filterExpression = new FilterExpression();
        filterExpression.Conditions.Add(conditionExpression);

        QueryExpression query = new QueryExpression(entityName)
        {
            ColumnSet = new ColumnSet(entityColumnId),
            Criteria = filterExpression,                
            TopCount = 1
        };                        

        EntityCollection results = await svc.RetrieveMultipleAsync(query);

        if(results?.Entities?.Count == 1)
            return results.Entities[0];
        else
            return null;
    }

    private void LogInformation(string tag, string message)
    {
        log.LogInformation($"Source: {SourceName}, Tag: {tag}, Message: {message}");
    }

    private void LogWarning(string tag, string message)
    {
        log.LogWarning($"Source: {SourceName}, Tag: {tag}, Message: {message}");
    }

    private void LogError(Exception ex, string tag, string message)
    {
        string logMessage = $"Source: {SourceName}, Tag: {tag}, Message: {message}";

        if (ex == null)
            log.LogError(logMessage);
        else
            log.LogError(ex, logMessage);
    }
}