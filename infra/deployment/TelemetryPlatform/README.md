# Setting up the Telemetry Platform

In this step you will create the necessary test certificates for the Event Grid MQTT broker and deploy all required Telmemetry Platform resources to Azure.

## Deployment Artifacts

The following resources are included in the deployment:

- Event Grid
- Event Grid Topic
- Function App
- App Service plan
- Storage Account
- Event Hubs namespace
- Application Insights
- Operational Insights

## Create test certificates

1. Change your directory to infra/deployment/TelemetryPlatform

1. Make the scripts executable:

```bash
chmod 700 generate-certificates.sh
chmod 700 ./cert-gen/certGen.sh
```

> **Warning**
> Make sure to run this script **only once** to avoid discrepancies between the generated certificates and the configuration files.

```bash
./generate-certificates.sh
```

## Execute the biceps deployment scripts

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

Execute the main.bicep refering to your resource group

``` bash
    az deployment group create --resource-group <ResourceGroupName> --template-file ./main.bicep 
```

For example:

``` bash
    az deployment group create --resource-group telemetryplatform --template-file ./main.bicep
```
