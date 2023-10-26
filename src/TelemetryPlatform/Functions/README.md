# Deploying the Telemetry Platform Functions

This document covers how to build, deploy and configure the Azure Functions for the Telemetry Platform.

The Telemetry Platform has two functions

* VehicleStatusHandler will process signals posted to the +/vehiclestatus topic
* VehicleEventHandler will process signals posted to the +/vehiclevent topic

The function apps require the following configuration

* Input: Routing from the Event Grid MQTT broker functionality to the Function App
* Output:
  * Exceptions are stored in a *deadletter* event hub
  * Events are routed to an event hub for alerts & events in the *Fleet Integration* Layer
  * Status updates are routed to an event hub for periodical status updates in the *Fleet Integration* Layer


## Prerequisites

* dotnet installed in the system
* Azure CLI installed
* Azure Functions Core Tools CLI installed

1. Change directory to the Telemetry Platform function app directory

```bash
    cd ./src/TelemetryPlatform/Functions
```

1. Build the Functions from the terminal

```bash
    dotnet build
```

1. Deploy the Functions to your Function app instance.

The following command shows you all of the function app resources deployed.

```bash
    az functionapp list --output table

```

A sample output looks like this

```bash
Name                     Location    State    ResourceGroup                        DefaultHostName                            AppServicePlan
-----------------------  ----------  -------  -----------------------------------  -----------------------------------------  ---------------------
functions-xxxxxxxxxxxxx  East US     Running  rg-fleetintegration                  functions-xxxxxxxxxxxxx.azurewebsites.net  appplan-xxxxxxxxxxxxx
functions-yyyyyyyyyyyyy  East US     Running  rg-telemetryplatform                 functions-yyyyyyyyyyyyy.azurewebsites.net  appplan-yyyyyyyyyyyyy

```

Please note the name of your function app "functions-yyyyyyyyyyyyy" in your *TelemetryPlatform* resource group. Deploy the function app to your function app instance using the following command

```bash
    func azure functionapp publish functions-yyyy --dotnet
```

After the command succeeds, you can check the functions deployed using the following command (replace name and resource group with your names)

```bash
     az functionapp function list --query "[].{name:name, resource:resourceGroup}" --name functions-yyyyyyyyyyyyy --resource-group rg-telemetryplatform --output table
```

1. Configure the consumption of messages from the event Event Grid Topic

  - Open your Event Grid Topic (by default: telemetryingestion) and go to the blade "Event Subscriptions"
  - Create an event subscription for vehicle status. Set the following parameters

    | Tab | Property | Value |
    |-----|------|-------|
    | Basics  | Name | vehiclestatus |
    | Basics  | Event Schema | Cloud Event Schema v1.0 |
    | Basics  | Endpoint type | Azure Function |
    | Basics  | Endpoint | Select your function app functions-yyyyyyyyyyyyy and the function VehicleStatusHandler |
    | Filters | Enable subject filtering | True |
    | Filters |Subject ends with | vehiclestatus |

  - Create an event subscription for vehicle events. Set the following parameters

    | Tab | Property | Value |
    |-----|------|-------|
    | Basics  | Name | vehicleevent |
    | Basics  | Event Schema | Cloud Event Schema v1.0 |
    | Basics  | Endpoint type | Azure Function |
    | Basics  | Endpoint | Select your function app functions-yyyyyyyyyyyyy and the function VehicleStatusHandler |
    | Filters | Enable subject filtering | True |
    | Filters |Subject ends with | vehicleevent |

1. Configure the event hubs endpoints. You can list the Event Hub namespaces using the following command

```bash

    az resource list --resource-type "Microsoft.EventHub/namespaces" --output table


```
    
    





