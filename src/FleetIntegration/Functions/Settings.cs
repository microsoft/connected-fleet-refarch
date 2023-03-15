// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.ConnectedFleet;

public static class Settings
{
    public static string EventEntityName { get { return GetSetting("EventEntity_EntityName"); } }
    public static string EventEntityDriver { get { return GetSetting("EventEntity_Driver"); } }
    public static string EventEntityDevice { get { return GetSetting("EventEntity_Device"); } }
    public static string EventEntityEventType { get { return GetSetting("EventEntity_EventType"); } }
    public static string EventEntityEventTime { get { return GetSetting("EventEntity_EventTime"); } }
    public static string EventEntityEventData{ get { return GetSetting("EventEntity_EventData"); } }
    public static string EventEntityAdditionalProperties { get { return GetSetting("EventEntity_AdditionalProperties"); } }
    public static string EventEntityEventDetails { get { return GetSetting("EventEntity_EventDetails"); } }

    public static string EventTypeEntityName { get { return GetSetting("EventType_EntityName"); } }
    public static string EventTypeEntityQueryColumn { get { return GetSetting("EventType_QueryColumn"); } }
    public static string EventTypeEntityKey { get { return GetSetting("EventType_EntityKey"); } }

    public static string DeviceEntityName { get { return GetSetting("Device_EntityName"); } }
    public static string DeviceEntityQueryColumn { get { return GetSetting("Device_QueryColumn"); } }
    public static string DeviceEntityKey { get { return GetSetting("Device_EntityKey"); } }

    public static string DriverEntityName { get { return GetSetting("Driver_EntityName"); } }
    public static string DriverEntityDriverName { get { return GetSetting("Driver_Name"); } }
    public static string DriverEntityQueryColumn { get { return GetSetting("Driver_QueryColumn"); } }
    public static string DriverEntityKey { get { return GetSetting("Driver_EntityKey"); } }

    public static string DataVerseSecret { get { return GetSetting("DataVerse_Secret"); } }
    public static string DataVerseAppId { get { return GetSetting("DataVerse_AppId"); } }
    public static string DataVerseUri { get { return GetSetting("DataVerse_Uri"); } }

    private static string GetSetting(string settingName)
    {
        return Environment.GetEnvironmentVariable(settingName);
    }
}