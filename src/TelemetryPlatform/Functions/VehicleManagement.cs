// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.ConnectedFleet.DataContracts;
using Microsoft.Azure.ConnectedVehicle.Services.VehicleManagement;

namespace Microsoft.Azure.ConnectedVehicle;

public class VehicleManagement
{
    [OpenApiOperation(operationId: "provisionvehicle", tags: new[] { "Provision Vehicle" }, Summary = "Provision Vehicle", Description = "Provisions a vehicle", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ProvisionVehicleRequest), Required = true, Description = "Provisioning details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(Vehicle), Summary = "Provisioning Result", Description = "Returns the result of the provisioning request")]
    [FunctionName("ProvisionVehicle")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "vehicles")] HttpRequest req, ILogger _logger)
    {
        string content = await req.ReadAsStringAsync();
        ProvisionVehicleRequest request = JsonSerializer.Deserialize<ProvisionVehicleRequest>(content);
               
        bool isValid = EnsureRequestIsValid(request, out string errorMsg);
        if (!isValid)
        {
            _logger.LogError(errorMsg);
            return new BadRequestObjectResult(errorMsg);
        }

        VehicleManager vehicleManager = new VehicleManager();
        Vehicle response = await vehicleManager.ProvisionVehicleAsync(request);

        return new OkObjectResult(response);
    }

    [OpenApiOperation(operationId: "getvehicle", tags: new[] { "Get Vehicle" }, Summary = "Get Vehicle", Description = "Gets a vehicle", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "vehicleId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Vehicle Id", Description = "The Id of the vehicle to retrieve")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(Vehicle), Summary = "Vehicle", Description = "Returns the vehicle")]
    [FunctionName("GetVehicle")]
    public async Task<IActionResult> GetVehicleAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "vehicles/{vehicleId}")] HttpRequest req, string vehicleId, ILogger _logger)
    {
        VehicleManager vehicleManager = new VehicleManager();
        Vehicle vehicle = await vehicleManager.GetVehicleAsync(vehicleId);

        return new OkObjectResult(vehicle);
    }

    [OpenApiOperation(operationId: "getvehiclebyuuid", tags: new[] { "Get Vehicle By UUID" }, Summary = "Get Vehicle by UUID", Description = "Gets a vehicle by UUID", Visibility = OpenApiVisibilityType.Important)]  
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "Vehicle UUID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(Vehicle), Summary = "Vehicle", Description = "Returns the vehicle")]
    [FunctionName("GetVehicleByUuid")]
    public async Task<IActionResult> GetVehicleByUuidAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "vehicles/uuid")] HttpRequest req)
    {
        string vehicleUuid = await req.ReadAsStringAsync();
        VehicleManager vehicleManager = new VehicleManager();
        Vehicle vehicle = await vehicleManager.GetVehicleByUuidAsync(vehicleUuid);

        return new OkObjectResult(vehicle);
    }

    [OpenApiOperation(operationId: "rollvehicleid", tags: new[] { "Roll Vehicle Id" }, Summary = "Roll Vehicle Id", Description = "Rolls the vehicle Id", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "Vehicle UUID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(Vehicle), Summary = "Vehicle", Description = "Returns the updated vehicle")]
    [FunctionName("RollVehicleId")]
    public async Task<IActionResult> RollVehicleIdAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "vehicles/roll")] HttpRequest req)
    {
        string vehicleUuid = await req.ReadAsStringAsync();
        VehicleManager vehicleManager = new VehicleManager();
        Vehicle vehicle = await vehicleManager.RollVehicleIdAsync(vehicleUuid);

        return new OkObjectResult(vehicle);
    }

    private bool EnsureRequestIsValid(ProvisionVehicleRequest provisioningDetails, out string errorMsg)
    {
        errorMsg = string.Empty;
        if (provisioningDetails == null)
        {
            errorMsg = "Request body is empty";
            return false;
        }

        if (string.IsNullOrWhiteSpace(provisioningDetails.VehicleUuid))
        {
            errorMsg = "VehicleUuid is required";
            return false;
        }    

        if (provisioningDetails.Devices == null || provisioningDetails.Devices.Count == 0)
        {
            errorMsg = "At least 1 device is required";
            return false;
        }   

        foreach(var device in provisioningDetails.Devices)
        {
            if (string.IsNullOrWhiteSpace(device.DeviceId))
            {
                errorMsg = "DeviceId is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(device.DeviceName))
            {
                errorMsg = "DeviceName is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(device.CertificateCN))
            {
                errorMsg = "CertificateCN is required";
                return false;
            }
        }

        return true;
    }
}

