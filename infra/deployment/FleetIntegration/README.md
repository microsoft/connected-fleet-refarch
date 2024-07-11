# Setting up the Fleet Integration layer

In this step we will deploy the resources required for the fleet integration layer

## Deployment Artifacts

The following resources are included in the deployment:

- Event Hub Namespace (2 Event Hubs)
- Azure Data Explorer
- Function App
- App Service Plan
- Storage Account
- Application Insights
- Operational Insights

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
    az group create --name fleetintegration --location eastus
```

Execute the main.bicep refering to your resource group

``` bash
    az deployment group create --resource-group <ResourceGroupName> --template-file ./main.bicep 
```

For example:

``` bash
    az deployment group create --resource-group fleetintegration --template-file ./main.bicep
```
