#!/bin/bash

## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.

function warn_only_once()
{
    echo
    [ ! -z "$TERM" ] && tput smso
    [ ! -z "$TERM" ] && tput setaf 3
    echo "WARNING: This script should be run only once per your environment."
    echo "It creates test root certificates and updates some resource files for scenarios."
    echo "If the script is run more than once it could create some inconsistencies with resources,"
    echo "and they might need to be cleaned out."
    [ ! -z "$TERM" ] && tput sgr0
}

pushd ./cert-gen

# Check if the CA certificate exists. If it does, warn and exit
if [ -f ./certs/azure-mqtt-test-only.intermediate.cert ]; then
    warn_only_once
    exit 0
fi

./certGen.sh create_root_and_intermediate

popd


