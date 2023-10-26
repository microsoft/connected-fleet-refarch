// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.ConnectedFleet;

public static class Settings
{
    public static string AssetEntityName { get { return GetSetting("AssetEntity_EntityName"); } }
    public static string AssetEntityQueryColumn { get { return GetSetting("AssetEntity_Name"); } }
    public static string AssetEntityKey { get { return GetSetting("AssetEntity_CustomerAssetId"); } }

    public static string IoTDeviceEntityName { get { return GetSetting("IoTDeviceEntity_EntityName"); } }
    public static string IoTDeviceEntityQueryColumn { get { return GetSetting("IoTDeviceEntity_Name"); } }
    public static string IoTDeviceEntityKey { get { return GetSetting("IoTDeviceEntity_IoTDeviceId"); } }


    public static string IoTAlertEntityName { get { return GetSetting("IoTAlertEntity_EntityName"); } }
    public static string IoTAlertEntityAsset { get { return GetSetting("IoTAlertEntity_Asset"); } }
    public static string IoTAlertEntityDevice { get { return GetSetting("IoTAlertEntity_Device"); } }    
    public static string IoTAlertEntityAlertTime { get { return GetSetting("IoTAlertEntity_AlertTime"); } }    
    public static string IoTAlertEntityDescription { get { return GetSetting("IoTAlertEntity_Description"); } }    
    public static string IoTAlertEntityAlertData { get { return GetSetting("IoTAlertEntity_AlertData"); } }    


    public static string DataVerseSecret { get { return GetSetting("DataVerse_Secret"); } }
    public static string DataVerseAppId { get { return GetSetting("DataVerse_AppId"); } }
    public static string DataVerseUri { get { return GetSetting("DataVerse_Uri"); } }

    private static string GetSetting(string settingName)
    {
        return Environment.GetEnvironmentVariable(settingName);
    }
}