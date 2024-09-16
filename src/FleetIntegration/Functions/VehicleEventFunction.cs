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

        LogInformation("vehicle-event-processing", $"Vehicle Event Function Started Processing {vehicleEvents.Length.ToString()} Events");

        List<Task> events = new List<Task>();        
        foreach (EventData eventData in vehicleEvents)
        {
            events.Add(ProcessEvent(eventData));
        }
        await Task.WhenAll(events.ToArray());

        LogInformation("vehicle-event-finished", $"Vehicle Event Function Finished Processing {vehicleEvents.Length.ToString()} Events");
    }

    private async Task ProcessEvent(EventData eventData)
    {
        string content = Encoding.UTF8.GetString(eventData.EventBody);
        LogInformation("vehicle-event-process", content);

        var vehicleEvent = JsonConvert.DeserializeObject<VehicleEvent>(content);

        if (string.IsNullOrEmpty(vehicleEvent?.EventSubType))
        {
            LogWarning("vehicle-event-subtype-invalid", $"Invalid Event or SubType not found");
            return;
        }

        // Grab all the foreign keys necessary to create an event
        Task<Guid> assetTask = UpsertAsset(vehicleEvent);
        Task<Guid> deviceTask = UpsertIoTDevice(vehicleEvent);
        
        await Task.WhenAll(assetTask, deviceTask);

        Guid assetId = await assetTask;
        Guid deviceId = await deviceTask;

        // If any of the FKs are missing, exit
        if (assetId == Guid.Empty ||
            deviceId == Guid.Empty)
        {
            LogWarning("vehicle-event-asset-invalid", $"Failed to retrieve the required foreign keys");
            return;
        }

        try
        {
            // Create IoT Alert Entity
            Entity eventEntity = new Entity(Settings.IoTAlertEntityName);
            eventEntity[Settings.IoTAlertEntityAsset] = new EntityReference(Settings.AssetEntityName, assetId);
            eventEntity[Settings.IoTAlertEntityDevice] = new EntityReference(Settings.IoTDeviceEntityName, deviceId);
            eventEntity[Settings.IoTAlertEntityAlertTime] = vehicleEvent.Timestamp;
            eventEntity[Settings.IoTAlertEntityAlertToken] = vehicleEvent.EventId;
            eventEntity[Settings.IoTAlertEntityDescription] = $"{vehicleEvent.EventType} / {vehicleEvent.EventSubType}";
            eventEntity[Settings.IoTAlertEntityAlertData] = JsonConvert.SerializeObject(vehicleEvent);
                        
            Guid eventId = svc.Create(eventEntity);
            LogInformation("vehicle-event-iot-alert-success", $"Successfully created IoT Alert with DataVerse EventId: {eventId.ToString()}");
        }
        catch (Exception ex)
        {
            LogError(ex, "vehicle-event-iot-alert-failure", "Failed to create IoT Alert record");
        }
    }

    private async Task<Guid> UpsertAsset(VehicleEvent vehicleEvent)
    {
        string entityName = Settings.AssetEntityName;
        string queryColumn = Settings.AssetEntityQueryColumn;
        string entityKey = Settings.AssetEntityKey;

        if (string.IsNullOrEmpty(vehicleEvent?.VehicleId))
        {
            LogWarning("vehicle-event-vehicleid-empty", "vehicleId field is empty");
            return Guid.Empty;
        }

        LogInformation("vehicle-event-asset-lookup", $"Looking up asset: {vehicleEvent.VehicleId}");

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, vehicleEvent.VehicleId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("vehicle-event-asset-lookup-failure", $"Could not find asset: {vehicleEvent.VehicleId}, creating new entity.");

            Entity eventTypeEntity = new Entity(entityName);
            eventTypeEntity[queryColumn] = vehicleEvent.VehicleId;

            return svc.Create(eventTypeEntity);
        }
        else
        {
            LogInformation("vehicle-event-asset-lookup-success", $"Found asset: {vehicleEvent.VehicleId} with ID: {lookupEntity.Id.ToString()}");
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
            LogWarning("vehicle-event-upsertiotdevice-vehicleid-empty", "vehicleId field is empty");
            return Guid.Empty;
        }

        LogInformation("vehicle-event-upsertiotdevice-iotdevice-lookup", $"Looking up IoT Device: {vehicleEvent.VehicleId}");

        Entity lookupEntity = await LookupEntity(entityName, queryColumn, vehicleEvent.VehicleId, entityKey);

        if (lookupEntity == null)
        {
            LogWarning("vehicle-event-upsertiotdevice-iotdevice-lookup-failure", $"Could not find IoT Device: {vehicleEvent.VehicleId}, creating new entity.");

            Entity eventTypeEntity = new Entity(entityName);
            eventTypeEntity[queryColumn] = vehicleEvent.VehicleId;

            return svc.Create(eventTypeEntity);
        }
        else
        {
            LogInformation("vehicle-event-upsertiotdevice-iotdevice-lookup-success", $"Found IoT Device: {vehicleEvent.VehicleId} with ID: {lookupEntity.Id.ToString()}");
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