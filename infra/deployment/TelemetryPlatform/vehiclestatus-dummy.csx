// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#r "Newtonsoft.Json"
#r "Microsoft.Azure.WebJobs"
#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

/**
 * This is a dummy function to test the deployment of the function app.
 * The real function is located in src/TelemetryPlatform/Functions/VehicleEventHandler.cs
 * This function will just log the vehicle id (subject).
 */
public static async Task Run(
    [EventGridTrigger]JObject eventGridEvent,
    ILogger log
    )
{
    log.LogInformation(eventGridEvent.ToString());

    // Extract the data from the Event Grid event
    var eventData = eventGridEvent["data"];

    // Process the Event Grid event
    // For example, let's log the subject of the event
    var subject = eventGridEvent["subject"].ToString();
    log.LogInformation($"Subject: {subject}");
}