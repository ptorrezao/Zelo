using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zelo.SharedKernel;

namespace Zelo.ServiceDefaults;

public static class HouseholdEndpoints
{
    /// Le "householdId" da query string (convencao ja usada em todos os
    /// endpoints que recebem este parametro) e confirma que o utilizador
    /// autenticado pertence a esse household antes de deixar o handler
    /// correr. Generico em TBuilder para se aplicar tanto a um grupo
    /// inteiro (RouteGroupBuilder) como a uma rota individual
    /// (RouteHandlerBuilder), quando um grupo mistura rotas com e sem
    /// householdId - mesmo padrao que Microsoft.AspNetCore.Builder usa
    /// para RequireAuthorization.
    ///
    /// 403, nao 404 - ao contrario de RequireFeatureFlag, aqui nao se
    /// esconde que a rota existe, so se impede o acesso aos dados de outro
    /// household.
    public static TBuilder RequireHouseholdMembership<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilter(async (context, next) =>
        {
            var raw = context.HttpContext.Request.Query["householdId"].FirstOrDefault();
            if (!Guid.TryParse(raw, out var householdId))
                return Results.BadRequest(new { error = "householdId em falta ou inválido." });

            var userIdRaw = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdRaw, out var userId))
                return Results.Unauthorized();

            var checker = context.HttpContext.RequestServices.GetRequiredService<IHouseholdMembershipChecker>();
            if (!await checker.IsMemberAsync(userId, householdId, context.HttpContext.RequestAborted))
                return Results.Forbid();

            return await next(context);
        });

        return builder;
    }
}
