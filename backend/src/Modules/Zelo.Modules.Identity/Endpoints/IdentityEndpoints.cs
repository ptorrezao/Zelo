using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        // Produces<T>() explicito em todos - o handler devolve Task<IResult>
        // (varios ramos: Ok/NotFound/Forbid/BadRequest), e a inferencia
        // automatica do schema OpenAPI para esse padrao mostrou-se fragil
        // (perdeu o tipo de varios endpoints do ficheiro de uma vez so por
        // causa de um handler novo sem Produces) - fixar o tipo aqui evita
        // depender dela.
        app.MapGet("/api/v1/households/me", HouseholdEndpointHandlers.GetMyHouseholds)
            .RequireAuthorization()
            .Produces<List<HouseholdResponse>>();
        app.MapPost("/api/v1/households", HouseholdEndpointHandlers.CreateHousehold)
            .RequireAuthorization()
            .Produces<HouseholdResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
        app.MapPut("/api/v1/households/{id:guid}", HouseholdEndpointHandlers.UpdateHousehold)
            .RequireAuthorization()
            .Produces<HouseholdResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        app.MapDelete("/api/v1/households/{id:guid}", HouseholdEndpointHandlers.DeleteHousehold)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // Nome do utilizador - o unico campo de perfil que o MapIdentityApi
        // (linha acima) nao cobre.
        app.MapGet("/api/v1/users/me", UserProfileEndpointHandlers.GetMyProfile)
            .RequireAuthorization()
            .Produces<UserProfileResponse>();
        app.MapPut("/api/v1/users/me", UserProfileEndpointHandlers.UpdateMyProfile)
            .RequireAuthorization()
            .Produces<UserProfileResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        return app;
    }
}
