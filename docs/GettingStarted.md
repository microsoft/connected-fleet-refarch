# Getting Started

## Pre-requisites

- An Azure Subscription with the necessary permissions to create resources.
- A Microsoft Dataverse Environment.
- A Linux or WSL2 Environment.
- Visual Studio Code.

## Setup Instructions

- Install WSL2 in your Windows computer
- Install dotnet in your WSL2 installation
- Install Visual Studio Code
- Check out the GitHub repository in your WSL environment

``` bash
git clone https://github.com/microsoft/connected-fleet-refarch.git
```

- Open Visual Studio Code from WSL using the argument

``` bash
cd connected-fleet-refarch
code .
```

- From the terminal, build the Azure Functions

``` bash
dotnet build src/FleetIntegration/Functions/
dotnet build src/TelemetryPlatform/Functions/
```

- From the terminal, build the Test Client
- 
``` bash
dotnet build src/TestClient/
`` 

- Install the following extensions in Visual Studio Code
  - [Bicep](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep): Bicep language support for Visual Studio Code
  - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions): quickly create, debug, manage, and deploy serverless apps directly from VS Code
  - [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client): to assist in updating
- Follow the instructions to deploy the infrastructure for each layer
  - [Telemetry Platform](../infra/deployment/TelemetryPlatform/README.md)
  - [Fleet Integration](../infra/deployment/FleetIntegration/README.md)

- Deploy the Azure Functions
- Use an MQTT Client or the Test Client to send test messages 

## DataVerse

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
