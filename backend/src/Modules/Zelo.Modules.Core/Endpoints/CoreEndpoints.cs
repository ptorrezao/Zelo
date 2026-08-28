using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zelo.Modules.Core.Endpoints;

public static class CoreEndpoints
{
    // NOTA: householdId vem por query param porque a autenticacao ainda
    // nao esta ligada aos outros modulos (ver IdentityModule) - passar a
    // vir de um claim do utilizador autenticado assim que essa integracao
    // existir.
    public static IEndpointRouteBuilder MapCoreEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/core").RequireAuthorization();

        group.MapGet("/assets", CoreEndpointHandlers.GetAssets);
        group.MapGet("/obligations", CoreEndpointHandlers.GetObligations);

        return app;
    }
}
