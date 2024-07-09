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


function generate_ca_cert_file()
{
    local file_path="${1}"
    local cert_value="\"-----BEGIN CERTIFICATE-----\\n${2}\\n-----END CERTIFICATE-----\""

    echo "{" >> $file_path
    echo "    \"encodedCertificate\": $cert_value" >> $file_path
    echo "}" >> $file_path
}

pushd ./cert-gen

# Check if the CA certificate exists. If it does, warn and exit
if [ -f ./certs/azure-mqtt-test-only.intermediate.cert ]; then
    warn_only_once
    exit 0
fi


./certGen.sh create_root_and_intermediate

./certGen.sh create_leaf_certificate_from_intermediate device01
./certGen.sh create_leaf_certificate_from_intermediate device02
./certGen.sh create_leaf_certificate_from_intermediate device03
./certGen.sh create_leaf_certificate_from_intermediate device04
./certGen.sh create_leaf_certificate_from_intermediate device05
./certGen.sh create_leaf_certificate_from_intermediate service01

popd


