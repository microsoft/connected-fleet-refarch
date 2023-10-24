#!/bin/bash

## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.

ns_name="vehicletelemetry" #Replace by a custom name if desired
resource_prefix="${ns_id_prefix}/${ns_name}"
gw_url="${ns_name}.${az_region}-1.ts.eventgrid.azure.net"

eg_topic_name="telemetryingestion" # Replace by a custom topic name if desired
eg_topic_id="/subscriptions/${sub_id}/resourcegroups/${rg_name}/providers/Microsoft.EventGrid/topics/${eg_topic_name}"

pushd ../cert-gen
./certGen.sh create_leaf_certificate_from_intermediate device01
./certGen.sh create_leaf_certificate_from_intermediate device02
./certGen.sh create_leaf_certificate_from_intermediate device03
./certGen.sh create_leaf_certificate_from_intermediate device04
./certGen.sh create_leaf_certificate_from_intermediate device05
./certGen.sh create_leaf_certificate_from_intermediate service01
popd

echo "Setting up EventGrid topic."
az eventgrid topic create -g ${rg_name} --name ${eg_topic_name} -l ${az_region} --input-schema cloudeventschemav1_0 
az role assignment create --assignee ${ad_username} --role "EventGrid Data Sender" --scope "${eg_topic_id}"
az provider register --namespace Microsoft.EventGrid
echo "EventGrid topic created."

echo "Updating namespace resource file with EventGrid topic"
escaped_eg_topic_id=$(printf '%s\n' "$eg_topic_id" | sed -e 's/[\/&]/\\&/g')
sed -i "s/<<eg-topic-id>>/${escaped_eg_topic_id}/" ./resources/NS_CVBroker.json
echo "Namespace resource file updated."

echo "Uploading ${ns_name} resources..."

az resource create --resource-type ${base_type} --id ${resource_prefix} --is-full-object --api-version 2023-06-01-preview --properties @./resources/NS_CVBroker.json
az resource create --resource-type ${base_type}/caCertificates --id ${resource_prefix}/caCertificates/test-ca-cert --api-version 2023-06-01-preview --properties @./resources/CAC_test-ca-cert.json
az resource create --resource-type ${base_type}/topicSpaces --id ${resource_prefix}/topicSpaces/telemetrysub --api-version 2023-06-01-preview --properties @./resources/TS_TelemetrySub.json
az resource create --resource-type ${base_type}/topicSpaces --id ${resource_prefix}/topicSpaces/telemetrypub --api-version 2023-06-01-preview --properties @./resources/TS_TelemetryPub.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/device01 --api-version 2023-06-01-preview --properties @./resources/C_device01.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/device02 --api-version 2023-06-01-preview --properties @./resources/C_device02.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/device03 --api-version 2023-06-01-preview --properties @./resources/C_device03.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/device04 --api-version 2023-06-01-preview --properties @./resources/C_device04.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/device05 --api-version 2023-06-01-preview --properties @./resources/C_device05.json
az resource create --resource-type ${base_type}/clients --id ${resource_prefix}/clients/service01 --api-version 2023-06-01-preview --properties @./resources/C_service01.json
az resource create --resource-type ${base_type}/clientGroups --id ${resource_prefix}/clientGroups/allservices --api-version 2023-06-01-preview --properties @./resources/CG_allservices.json
az resource create --resource-type ${base_type}/clientGroups --id ${resource_prefix}/clientGroups/allvehicles --api-version 2023-06-01-preview --properties @./resources/CG_allvehicles.json
az resource create --resource-type ${base_type}/permissionBindings --id ${resource_prefix}/permissionBindings/sub-allservices --api-version 2023-06-01-preview --properties @./resources/PB_sub_allservices.json
az resource create --resource-type ${base_type}/permissionBindings --id ${resource_prefix}/permissionBindings/pub-allvehicles --api-version 2023-06-01-preview --properties @./resources/PB_pub_allvehicles.json

echo "Resources uploaded."

echo "Run the following in all shell windows before running python scripts:"
echo "export gw_url=${gw_url}"

