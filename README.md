# Microsoft connected fleets reference architecture

Welcome to the Microsoft Connected Fleet Reference Architecture

This sample code implements the concepts described in the [connected fleets reference architecture](https://learn.microsoft.com/azure/architecture/industries/automotive/automotive-connected-fleets)  in the Microsoft Learn website.

![High Level Architecture](./docs/images/high-level-architecture.svg)

The connected fleets reference archtiecture covers concepts such as:

- Telemetry platform to communicate to vehicles and devices
- Fleet integration to harmonize data and route messages to the right services
- Data analytics to create insights based on the vehicle and business data
- Business integration to provide customization using low-code / no-code
- Business operations for managing vehicles, devices, tasks and personnel.

The reference architecture also describes several integration points, where 3rd party applications can consume and share data.

## Available Capabilities

The initial release of the Connected Fleet Reference Architecture is focused on 3 key components:

![High Level Architecture](./docs/images/capabilities-overview.svg)

1. **Mobility Services Telemetry Platform** - a lightweight sample application to ingest MQTT data from the vehicle, parse and send to the Fleet Integration layer.   This sample is meant to be replaced with a fully functional Connected Vehicle/Fleet solution.

1. **Fleet Integration** - The ability to standardize the import of vehicle telemetry and vehicle events into Analytics and Business Integration. This integration enables seamless connectivity from the vehicle all the way to business integration, providing real-time insights into fleet performance and productivity.

1. **Integration with Dataverse, Power Platform and Dynamics 365 Field Service** - Leveraging the out of box entities of D365 Field Service to store IoT Devices, Assets and Alerts for vehicle events.

This sample code provides the following capabilities:

- Mobility Services Telemetry Platform.
  - Cloud based Event grid with the MQTT broker feature.
  - Azure Functions to process device to cloud messages and send Vehicle Events and Vehicle Status updates to the Fleet Integration Layer.
  - Generation of test certificates for the broker and devices
  - Deployment script templates to create the dependent components Azure.
  - Sample MQTT client that transmits telemetry for the test devices
- Fleet Integration.
  - Event Hub to receive Vehicle Events and Vehicle Status updates.
  - Azure Function to connect into Dataverse and persist Vehicle Event Data.
  - Azure Data Explorer service to save Vehicle Status updates and supporting Vehicle Event Data.
  - Azure Data Explorer Ingestion Function to parse incoming event hub data and persist into tables.
  - BICEP script templates to create the dependent components Azure.
- Dynamics 365 Field Service Integration.
  - Storing of Vehicle Event critical event data into D365 Field Service entities including IoT Assets, Devices and Alerts.

## Getting Started

To get started, please referring to the [Getting Started](./docs/GettingStarted.md) guide.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
