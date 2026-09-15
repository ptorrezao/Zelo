using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Infrastructure;

/// Logica de households partilhada entre HouseholdEndpointHandlers (REST)
/// e HouseholdMembershipChecker (usado pelas tools MCP do Auto) - as duas
/// superficies que listam/criam/renomeiam households de um utilizador.
internal static class HouseholdProvisioning
{
    /// Lanca ArgumentException se invalido - quem chama decide como
    /// traduzir isso (BadRequest no REST, McpException no MCP).
    public static string ValidateName(string name)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 200)
            throw new ArgumentException("Nome do household inválido (entre 1 e 200 caracteres).");

        return trimmed;
    }

    /// Garante a invariante "todo o utilizador autenticado tem sempre pelo
    /// menos um household" (marcado como predefinido) - so criamos aqui,
    /// na primeira vez que a lista vem vazia, porque o registo
    /// (MapIdentityApi) nao tem gancho para o fazer no momento da conta.
    public static async Task<List<HouseholdMember>> GetOrCreateMembershipsAsync(
        Guid userId, IdentityDbContext db, CancellationToken ct)
    {
        var memberships = await db.HouseholdMembers
            .Include(m => m.Household)
            .Where(m => m.UserId == userId)
            .ToListAsync(ct);

        if (memberships.Count > 0)
            return memberships;

        var membership = await CreateHouseholdAsync(userId, "A minha casa", isDefault: true, db, ct);
        memberships.Add(membership);
        return memberships;
    }

    public static async Task<HouseholdMember> CreateHouseholdAsync(
        Guid userId, string name, bool isDefault, IdentityDbContext db, CancellationToken ct)
    {
        var validName = ValidateName(name);

        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = validName,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDefault = isDefault,
        };
        var membership = new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = household.Id,
            UserId = userId,
            Role = HouseholdRole.Owner,
            JoinedAt = DateTimeOffset.UtcNow,
            Household = household,
        };

        db.Households.Add(household);
        db.HouseholdMembers.Add(membership);
        await db.SaveChangesAsync(ct);

        return membership;
    }

    /// Devolve null se o household nao existe ou o utilizador nao e
    /// membro (as duas coisas ficam indistinguiveis de proposito - nao
    /// confirmar a um utilizador nao autorizado que um household com
    /// aquele id existe). Lanca UnauthorizedAccessException se o
    /// utilizador e membro mas nao Owner - esse caso e diferente porque o
    /// utilizador ja sabe que o household existe, so nao pode renomea-lo.
    public static async Task<HouseholdMember?> RenameHouseholdAsync(
        Guid userId, Guid householdId, string name, IdentityDbContext db, CancellationToken ct)
    {
        var validName = ValidateName(name);

        var membership = await db.HouseholdMembers
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.HouseholdId == householdId && m.UserId == userId, ct);

        if (membership is null)
            return null;

        if (membership.Role != HouseholdRole.Owner)
            throw new UnauthorizedAccessException("Só o Owner pode renomear este household.");

        membership.Household.Name = validName;
        await db.SaveChangesAsync(ct);

        return membership;
    }
}
