using Microsoft.EntityFrameworkCore;
using Zelo.SharedKernel;

namespace Zelo.Modules.Identity.Infrastructure;

internal sealed class HouseholdMembershipChecker(IdentityDbContext db) : IHouseholdMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) =>
        db.HouseholdMembers.AnyAsync(m => m.UserId == userId && m.HouseholdId == householdId, ct);

    public async Task<IReadOnlyList<HouseholdSummary>> GetMyHouseholdsAsync(Guid userId, CancellationToken ct = default)
    {
        var memberships = await HouseholdProvisioning.GetOrCreateMembershipsAsync(userId, db, ct);
        return memberships
            .Select(m => new HouseholdSummary(m.HouseholdId, m.Household.Name, m.Household.IsDefault))
            .ToList();
    }

    public async Task<HouseholdSummary> CreateHouseholdAsync(Guid userId, string name, CancellationToken ct = default)
    {
        var membership = await HouseholdProvisioning.CreateHouseholdAsync(userId, name, isDefault: false, db, ct);
        return new HouseholdSummary(membership.HouseholdId, membership.Household.Name, membership.Household.IsDefault);
    }

    public async Task<HouseholdSummary?> RenameHouseholdAsync(Guid userId, Guid householdId, string name, CancellationToken ct = default)
    {
        var membership = await HouseholdProvisioning.RenameHouseholdAsync(userId, householdId, name, db, ct);
        return membership is null
            ? null
            : new HouseholdSummary(membership.HouseholdId, membership.Household.Name, membership.Household.IsDefault);
    }

    public async Task<IReadOnlyList<string>> GetMemberEmailsAsync(Guid householdId, CancellationToken ct = default) =>
        await db.HouseholdMembers
            .Where(m => m.HouseholdId == householdId)
            .Join(db.Users, m => m.UserId, u => u.Id, (m, u) => u.Email)
            .Where(email => email != null)
            .Select(email => email!)
            .ToListAsync(ct);
}
