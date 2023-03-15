// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

[assembly: FunctionsStartup(typeof(Microsoft.Azure.ConnectedFleet.Startup))]

namespace Microsoft.Azure.ConnectedFleet;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        ServiceClient svc = GetServiceClient();
        if (svc != null)
            builder.Services.AddSingleton<ServiceClient>((s) =>
            {
                return GetServiceClient();
            });
    }

    private ServiceClient GetServiceClient()
    {
        string SecretID = Settings.DataVerseSecret;
        string AppID = Settings.DataVerseAppId;
        string InstanceUri = Settings.DataVerseUri;

        string ConnectionStr = $@"AuthType=ClientSecret;
                                    SkipDiscovery=true;url={InstanceUri};
                                    Secret={SecretID};
                                    ClientId={AppID};
                                    RequireNewInstance=true";

        ServiceClient svc = new ServiceClient(ConnectionStr);

        if (svc.IsReady)
            return svc;
        else
            throw svc.LastException;
    }
}