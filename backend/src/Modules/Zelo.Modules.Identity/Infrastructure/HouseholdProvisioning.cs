using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Infrastructure;

/// Garante a invariante "todo o utilizador autenticado tem sempre pelo
/// menos um household" (marcado como predefinido), partilhada entre
/// HouseholdEndpointHandlers.GetMyHouseholds (REST) e
/// HouseholdMembershipChecker.GetMyHouseholdsAsync (usado pelas tools
/// MCP) - as duas listagens de households de um utilizador que existem
/// no sistema. So criamos aqui, na primeira vez que a lista vem vazia,
/// porque o registo (MapIdentityApi) nao tem gancho para o fazer no
/// momento da conta.
internal static class HouseholdProvisioning
{
    public static async Task<List<HouseholdMember>> GetOrCreateMembershipsAsync(
        Guid userId, IdentityDbContext db, CancellationToken ct)
    {
        var memberships = await db.HouseholdMembers
            .Include(m => m.Household)
            .Where(m => m.UserId == userId)
            .ToListAsync(ct);

        if (memberships.Count > 0)
            return memberships;

        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = "A minha casa",
            CreatedAt = DateTimeOffset.UtcNow,
            IsDefault = true,
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

        memberships.Add(membership);
        return memberships;
    }
}
