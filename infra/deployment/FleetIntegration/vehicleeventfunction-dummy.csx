// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#r "Microsoft.Azure.WebJobs"
#r "Microsoft.Azure.WebJobs.Extensions.EventHubs"
#r "System.Threading.Tasks"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventHubs;
using System.Text;
using System.Threading.Tasks;

/**
 * This is a dummy function to test the deployment of the function app.
 * The real function is located in the src/FleetIntegration/Functions/VehicleEventFunction.cs file and uses the information
 * about the event to add entries to Dataverse.
 * This function will just log the received messages.
 */
public static async Task Run(
    [EventHubTrigger("vehicleevent", Connection = "EventHubName", ConsumerGroup = "afvehicleeventscg")] string[] vehicleEvents,
    ILogger log)
{
    foreach (string eventData in vehicleEvents)
    {

        // Show the message
        log.LogInformation($"C# Event Hub trigger function processed a message: {eventData}");

        await Task.Yield();
    }
}