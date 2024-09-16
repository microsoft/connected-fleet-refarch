#r "Newtonsoft.Json"
#r "Microsoft.Azure.WebJobs"
#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

public static async Task Run(
    [EventGridTrigger]JObject eventGridEvent,
    ILogger log//,
    //IAsyncCollector<string> deadLetterEvents,
    //IAsyncCollector<string> vehicleStatusEvents,
    //IAsyncCollector<string> vehicleEventEvents
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