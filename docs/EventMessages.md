
# Event Structure

The events have the following structure.

* eventId is a guid that uniquely identifies the event
* eventType is a the category of event - such as UserAlert, PredictiveMaintenance, etc.
* eventSubtype is a string that represents the subcategory of the event, and it is dependent on the event type.
* driverId identifies the driver using a unique string - this is optional and can be null.
* extendedProperties contains a list of name / value pairs that contain information extended specific to the eventType and eventSubtype.
* additionalProperties contains information about the message from the source system.
  * Source is the system that originates the eent
  * Type is the type of message (e.g. MQTT.EventPublished)
  * Time is the datetime when the event was published
* vehicleId contains the identification of the vehicle / device.
* timestamp contains the datetime where the message was generated.

```json
{
    "eventId": "6dc35950-0412-48e3-9211-cee273b4de71",
    "eventType": "AnEventTypeIdString",
    "eventSubType": "AnEventSubTypeIdString",
    "driverId": "AStringThatIdentifiesTheDriver",
    "extendedProperties": [
        {
            "name": "key1",
            "value": "value1"
        },
        {
            "name": "key2",
            "value": "value2"
        },
        {
            "name": "keyn",
            "value": "valuen"
        }
    ],
    "additionalProperties": {
        "Data": {},
        "Id": "3cf4a8fa-25b2-46cb-b9ad-bf1709786d17",
        "Source": "vehicletelemetry",
        "Type": "MQTT.EventPublished",
        "Time": "2023-12-12T12:08:41.6570000Z",
        "DataSchema": null,
        "DataContentType": null,
        "Subject": "device01.mqtt.contoso.com/vehicleevent",
        "ExtensionAttributes": {}
    },
    "vehicleId": "device01.mqtt.contoso.com",
    "timestamp": "2023-12-12T12:08:43.0525460Z",
    "schemaVersion": "1.0",
    "geoLocation": null,
}
```

## Event Types

We differenciate between events (no inmediate reaction required) and alerts (a reaction is expected - albeit not necessarily inmediately)

| Types of Events      | Sub Type         | Description     |
| ---------------------| -----------------| --------------- |
| BreakdownAlert       | UserRequest      | User requests breakdown services |
| MaintenanceAlert     | OilChange        | Oil change required |
| MaintenanceAlert     | TireRotation     | Indication that the tires should be rotated

### Breakdown Alerts

The following extended properties are optional for Breakdown requests (the type of error codes is dependent on the vehicle type).

* obd2ErrorCodes: A list of detected OBD-II error codes.
  * DTC as X0000 (e.g. P0746)
  * Name optional description of the error code (e.g."Pressure Control Solenoid Performance or Stuck Off")
  * Source the origin of the error code (usually an ECU or Subsystem) (e.g. "Transmission - gearbox control")

* udsErrorCodes: A list of detected UDS error codes.

*j1939ErrorCodes: A list of detected J1939 error codes:
  
* partNumbers: A list of relevant hardware part numbers. Each entry can contain:
  * HardwarePartNumber
  * HardwareVersion
  * SoftwarePartNumber
  * SoftwareVersion
  * SerialNumber

## Maintenance Alerts

The following extended properties are expected for Maintenance

* MaximumDueDate as date time: indicates the maximum date when the maintenance needs to be performed.
