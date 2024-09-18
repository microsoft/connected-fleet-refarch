# Deploying the Fleet Integration Functions

This document describes the fleet integration functions.

![Deployment Diagram](FunctionDeploymentOverview.svg)

The Fleet Integration layer has one function

* VehicleEventHandler will process all device messages of type events and create an entry in the IoT Alert Table of the configured dataverse instance.

The function apps require the following configuration:

* Connection string to the event hub that stores messages with a Listen claim.
* Application id and secret with write access to the Dataverse instance
* Connection string (URI) to the dataverse

The setup procedure has 3 main steps:

1. Create an application rgistration in Entra ID with permissions to access the Common Data Service (Dataverse).
1. Add the application registration as a user in the Dataverse with the *Service Write* security role.
1. Configure and deploy the Function App.

## Prerequisites

* dotnet installed in the system
* Azure CLI installed
* Azure Functions Core Tools CLI installed

## Instructions

* Create an Application Registration with permissions to write to the Dataverse

  * Open Microsoft Entra ID in use by the Dataverse
  * Go to the App registrations blade
  * Click on *New Registration* on the toolbar
  * Use the following parameters to create the application registration
  
  | Key     | Value   |
  |---------|---------|
  | name | Dynamics Service Account App |
  | Who can use this app | Microsoft only - single tenant |
  
  * Once created, go to the API permissions blade and add the permission "Dynamics CRM - Access Common Data Service as organization users"
  * Go to the blade "Certificates and Secrets" and create a new Client Secret. Note the value and the secret id.

* Add the application registration as a registered user to the dataverse.

  * Go to the [PowerPlatform administration console](https://admin.powerplatform.microsoft.com/home)
  * Open the blade *Environments* and select your environment.
  * Open *settings* in the toolbar.
  * Open the entry *Users + permissions* / *Application Users*
  * Select *New App user* on the toolbar
  * Select your created application registration *DynamicsServiceAccountApp*
  * Select the business unit (your selected organiyation id)
  * Add the security role *Service Writer*

* Configure the event hubs shared access policies that will be used by the Function Apps to process the event messages. Configure a Listen policy.

You can list the Event Hub namespaces using the following command

```bash
    az resource list --resource-type "Microsoft.EventHub/namespaces" --output table
```

A sample output looks like this

```bash
Name              ResourceGroup                        Location    Type                           Status
----------------  -----------------------------------  ----------  -----------------------------  --------
eh-zzzzzzzzzzzzz  eg-fleetintegration                  eastus      Microsoft.EventHub/namespaces
eh-wwwwwwwwwwwww  eg-telemetryplatform                 eastus      Microsoft.EventHub/namespaces
```

You can use the following command to create the shared access policy in the Fleet Integration layer to listen for messages (replace the namespace name and the resource group with your values):

```bash
az eventhubs namespace authorization-rule create --name dynamicsintegration --namespace-name eh-zzzzzzzzzzzzz --resource-group eg-fleetintegration  --rights Listen
```

Use the following command to retrieve the primary connection string

```bash
az eventhubs namespace authorization-rule keys list --name dynamicsintegration --namespace-name eh-zzzzzzzzzzzzz --resource-group eg-fleetintegration
```

* Change directory to the Fleet Integration function app directory

```bash
    cd ./src/FleetIntegration/Functions
```

* Build the Functions from the terminal

```bash
    dotnet build
```

* Modify the configuration file. Open FunctionsConfig.json and add the following entries:

  | Key | Value |
  |-----|-------|
  | DataVerse_Secret | The Application registration secret value from the first step |
  | DataVerse_AppId | The Application registration id |
  | DataVerse_Uri | The uri of the dataverse

* Deploy the Functions to your Function app instance.

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

Please note the name of your function app "functions-xxxxxxxxxxxxx" in your *FleetIntegration* resource group. 

Set the configuration using the following command:

```bash
    az functionapp config appsettings set --resource-group rg-fleetintegration --name functions-xxxxxxxxxxxxx --settings @FunctionsConfig.json
```

Deploy the function app to your function app instance using the following command

```bash
    func azure functionapp publish functions-xxxxxxxxxxxxx --dotnet
```

After the command succeeds, you can check the functions deployed using the following command (replace name and resource group with your names)

```bash
     az functionapp function list --query "[].{name:name, resource:resourceGroup}" --name functions-yyyyyyyyyyyyy --resource-group rg-fleetintegration --output table
```

## Verification

* Use the Test Client to send Event Data. Event data will be stored in the dataverse as IoT Alerts.

```bash
    func azure functionapp logstream functions-xxxxxxxxxxxxx
```
