# Instructions for the MQTT broker setup

Follow these steps to configure the MQTT Broker

1. Navigate to the Broker_configuration folder in your cloned repo through `cd ./infra/deployment/TelemetryPlatform/Broker_configuration/`

1. Make the script is executable:

```bash
chmod 700 create_resources.sh
```

1. Update create_resources.sh file to specify your broker namespace name and topic name, if the defaults are not acceptable.

```bash
ns_name="vehicletelemetry" # Replace as desired
eg_topic_name="telemetryingestion" # Repalce as desired
```

1. Run the "./create_resources.sh" script to create the device certificates and the MQTT broker configuration
