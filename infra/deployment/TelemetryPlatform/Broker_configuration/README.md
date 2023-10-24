# Instructions for the MQTT broker setup

Follow these steps to configure the MQTT Broker

1. Make the script is executable:

```bash
chmod 700 create_resources.sh
```

1. Update create_resources.sh file to specify your broker namespace name and topic name.

```bash
ns_name="<<your-namespace>>"
eg_topic_name="<<your-topic-name>>"
```

1. Run the "./create_resources.sh" script to create the device certificates and the MQTT broker configuration
