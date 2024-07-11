#!/bin/bash

## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.

./certGen.sh create_leaf_certificate_from_intermediate device01
./certGen.sh create_leaf_certificate_from_intermediate device02
./certGen.sh create_leaf_certificate_from_intermediate device03
./certGen.sh create_leaf_certificate_from_intermediate device04
./certGen.sh create_leaf_certificate_from_intermediate device05
./certGen.sh create_leaf_certificate_from_intermediate service01

popd


