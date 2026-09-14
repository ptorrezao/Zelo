using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        var households = await db.HouseholdMembers
            .Where(m => m.UserId == userId)
            .Select(m => new HouseholdResponse(m.HouseholdId, m.Household.Name, m.Role))
            .ToListAsync(ct);

        // Invariante: todo o utilizador autenticado tem sempre pelo menos
        // um household - o registo ainda nao cria nenhum (nenhum gancho
        // exposto pelo MapIdentityApi para isso), por isso criamos aqui,
        // na primeira vez que alguem pede a lista e ela vem vazia. Evita
        // que o utilizador fique preso sem destino nenhum para escolher
        // (ex.: no seletor "Importar para" da importacao de veiculos).
        if (households.Count == 0)
        {
            var response = await CreateHouseholdInternalAsync(userId, "A minha casa", db, ct);
            households.Add(response);
        }

        return Results.Ok(households);
    }

    public static async Task<IResult> CreateHousehold(HouseholdUpdateRequest request, ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200)
            return Results.BadRequest(new { error = "Nome do household inválido (entre 1 e 200 caracteres)." });

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var response = await CreateHouseholdInternalAsync(userId, name, db, ct);
        return Results.Created($"/api/v1/households/{response.Id}", response);
    }

    private static async Task<HouseholdResponse> CreateHouseholdInternalAsync(
        Guid userId, string name, IdentityDbContext db, CancellationToken ct)
    {
        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = household.Id,
            UserId = userId,
            Role = HouseholdRole.Owner,
            JoinedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);

        return new HouseholdResponse(household.Id, household.Name, HouseholdRole.Owner);
    }

    public static async Task<IResult> UpdateHousehold(
        Guid id, HouseholdUpdateRequest request, ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200)
            return Results.BadRequest(new { error = "Nome do household inválido (entre 1 e 200 caracteres)." });

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var membership = await db.HouseholdMembers
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.HouseholdId == id && m.UserId == userId, ct);

        // 404 para os dois casos (household inexistente vs. utilizador nao
        // e membro) - nao confirmar a um utilizador nao autorizado que um
        // household com aquele id sequer existe.
        if (membership is null)
            return Results.NotFound();

        // So o Owner pode renomear - impede que um Member futuro (quando
        // existir convite) altere as definicoes de outra pessoa.
        if (membership.Role != HouseholdRole.Owner)
            return Results.Forbid();

        membership.Household.Name = name;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new HouseholdResponse(membership.HouseholdId, membership.Household.Name, membership.Role));
    }
}
