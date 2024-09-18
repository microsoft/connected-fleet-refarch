# Telemetry Platform Functions

This document describes the Azure Functions for the Telemetry Platform. These functions process vehicle telemetry (status and events).

Events are defined by a [JSON message that contains an id, the type of event](../../../docs/EventMessages.md), and associated metadata.

![Deployment Diagram](FunctionDeploymentOverview.svg)

The Telemetry Platform has two functions

* VehicleStatusHandler will process signals posted to the +/vehiclestatus topic
* VehicleEventHandler will process signals posted to the +/vehiclevent topic

The function apps require the following configuration

* Input: Routing from the Event Grid MQTT broker functionality to the Function App
* Output:
  * Exceptions are stored in a *deadletter* event hub
  * Events are routed to an event hub for alerts & events in the *Fleet Integration* Layer
  * Status updates are routed to an event hub for periodical status updates in the *Fleet Integration* Layer
