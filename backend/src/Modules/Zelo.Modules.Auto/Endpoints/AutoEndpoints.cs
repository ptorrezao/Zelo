using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Zelo.ServiceDefaults;

namespace Zelo.Modules.Auto.Endpoints;

public static class AutoEndpoints
{
    // NOTA: householdId por query param - ver a mesma nota em CoreEndpoints.
    public static IEndpointRouteBuilder MapAutoEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/auto")
            .RequireAuthorization()
            .RequireFeatureFlag("auto-app-enabled");

        MapVehicles(group);
        MapMaintenances(group);
        MapDocuments(group);
        MapStats(group);

        return app;
    }

    private static void MapVehicles(RouteGroupBuilder group)
    {
        group.MapGet("/vehicles", AutoEndpointHandlers.GetVehicles);
        group.MapPost("/vehicles", AutoEndpointHandlers.CreateVehicle);
        group.MapGet("/vehicles/{id:guid}", AutoEndpointHandlers.GetVehicle);
        group.MapPut("/vehicles/{id:guid}", AutoEndpointHandlers.UpdateVehicle);
        group.MapDelete("/vehicles/{id:guid}", AutoEndpointHandlers.DeleteVehicle);
    }

    private static void MapMaintenances(RouteGroupBuilder group)
    {
        group.MapGet("/vehicles/{vehicleId:guid}/maintenances", AutoEndpointHandlers.GetMaintenances);
        group.MapPost("/vehicles/{vehicleId:guid}/maintenances", AutoEndpointHandlers.CreateMaintenance);
        group.MapGet("/maintenances/{id:guid}", AutoEndpointHandlers.GetMaintenance);
        group.MapPut("/maintenances/{id:guid}", AutoEndpointHandlers.UpdateMaintenance);
        group.MapDelete("/maintenances/{id:guid}", AutoEndpointHandlers.DeleteMaintenance);
    }

    private static void MapDocuments(RouteGroupBuilder group)
    {
        group.MapPost("/vehicles/{vehicleId:guid}/documents/upload-url", AutoEndpointHandlers.CreateUploadUrl);
        group.MapPost("/vehicles/{vehicleId:guid}/documents", AutoEndpointHandlers.CreateDocument);
        group.MapGet("/vehicles/{vehicleId:guid}/documents", AutoEndpointHandlers.GetDocuments);
        group.MapDelete("/documents/{id:guid}", AutoEndpointHandlers.DeleteDocument);
    }

    private static void MapStats(RouteGroupBuilder group)
    {
        group.MapGet("/vehicles/{vehicleId:guid}/stats", AutoEndpointHandlers.GetStats);
    }
}
