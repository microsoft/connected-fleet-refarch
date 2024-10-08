# Getting Started

## Pre-requisites

### Overview

- An Azure Subscription with the necessary permissions to create resources and application registrations.
- A Linux or WSL2 Environment.
- A GitHub client.
- Visual Studio Code.
- (Optional) Docker
- (Optional) A Microsoft Dataverse Environment.
- (Optional) A Dynamics 365 installation with Field Service.

### Preparation

- If you don't have an Azure subscription, you can [create a free account](https://azure.microsoft.com/pricing/purchase-options/azure-account).

- Install the Windows Subsystem for Linux (WSL2) in your Windows computer or use a suitable Linux Distribution such as Ubuntu 22.04 LTS.

- Install dotnet-sdk-8.0. Read [Install .NET on Linux](https://learn.microsoft.com/dotnet/core/install/linux) and follow the steps for your distro.

- Install [Visual Studio Code](https://visualstudio.microsoft.com/) in your development computer.

- Make sure you have [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli-linux?pivots=apt) installed. Run `az --version` to verify. If it's not installed, run the following command to install it:

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

- Install the [Azure Functions core tools](https://github.com/Azure/azure-functions-core-tools/blob/v4.x/README.md#linux) to work with Azure functions.
- Check out the GitHub repository in your environment

``` bash
git clone https://github.com/microsoft/connected-fleet-refarch.git
```

- If using WSL, Open Visual Studio Code from WSL on the code directory

``` bash
cd connected-fleet-refarch
code .
```

- If you want to modify the function code, install the following extensions in Visual Studio Code to make your life easier.
  - [Bicep](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep): Bicep language support for Visual Studio Code
  - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions): quickly create, debug, manage, and deploy serverless apps directly from VS Code

- If you want to run the TestClient as a docker container, ensure that you have docker in your system. You can check if you have docker installed by running

``` bash
docker --version
```

## Deployment Steps

In the following deployment steps, you will:

- Deploy the telemetry platform layer, the fleet integration layer and the required azure functions
- Test connectivity and visualize results
- Clean-up resources



### 1. Deploying the telemetry platform layer

In this step you will:

- Create the test certificates for the Event Grid MQTT broker and test devices.
- Deploy all required Telemetry Platform resources to Azure using a Bicep script.
- Register 5 test devices to Event Grid as part of the Bicep script

#### 1.1 Create test certificates

- Change your directory to ```infra/deployment/TelemetryPlatform```

- Make the scripts executable:

```bash
chmod 700 generate-root-certificate.sh
chmod 700 generate-client-certificates.sh
chmod 700 ./cert-gen/certGen.sh
```

- Create a test root and intermediate certificate for the server, and create the certificates for the test clients.

> [!Warning]
> Make sure to run this script **only once** to avoid discrepancies between the generated certificates and the configuration files.

```bash
./generate-root-certificate.sh
./generate-client-certificates.sh
```

#### 1.3 Deploy the telemetry platform using Bicep

- login to your Azure account and select your subscription.

``` bash
az login
```
- export variables with your preferred resource group names and location.

``` bash
export RG_TELEMETRYPLATFORM=telemetryplatform
export RG_FLEETINTEGRATION=fleetintegration
export LOCATION=eastus
```

- [Create a resource group](https://learn.microsoft.com/cli/azure/manage-azure-groups-azure-cli#create-a-resource-group) for the deployment in a region

``` bash
az group create --name ${RG_TELEMETRYPLATFORM} --location ${LOCATION}
```

- Execute the main.bicep targeting your telemetry platform resource group. This step will take around 5 minutes to provision all necessary resources.

``` bash
az deployment group create --resource-group ${RG_TELEMETRYPLATFORM} --template-file ./main.bicep 
```

#### 1.3 Deploy the telemetry platform functions

* Change directory to ```./src/TelemetryPlatform/Functions```

* Export a variable with the name of the function app created in the telemetry platform

```bash
export tpfunctionapp=$(az functionapp list --query "[].name" --resource-group ${RG_TELEMETRYPLATFORM} --output tsv)
```

* Publish the functions to the function app

```bash
func azure functionapp publish ${tpfunctionapp} --dotnet
```

### 2. Deploy the fleet integration layer

In this step you will deploy the resources required for the fleet integration layer

#### 2.1 Execute fleet integration Bicep deployment scripts

- Change your directory to ```infra/deployment/FleetIntegration```

- [Create a resource group](https://learn.microsoft.com/cli/azure/manage-azure-groups-azure-cli#create-a-resource-group) for the deployment in a region

``` bash
az group create --name ${RG_FLEETINTEGRATION} --location ${LOCATION}
```

- Get the name of the event hub created in the telemetry platform.

``` bash
export tpeventhubname=$(az eventhubs namespace list --resource-group ${RG_TELEMETRYPLATFORM} --query "[].name" --output tsv)
```

- Execute the main.bicep refering to your resource group and using the event hub namespace from the telemetry platform as an argument. This step will take around 15 minutes to provision all necessary resources.

``` bash
az deployment group create --resource-group ${RG_FLEETINTEGRATION} --template-file ./main.bicep --parameters evhnsTelemetryPlatformNamespaceName=${tpeventhubname}
```

You can try out sending messages and the integration with Azure Data Explorer.

#### 2.2 Deploy the telemetry platform functions for connection to the dataverse

> [!NOTE]
> This step requires a Dynamics 365 installation with Field Service. You can skip this step in case you don't have an installation available.

Follow the instructions in [Dataverse Integration](DataverseIntegration.md) to automatically create ```IoT Event``` entities based on vehicle events.

## Try it out

### Send sample messages

The MQTT Test Client will send sample payloads using the generated certificates. The client will create 5 connections to the MQTT broker functionality
of Event Grid, and send a JSON payload every second per device.

The payloads can be found in the ./SamplePayloads directory. It consists of a JSON array that contains several values, harmonized using the
Vehicle Signal Specification (VSS) from COVESA.

Change your directory to ```./src/TestClient``` and build the client

```bash
dotnet build
```

To execute, set an environment variable with the MQTT host name and run using dotnet.

```bash
export gw_url=$(az eventgrid namespace list --resource-group ${RG_TELEMETRYPLATFORM} --query "[].topicSpacesConfiguration.hostname" --output tsv)
dotnet run
```

If you prefer to use containers, you can build the Test Client using:

```bash
docker build -t test-client-image -f Dockerfile ../..
```

> [!WARNING] 
> The image includes the client certificates. **Don't use these certificates in a production environment.**

Then use the following command to run locally.

```bash
docker run -it -e gw_url=${gw_url} --rm test-client-image
```

### Monitor the behaviour of the functions

You can connect to the Azure Functions log stream from the portal or by using the following command:

```bash
export tpfunctionapp=$(az functionapp list --query "[].name" --resource-group ${RG_TELEMETRYPLATFORM} --output tsv)
func azure functionapp logstream ${tpfunctionapp}
```

As the vehicle messages are sent to the device, the logstream will show the processing of each message and the routing. It is also possible to test the deadletter event hub by modifying ```src/TestClient/SamplePayloads``` to include a few invalid messages.

### Visualize the messages in Azure Data Explorer

You can query and visualize the messages in Azure Data explorer.

> [!NOTE]
> Default streaming ingestion configuration in Azure Data Explorer is 5 minutes.

* Open the Azure Portal
* Navigate to the Azure Data Explorer instance in your fleet integration resource group
* Go to Data > Query to open the Azure Data Explorer query interface

You can now use KQL statements to visualize the data, for example:

Visualize raw messages

``` kql
RawVehicleStatus
| take 100
```

Show the last position of the vehicles in a map

``` kql
VehicleStatusHarmonized
| summarize arg_max(timestamp, longitude, latitude) by vehicleId
| render scatterchart with (kind=map)
```

## Clean-up

If you no longer need the resources, use Azure CLI or the Azure Portal to delete the resource groups and its resources

```bash
az group delete --name ${RG_TELEMETRYPLATFORM}

az group delete --name ${RG_FLEETINTEGRATION}
```
