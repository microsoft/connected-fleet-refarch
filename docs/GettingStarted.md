# Getting Started

## Pre-requisites

- An Azure Subscription with the necessary permissions to create resources and application registrations.
- A Microsoft Dataverse Environment.
- A Dynamics 365 installation with Field Service.
- A Linux or WSL2 Environment.
- Visual Studio Code.

## Preparation

- Install the Windows Subsystem for Linux (WSL2) in your Windows computer or a suitable Linux Distribution
- Install dotnet-sdk-6.0
- Install Visual Studio Code
- Make sure you have [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt) installed. Run `az --version` to verify. If it's not installed, run the following command to install it:

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

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

## Deployment

- Follow the instructions to deploy the infrastructure for each layer.
  - [Telemetry Platform](../infra/deployment/TelemetryPlatform/README.md) requires creating test certificates andcreating Azure resources for Event Grid, Event Grid Topics, Azure Functions and test devices.
  - [Fleet Integration](../infra/deployment/FleetIntegration/README.md) deploy the resources to process messages into analytics and business integration.

- Deploy and configure the Azure Functions
  - [Deploy and configure Telemetry Platform](../src/TelemetryPlatform/Functions/README.md) deploys and configures the message processing code
  - [Deploy and configure Fleet Integration](../src/FleetIntegration/Functions/README.md) deploys and configures the dataverse integration code

- Use an MQTT Client or the [Test Client](../src/TestClient/README.md) to send test messages.
