# Getting Started

## Pre-requisites

- An Azure Subscription with the necessary permissions to create resources and application registrations.
- A Microsoft Dataverse Environment.
- A Dynamics 365 installation with Field Service.
- A Linux or WSL2 Environment.
- A GitHub client.
- Visual Studio Code.

## Deployment Steps

In the following deployment steps, you will:

- Install all the necessary pre-requisites
- Deploy the telemetry platform layer, the fleet integration layer and the required azure functions
- Test connectivity and visualize results
- Clean-up resources

### Preparation

- If you don't have an Azure subscription, you can [create a free account](https://azure.microsoft.com/en-us/pricing/purchase-options/azure-account).
- Install the Windows Subsystem for Linux (WSL2) in your Windows computer or use a suitable Linux Distribution such as Ubuntu 22.04 LTS.
- Install dotnet-sdk-8.0. Read [Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux) and follow the steps for your distro.
- Install [Visual Studio Code](https://visualstudio.microsoft.com/)
- Make sure you have [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt) installed. Run `az --version` to verify. If it's not installed, run the following command to install it:
- Install the [Azure Functions core tools](https://github.com/Azure/azure-functions-core-tools/blob/v4.x/README.md#linux) to work with Azure functions.

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

- Check out the GitHub repository in your environment

``` bash
git clone https://github.com/microsoft/connected-fleet-refarch.git
```

- If using WSL, Open Visual Studio Code from WSL on the code directory

``` bash
cd connected-fleet-refarch
code .
```

- To simplify development, install the following extensions in Visual Studio Code
  - [Bicep](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep): Bicep language support for Visual Studio Code
  - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions): quickly create, debug, manage, and deploy serverless apps directly from VS Code

### Deploying the telemetry platform layer

In this step you will:

- Create the test certificates for the Event Grid MQTT broker and test devices.
- Deploy all required Telemetry Platform resources to Azure using a biceps script.
- Register 5 test devices to Event Grid as part of the biceps script

#### Telemetry platform deployment artifacts

The following resources are included in the deployment:

- Event Grid
- Event Grid Topic
- Function App
- App Service plan
- Storage Account
- Event Hubs namespace
- Application Insights
- Operational Insights

#### Create test certificates

- Change your directory to ```infra/deployment/TelemetryPlatform```

- Make the scripts executable:

```bash
chmod 700 generate-root-certificate.sh
chmod 700 generate-client-certificates.sh
chmod 700 ./cert-gen/certGen.sh
```

- Generate the certificates

Create a test root and intermediate certificate.

> **Warning**
> Make sure to run this script **only once** to avoid discrepancies between the generated certificates and the configuration files.

```bash
./generate-root-certificate.sh
```

Create certificates for several test clients

```bash
./generate-client-certificates.sh
```

#### Execute the telemetry platform biceps deployment scripts

- login to your Azure account and select your subscription

``` bash
az login
```

- [Create a resource group](https://learn.microsoft.com/cli/azure/manage-azure-groups-azure-cli#create-a-resource-group) for the deployment in a region

``` bash
az group create --name <ResourceGroupName> --location <mylocation>
```

For example:

``` bash
az group create --name telemetryplatform --location eastus
```

- Execute the main.bicep refering to your resource group

``` bash
az deployment group create --resource-group <ResourceGroupName> --template-file ./main.bicep 
```

For example:

``` bash
az deployment group create --resource-group telemetryplatform --template-file ./main.bicep
```

#### Deploy the telemetry platform functions

* Change directory to the Telemetry Platform function app directory

```bash
cd ./src/TelemetryPlatform/Functions
```

* Deploy the Functions to your Function app instance.

The following command shows you all of the function app resources deployed.

```bash
az functionapp list --output table
```

A sample output looks like this

```bash
Name                     Location    State    ResourceGroup                        DefaultHostName                            AppServicePlan
-----------------------  ----------  -------  -----------------------------------  -----------------------------------------  ---------------------
functions-yyyyyyyyyyyyy  East US     Running  rg-telemetryplatform                 functions-yyyyyyyyyyyyy.azurewebsites.net  appplan-yyyyyyyyyyyyy

```

Please note the name of your function app "functions-yyyyyyyyyyyyy" in your *TelemetryPlatform* resource group. Deploy the function app to your function app instance using the following command

```bash
func azure functionapp publish <functions-yyyy> --dotnet
```

### Deploy the fleet integration layer

In this step you will deploy the resources required for the fleet integration layer

#### Fleet integration deployment artifacts

The following resources will be created as part of the deployment:

- Event Hub Namespace (2 Event Hubs)
- Azure Data Explorer
- Function App
- App Service Plan
- Storage Account
- Application Insights
- Operational Insights

#### Execute fleet integration biceps deployment scripts

- Change your directory to ```infra/deployment/FleetIntegration```

- [Create a resource group](https://learn.microsoft.com/cli/azure/manage-azure-groups-azure-cli#create-a-resource-group) for the deployment in a region

``` bash
az group create --name <ResourceGroupName> --location <mylocation>
```

For example:

``` bash
az group create --name fleetintegration --location eastus
```

- Execute the main.bicep refering to your resource group

``` bash
az deployment group create --resource-group <ResourceGroupName> --template-file ./main.bicep 
```

For example:

``` bash
az deployment group create --resource-group fleetintegration --template-file ./main.bicep
```

### Deploy the Azure functions

- [Deploy and configure Telemetry Platform](../src/TelemetryPlatform/Functions/README.md) deploys and configures the message processing code
- [Deploy and configure Fleet Integration](../src/FleetIntegration/Functions/README.md) deploys and configures the dataverse integration code

## Try it out

### Send sample messages

The MQTT Test Client will send sample payloads using the generated certificates. The client will create 5 connections to the MQTT broker functionality
of Event Grid, and send a JSON payload every second per device.

The payloads can be found in the ./SamplePayloads directory. It consists of a JSON array that contains several values, harmonized using the
Vehicle Signal Specification (VSS) from COVESA.

To build, use

```bash
dotnet build
```

To execute, use

```bash
dotnet run
```

If you prefer to use containers, you can build the Test Client in a container using:

```bash
docker build -t test-client-image -f Dockerfile ../..
```

> The command references the top level directory of the project, so we can include the generated certs automatically.

Then use the following command to run locally (make sure to pass your Event Grid MQTT broker url):

```bash
docker run -it -e gw_url="<yournamespace>-1.ts.eventgrid.azure.net" --rm test-client-image
```

You can push the image to a container registry and run it in an Azure Container Instance

```bash
docker tag test-client-image <yourregistry>.azurecr.io/test-client-image

docker push <yourregistry>.azurecr.io/test-client-image
```

### Monitor the behaviour of the azure functions

You can connect to the azure functions streams from the Portal or using  the following command:

```bash
func azure functionapp logstream <functions-yyyyyyyyyyyyy>
```

## Clean-up

If you no longer need the resources, use Azure CLI or the Azure Portal to delete the resource groups and its resources

```bash
az group delete --name <yourtelemetryplatformrgname>

az group delete --name <yourfleetintegrationrgname>

```

for example

```bash
az group delete --name telemetryplatform

az group delete --name fleetintegration
```
