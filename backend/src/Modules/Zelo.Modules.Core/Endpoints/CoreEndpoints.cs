using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Zelo.ServiceDefaults;

namespace Zelo.Modules.Core.Endpoints;

public static class CoreEndpoints
{
    // householdId vem por query param, como em todo o resto da app -
    // RequireHouseholdMembership confirma que o utilizador autenticado
    // pertence de facto a esse household antes do handler correr.
    public static IEndpointRouteBuilder MapCoreEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/core")
            .RequireAuthorization()
            .RequireHouseholdMembership();

        group.MapGet("/assets", CoreEndpointHandlers.GetAssets);
        group.MapGet("/obligations", CoreEndpointHandlers.GetObligations);

        group.MapGet("/notifications", CoreEndpointHandlers.GetNotifications);
        group.MapPost("/notifications/{id:guid}/acknowledge", CoreEndpointHandlers.AcknowledgeNotification);
        group.MapGet("/notifications/preferences", CoreEndpointHandlers.GetPreferences);
        group.MapPut("/notifications/preferences", CoreEndpointHandlers.UpdatePreferences);

        return app;
    }
}
