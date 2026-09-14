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

        // Referencia estatica (marcas/modelos), nao dados de household -
        // sem RequireHouseholdMembership, so a autenticacao normal do grupo.
        group.MapGet("/vehicle-catalog", AutoEndpointHandlers.GetVehicleCatalog);

        // Grupo versionado (/api/v1/...) - so os endpoints de importacao
        // vivem aqui para ja: precisam de ser distinguiveis por versao,
        // porque sao chamados por outro deployment desta app que pode
        // ainda nao os ter (ver ImportRemoteClient). O resto do Auto fica
        // sem prefixo de versao por agora.
        var importGroup = app.MapGroup("/api/v1/auto")
            .RequireAuthorization()
            .RequireFeatureFlag("auto-app-enabled")
            .RequireHouseholdMembership();

        MapImport(importGroup);

        return app;
    }

    private static void MapVehicles(RouteGroupBuilder group)
    {
        // householdId (destino/filtro) so existe nestas duas - as de baixo
        // sao so por {id}, sem household nenhum a validar aqui (gap maior,
        // ver NOTA de seguranca no ficheiro de plano/PR).
        group.MapGet("/vehicles", AutoEndpointHandlers.GetVehicles).RequireHouseholdMembership();
        group.MapPost("/vehicles", AutoEndpointHandlers.CreateVehicle).RequireHouseholdMembership();
        group.MapGet("/vehicles/{id:guid}", AutoEndpointHandlers.GetVehicle);
        group.MapPut("/vehicles/{id:guid}", AutoEndpointHandlers.UpdateVehicle);
        group.MapDelete("/vehicles/{id:guid}", AutoEndpointHandlers.DeleteVehicle);
    }

    private static void MapImport(RouteGroupBuilder group)
    {
        group.MapPost("/vehicles/import/preview", AutoEndpointHandlers.PreviewImport);
        group.MapPost("/vehicles/import/confirm", AutoEndpointHandlers.ConfirmImport);
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
