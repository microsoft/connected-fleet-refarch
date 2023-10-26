# Getting Started

## Pre-requisites

- An Azure Subscription with the necessary permissions to create resources.
- A Microsoft Dataverse Environment.
- A Linux or WSL2 Environment.
- Visual Studio Code.

## Setup Instructions

- Install the Windows Subsystem for Linux (WSL2) in your Windows computer or a suitable Linux Distribution
- Install dotnet
- Install Visual Studio Code
- Check out the GitHub repository in your environment

``` bash
git clone https://github.com/microsoft/connected-fleet-refarch.git
```

- Open Visual Studio Code from WSL using the argument

``` bash
cd connected-fleet-refarch
code .
```

- Install the following extensions in Visual Studio Code
  - [Bicep](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep): Bicep language support for Visual Studio Code
  - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions): quickly create, debug, manage, and deploy serverless apps directly from VS Code
  - [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client): to assist in updating
  
- Follow the instructions to deploy the infrastructure for each layer
  - [Telemetry Platform](../infra/deployment/TelemetryPlatform/README.md) requires configuring the Event Grid MQTT broker feature, create resources that represent the devices and deploy function apps to process the vehicle messages
  - [Fleet Integration](../infra/deployment/FleetIntegration/README.md) deploy the resources to process messages into analytics and business integration

- Deploy and configure the Azure Functions
  - [Deploy and configure the Telemetry Platform](../src/TelemetryPlatform/Functions/README.md) deploys and configures the message processing code
  - [Deploy and configure the Fleet Integration](../src/FleetIntegration/Functions/README.md) deploys and configures the dataverse integration code

- Use an MQTT Client or the [Test Client](../src/TestClient/README.md) to send test messages.

## DataVerse

The Fleet Integration Layer is able to create entries in the Dataverse.

<!--

The Fleet Integration Layer has a dependency on the Microsoft Automotive Common Data Model.

- Configure a DataVerse Environment
- Import the [Automotive CDM](https://github.com/microsoft/Industry-Accelerator-Automotive/releases).   
  - Only the CDM component is needed (https://github.com/microsoft/Industry-Accelerator-Automotive/releases#:~:text=MicrosoftCommonDataModelforAutomotive_2_0_0_1_managed.zip)
  - An older version of the CRM Package Importer is needed to perform the import.  
- Run the [REST Client scripts](../src/Utils/RESTClient/DataVerse.http) to create the new entities. 
  - Replace all instances of "msauto_" to your Dataverse environment namespace (e.g. cr29e_).  This is needed in the short term until the ACDM is officially updated
- Update the Azure Functions configuration settins
  - Replace all the instances of "msauto_" in the [FunctionsConfig.json](../src/FleetIntegration/Functions/FunctionsConfig.json) to use your namespace
  - Update the DataVerse AppId, Secret and URI
  - Deploy the Functions Configuration by executing the following code

``` bash
    az functionapp config appsettings set -g <resourceGroup> -n <funcApp> --settings @FunctionsConfig.json
```

-->