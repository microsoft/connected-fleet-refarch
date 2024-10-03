// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Microsoft.Azure.ConnectedVehicle;

internal class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Microsoft Connected Fleet Platform APIs",
        Description = "Sample APIs for the Microsoft Connected Fleet Platform",
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("http://opensource.org/licenses/MIT"),
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
