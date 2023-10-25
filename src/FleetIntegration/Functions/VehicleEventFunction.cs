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
        Task<Guid> assetTask = UpsertAsset(vehicleEvent);
        Task<Guid> deviceTask = UpsertIoTDevice(vehicleEvent);
        
        await Task.WhenAll(assetTask, deviceTask);

        Guid assetId = assetTask.Result;
        Guid deviceId = deviceTask.Result;

        // If any of the FKs are missing, exit
        if (assetId == Guid.Empty ||
            deviceId == Guid.Empty)
        {
            LogWarning("7a8157c3-c1af-4435-8202-e59757abcc09", $"Failed to retrieve the required foreign keys");
            return;
        }

        try
        {
            // Create IoT Alert Entity
            Entity eventEntity = new Entity(Settings.IoTAlertEntityName);
            eventEntity[Settings.IoTAlertEntityAsset] = new EntityReference(Settings.AssetEntityName, assetId);
            eventEntity[Settings.IoTAlertEntityDevice] = new EntityReference(Settings.IoTDeviceEntityName, deviceId);
            eventEntity[Settings.IoTAlertEntityAlertTime] = vehicleEvent.Timestamp;
            eventEntity[Settings.IoTAlertEntityDescription] = $"{vehicleEvent.EventType} / {vehicleEvent.EventSubType}";
            eventEntity[Settings.IoTAlertEntityAlertData] = JsonConvert.SerializeObject(vehicleEvent);
                        
            Guid eventId = svc.Create(eventEntity);
            LogInformation("ece8b860-0523-4aea-bbde-45f79e903352", $"Successfully created IoT Alert with DataVerse EventId: {eventId.ToString()}");
        }
        catch (Exception ex)
        {
            LogError(ex, "7a8680f6-6f0b-4d4a-a3db-fd4d7ee127b8", "Failed to create IoT Alert record");
        }
    }

    private async Task<Guid> UpsertAsset(VehicleEvent vehicleEvent)
    {
        string entityName = Settings.AssetEntityName;
        string queryColumn = Settings.AssetEntityQueryColumn;
        string entityKey = Settings.AssetEntityKey;

        if (string.IsNullOrEmpty(vehicleEvent?.VehicleId))
        {
            LogWarning("cf572643-2de5-457c-9ec9-acf010201184", "vehicleId field is empty");
            return Guid.Empty;
        }

        LogInformation("238115fb-bb6e-4fb8-933d-abaabf2724cd", $"Looking up asset: {vehicleEvent.VehicleId}");

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, vehicleEvent.VehicleId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("655b4b59-5dd8-4e32-9650-87608d258594", $"Could not find asset: {vehicleEvent.VehicleId}, creating new entity.");

            Entity eventTypeEntity = new Entity(entityName);
            eventTypeEntity[queryColumn] = vehicleEvent.VehicleId;

            return svc.Create(eventTypeEntity);
        }
        else
        {
            LogInformation("f583482e-0cb5-4a39-89b1-eeb85b1867ac", $"Found asset: {vehicleEvent.VehicleId} with ID: {lookupEntity.Id.ToString()}");
            return lookupEntity.Id;
        }
    }

    private async Task<Guid> UpsertIoTDevice(VehicleEvent vehicleEvent)
    {
        string entityName = Settings.IoTDeviceEntityName;
        string queryColumn = Settings.IoTDeviceEntityQueryColumn;
        string entityKey = Settings.IoTDeviceEntityKey;

        if (string.IsNullOrEmpty(vehicleEvent?.VehicleId))
        {
            LogWarning("e84e1fff-c851-42cf-b11a-1468cc33e141", "vehicleId field is empty");
            return Guid.Empty;
        }

        LogInformation("8549ad66-b97e-434e-a0c9-206f84bea0f2", $"Looking up IoT Device: {vehicleEvent.VehicleId}");

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, vehicleEvent.VehicleId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("503a9820-41e9-40df-96b9-0c68437d19cb", $"Could not find IoT Device: {vehicleEvent.VehicleId}, creating new entity.");

            Entity eventTypeEntity = new Entity(entityName);
            eventTypeEntity[queryColumn] = vehicleEvent.VehicleId;

            return svc.Create(eventTypeEntity);
        }
        else
        {
            LogInformation("8aff87c1-a9d8-4102-943e-21f3390560f8", $"Found IoT Device: {vehicleEvent.VehicleId} with ID: {lookupEntity.Id.ToString()}");
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