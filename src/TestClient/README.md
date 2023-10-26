# MQTT Test Client

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
