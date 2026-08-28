using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.ServiceDefaults;

public static class FeatureFlagEndpoints
{
    /// Aplica-se a um grupo inteiro (ex.: MapGroup("/api/auto")). Devolve
    /// 404 em vez de 403 - a mesma logica que esconde a app na navegacao
    /// do frontend, a app "nao existe" quando a flag esta desligada.
    public static RouteGroupBuilder RequireFeatureFlag(this RouteGroupBuilder group, string flagName)
    {
        ArgumentNullException.ThrowIfNull(group);

        group.AddEndpointFilter(async (context, next) =>
        {
            var gate = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagGate>();
            if (!await gate.IsEnabledAsync(flagName, context.HttpContext.RequestAborted))
                return Results.NotFound();

            return await next(context);
        });

        return group;
    }
}
