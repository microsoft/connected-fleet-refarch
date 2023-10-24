# Instructions for the Environment setup

1. Create a Resource Group in the Azure Portal 
2. Follow instructions in [Environment Configuration](./Environment_configuration/README.md)
3. Follow instructions in [Broker Configuration](./Broker_configuration/README.md)
4. Execute the main BICEP script using the instructions below to deploy the resource using the resource group name from Step 1

&nbsp;  

# BICEP Instructions

Execute the main.bicep file using the syntax:

``` bash
    az deployment group create --resource-group <ResourceGroupName> --template-file ./main.bicep 
```

For example:

    az deployment group create --resource-group TelemetryDev --template-file ./main.bicep
