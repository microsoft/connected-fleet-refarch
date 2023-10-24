# Instructions for the initial environment setup

Follow these steps to configure Azure CLI and set the common variables that will be used in the each scenario for deploying resources.

1. Make sure you have [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt) installed. Run `az --version` to verify. If it's not installed, run the following command to install it:

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

1. Navigate to the Environment_configuration folder in your cloned repo through `cd ./infra/deployment/TelemetryPlatform/Environment_configuration/`

1. Update profile.sh file to specify your subscription id, resource group name and AAD email address.

```bash
sub_id="<<your-subscription-id>>"
rg_name="<<your-resource-group-name>>"
ad_username="<<your-email@domain.com>>"
```

1. Make the scripts executable:

```bash
chmod 700 profile.sh
chmod 700 setupEnv.sh
```

1. Run the "profile.sh" script to set the common variables that will be used in the each scenario for deploying resources. Make sure you rerun  on every new shell window to set the right variables used in the scripts in the scenarios.

```bash
source profile.sh
```

1. Run the "setupEnv.sh"  script to create the root certificate that will be used in the sample scenarios and update the resources' configuration files (CAC_test-ca-cert.json). The script will also configure Azure CLI. A browser window will open to complete the login.

> **Warning**
> Make sure to run this script **only once** to avoid discrepancies between the generated certificates and the configuration files.

```bash
./setupEnv.sh
```
