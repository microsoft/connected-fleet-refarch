# Introduction 
This deployment directory contains the BICEP scripts to deploy the base Fleet Integration Layer

# Deployment Artifacts

The following resources are included in the deployment:

- Resource Group
- Application Insights
- Operational Insights
- App Service Plan
- Storage Account
- Logic App
- Event Hub Namespace (2 Event Hubs)
- Azure Data Explorer



# Instructions

1. Create a Resource Group in the Azure Portal to have the resources deployed into  

2. Execute the main.bicep file using the syntax:

        az deployment group create --resource-group <FleetResourceGroup> --template-file ./main.bicep 

    For example:

        az deployment group create --resource-group FleetLayerDev --template-file ./main.bicep 

