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

        return app;
    }
}
