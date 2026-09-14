using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Endpoints;

/// Primeiro modulo a expor endpoints - estabelece o padrao (nao documentado
/// ainda em docs/modules/module-contract.md, a acrescentar la): um metodo de
/// extensao MapXptoEndpoints(WebApplication) chamado so pelo host Api.
public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // /api/auth/login, /register, /refresh, /confirmEmail,
        // /resendConfirmationEmail, /forgotPassword, /resetPassword,
        // /manage/2fa, /manage/info.
        app.MapGroup("/api/auth").MapIdentityApi<ZeloUser>();

        // Primeiro endpoint versionado da app (/api/v1/...) - permite que um
        // ambiente remoto mais antigo (sem esta rota) seja detetado por 404
        // em vez de confundido com outro tipo de falha. Ver feature de
        // importacao de veiculos entre ambientes, no modulo Auto.
        app.MapGet("/api/v1/households/me", HouseholdEndpointHandlers.GetMyHouseholds).RequireAuthorization();
        app.MapPost("/api/v1/households", HouseholdEndpointHandlers.CreateHousehold).RequireAuthorization();
        app.MapPut("/api/v1/households/{id:guid}", HouseholdEndpointHandlers.UpdateHousehold).RequireAuthorization();

        return app;
    }
}
