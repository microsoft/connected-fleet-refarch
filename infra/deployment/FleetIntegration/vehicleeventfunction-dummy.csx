#r "Microsoft.Azure.WebJobs"
#r "Microsoft.Azure.WebJobs.Extensions.EventHubs"
#r "System.Threading.Tasks"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventHubs;
using System.Text;
using System.Threading.Tasks;

public static async Task Run(
    [EventHubTrigger("vehicleevent", Connection = "EventHubName", ConsumerGroup = "afvehicleeventscg")] string[] vehicleEvents,
    ILogger log)
{
    foreach (string eventData in vehicleEvents)
    {
        //string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

        // Process the message
        log.LogInformation($"C# Event Hub trigger function processed a message: {eventData}");

        await Task.Yield();
    }
}