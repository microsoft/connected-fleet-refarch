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

To build a container use

```bash
docker build -t test-client-image -f Dockerfile ../..
```

Note: we are passing the top level directory of the project, so we can include the generated certs automatically.

Then use the following command to run locally (make sure to pass your Event Grid MQTT broker url):

```bash
docker run -it -e gw_url="<yournamespace>-1.ts.eventgrid.azure.net" --rm test-client-image
```

You can push the image to a container registry and run it in an Azure Container Instance

```bash
docker tag test-client-image <yourregistry>.azurecr.io/test-client-image

docker push <yourregistry>.azurecr.io/test-client-image
```
