using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Endpoints;

/// Handlers extraidos das lambdas inline de IdentityEndpoints para serem
/// testaveis diretamente contra IdentityDbContext, seguindo o mesmo padrao
/// do AutoEndpointHandlers.
internal static class HouseholdEndpointHandlers
{
    public static async Task<IResult> GetMyHouseholds(ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // HouseholdProvisioning garante a invariante "pelo menos um
        // household, marcado como predefinido" - evita que o utilizador
        // fique preso sem destino nenhum para escolher (ex.: no seletor
        // "Importar para" da importacao de veiculos), e da ao
        // DeleteHousehold um destino garantido para onde redirecionar os
        // itens de um household eliminado.
        var memberships = await HouseholdProvisioning.GetOrCreateMembershipsAsync(userId, db, ct);
        var households = memberships
            .Select(m => new HouseholdResponse(m.HouseholdId, m.Household.Name, m.Role, m.Household.IsDefault))
            .ToList();

        return Results.Ok(households);
    }

    public static async Task<IResult> CreateHousehold(HouseholdUpdateRequest request, ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        HouseholdMember membership;
        try
        {
            membership = await HouseholdProvisioning.CreateHouseholdAsync(userId, request.Name, isDefault: false, db, ct);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }

        var response = new HouseholdResponse(membership.HouseholdId, membership.Household.Name, membership.Role, membership.Household.IsDefault);
        return Results.Created($"/api/v1/households/{response.Id}", response);
    }

    public static async Task<IResult> UpdateHousehold(
        Guid id, HouseholdUpdateRequest request, ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        HouseholdMember? membership;
        try
        {
            membership = await HouseholdProvisioning.RenameHouseholdAsync(userId, id, request.Name, db, ct);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            // So o Owner pode renomear - impede que um Member futuro (quando
            // existir convite) altere as definicoes de outra pessoa.
            return Results.Forbid();
        }

        // 404 - nao confirmar a um utilizador nao autorizado que um
        // household com aquele id sequer existe (o utilizador nao e
        // membro, distinto do caso acima em que e membro mas nao Owner).
        if (membership is null)
            return Results.NotFound();

        return Results.Ok(new HouseholdResponse(membership.HouseholdId, membership.Household.Name, membership.Role, membership.Household.IsDefault));
    }

    public static async Task<IResult> DeleteHousehold(
        Guid id, ClaimsPrincipal user, IdentityDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var membership = await db.HouseholdMembers
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.HouseholdId == id && m.UserId == userId, ct);

        // 404 para os dois casos (household inexistente vs. utilizador nao
        // e membro) - mesmo raciocinio de UpdateHousehold.
        if (membership is null)
            return Results.NotFound();

        if (membership.Role != HouseholdRole.Owner)
            return Results.Forbid();

        // O predefinido e o destino de reatribuicao de qualquer outro
        // household eliminado - sem ele, um household eliminado nao teria
        // para onde mandar os seus veiculos/ativos/obrigacoes. Mantem
        // sempre a invariante de "pelo menos um household" tambem.
        if (membership.Household.IsDefault)
            return Results.BadRequest(new ErrorResponse("Não é possível remover o household predefinido."));

        var defaultMembership = await db.HouseholdMembers
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Household.IsDefault, ct);

        if (defaultMembership is null)
            return Results.BadRequest(new ErrorResponse("Não foi encontrado um household predefinido para redirecionar os itens."));

        db.Households.Remove(membership.Household); // cascade: HouseholdMembers deste household tambem saem
        await db.SaveChangesAsync(ct);

        await events.PublishAsync(
            new HouseholdDeleted(Guid.NewGuid(), DateTimeOffset.UtcNow, id, defaultMembership.HouseholdId), ct);

        return Results.NoContent();
    }
}
