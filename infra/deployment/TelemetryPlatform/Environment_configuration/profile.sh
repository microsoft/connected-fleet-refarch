#!/bin/bash

## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.

export sub_id="<<your-subscription-id>>"
export rg_name="<<your-resource-group-name>>"
export az_region="centraluseuap"
export ad_username="<<your-email@domain.com>>"
export ns_name_suffix="${rg_name:0:10}-${sub_id:0:8}"
export base_type="Microsoft.EventGrid/namespaces"
export ns_id_prefix="/subscriptions/${sub_id}/resourceGroups/${rg_name}/providers/Microsoft.EventGrid/namespaces"

echo "Namespace prefix set to ${ns_id_prefix}"
